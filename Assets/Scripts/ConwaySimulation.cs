using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class ConwaySimulation : MonoBehaviour
{
    [Serializable]
    public struct StaticConfiguration
    {
        public int seed;
        public int spawnProbability;
        public int width;
        public int height;
        public int depth;
        public int sumRange;
    }

    [Serializable]
    public struct DynamicConfiguration
    {
        public float cellSize;
        public float spacing;
        public float simulationTickRate;
        public int minPopulationCutoff;
        public int maxPopulationThreshold;
        public int adjanceLiveCellCountForRevival;
    }

    public int width { get; private set; }
    public int height { get; private set; }
    public int depth { get; private set; }
    public int maxCount { get; private set; }
    public Vector3 boundsSize { get; private set; }
    public Vector3 center { get; private set; }
    public int generationCount { get; private set; }
    public int aliveCellsCount { get; private set; }
    public float spacing => m_dynamicConfiguration.spacing;
    public NativeArray<int> states => m_statesCopy;
    public float cellSize => m_dynamicConfiguration.cellSize;

    private DynamicConfiguration m_dynamicConfiguration => ConwaySimulationConfigHolder.Instance.dynamicConfiguration;
    private StaticConfiguration m_staticConfiguration;
    private NativeArray<int> m_states;
    private NativeArray<int> m_statesCopy;
    private NativeArray<int>[] m_sums;
    private NativeArray<JobHandle> m_sumJobs;
    private JobHandle m_conJobHandle;
    private float m_simulationTime;

    private void Awake()
    {
        m_staticConfiguration = ConwaySimulationConfigHolder.Instance.staticConfiguration;

        UnityEngine.Random.InitState(m_staticConfiguration.seed);

        width = m_staticConfiguration.width;
        height = m_staticConfiguration.height;
        depth = m_staticConfiguration.depth;
        maxCount = width * height * depth;
        m_states = new NativeArray<int>(maxCount, Allocator.Persistent);
        m_statesCopy = new NativeArray<int>(maxCount, Allocator.Persistent);

        for (int index = 0; index < maxCount; index++)
        {
            m_states[index] = UnityEngine.Random.Range(0, m_staticConfiguration.spawnProbability) == 0 ? 1 : 0;
            m_statesCopy[index] = m_states[index];
        }

        var sumJobCount = maxCount / m_staticConfiguration.sumRange;
        sumJobCount = sumJobCount <= 0 ? 1 : sumJobCount;
        m_sumJobs = new NativeArray<JobHandle>(sumJobCount, Allocator.Persistent);
        m_sums = new NativeArray<int>[sumJobCount];
        for (var i = 0; i < sumJobCount; i++)
        {
            m_sums[i] = new NativeArray<int>(1, Allocator.Persistent);
        }

        UpdateBounds();
    }

    private void Update()
    {
        m_simulationTime += Time.deltaTime;
        if (m_simulationTime >= m_dynamicConfiguration.simulationTickRate)
        {
            m_simulationTime = 0;
            ExecuteJobs(); //TODO defer
        }

        UpdateBounds();
    }

    private void UpdateBounds()
    {
        var whd = new Vector3(width, height, depth) * m_dynamicConfiguration.cellSize;
        var whds = new Vector3(width - 1, height - 1, depth - 1) * m_dynamicConfiguration.spacing;
        boundsSize = whd + whds;
        center = boundsSize / 2;
    }

    private void ExecuteJobs()
    {
        m_conJobHandle.Complete(); //Complete previous Job

        m_states.CopyTo(m_statesCopy);

        var sumsCount = m_sums.Length;
        for (var i = 0; i < sumsCount; i++)
        {
            var slice = new NativeSlice<int>(m_statesCopy, i * m_staticConfiguration.sumRange, m_staticConfiguration.sumRange);
            var sumJob = new SumJob
            {
                states = slice,
                sums = m_sums[i],
            };
            m_sumJobs[i] = sumJob.Schedule(m_sums[i].Length, 64);
        }

        JobHandle.CompleteAll(m_sumJobs);

        var aliveCellsCountTemp = 0;
        for (var i = 0; i < m_sums.Length; i++)
        {
            aliveCellsCountTemp += m_sums[i][0];
        }
        aliveCellsCount = aliveCellsCountTemp;

        var conJob = new ConJob
        {
            width = m_staticConfiguration.width,
            height = m_staticConfiguration.height,
            depth = m_staticConfiguration.depth,
            minPopulationCutoff = m_dynamicConfiguration.minPopulationCutoff,
            maxPopulationThreshold = m_dynamicConfiguration.maxPopulationThreshold,
            states = m_states,
            statesCopy = m_statesCopy,
            reproductionStateCount = m_dynamicConfiguration.adjanceLiveCellCountForRevival
        };
        m_conJobHandle = conJob.Schedule(maxCount, 64);
        generationCount++;
    }

    private void OnDestroy()
    {
        m_conJobHandle.Complete();

        m_states.Dispose();
        m_statesCopy.Dispose();
        m_sumJobs.Dispose();

        for (var i = 0; i < m_sums.Length; i++)
        {
            m_sums[i].Dispose();
        }
    }

    [BurstCompile]
    public struct SumJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeSlice<int> states;
        public NativeArray<int> sums;

        public void Execute(int index)
        {
            int sum = 0;
            for (int i = 0; i < states.Length; i++)
            {
                sum += states[i];
            }
            sums[index] = sum;
        }
    }

    [BurstCompile]
    public struct ConJob : IJobParallelFor
    {
        [ReadOnly]
        public int width;
        [ReadOnly]
        public int height;
        [ReadOnly]
        public int depth;
        [ReadOnly]
        public int minPopulationCutoff;
        [ReadOnly]
        public int maxPopulationThreshold;
        [ReadOnly]
        public int reproductionStateCount;
        [ReadOnly]
        public NativeArray<int> statesCopy;
        public NativeArray<int> states;

        public void Execute(int index)
        {
            int validAndAliveAdjacentIndicesCount = 0;
            int k = index / (width * height);
            int remainder = index % (width * height);
            int j = remainder / width;
            int i = remainder % width;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        if (dx == 0 && dy == 0 && dz == 0)
                            continue;

                        int iA = i + dx;
                        int jA = j + dy;
                        int kA = k + dz;
                        var isValidCoordinate = iA >= 0 && iA < width && jA >= 0 && jA < height && kA >= 0 && kA < depth;
                        if (!isValidCoordinate)
                            continue;

                        int adjacentIndex = iA + jA * width + kA * width * height;
                        int adjacentState = statesCopy[adjacentIndex];
                        if (adjacentState == 1)
                        {
                            validAndAliveAdjacentIndicesCount++;
                        }
                    }
                }
            }

            var canSpawn = validAndAliveAdjacentIndicesCount == reproductionStateCount;
            var canDie = validAndAliveAdjacentIndicesCount < minPopulationCutoff || validAndAliveAdjacentIndicesCount > maxPopulationThreshold;

            var currentStateValue = states[index];
            var isCurrentlyAlive = currentStateValue == 1;
            var canLive = (!isCurrentlyAlive && canSpawn) || (isCurrentlyAlive && !canDie);
            states[index] = math.select(0, 1, canLive);
        }
    }
}
