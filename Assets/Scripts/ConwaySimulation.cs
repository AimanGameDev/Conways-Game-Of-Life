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
        public bool canRender;
        public float cellSize;
        public float spacing;
        public float simulationTickRate;
        public int minPopulationCutoff;
        public int maxPopulationThreshold;
        public int adjanceLiveCellCountForRevival;
    }

    public enum Stages
    {
        Idle,
        CompleteConJob,
        CompleteSumJob,
        ExecuteCopyJob,
        ScheduleConJob,
        ScheduleSumJob,
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
    public Stages stages => m_stages;
    public bool canRender => m_dynamicConfiguration.canRender;

    private DynamicConfiguration m_dynamicConfiguration => ConwaySimulationConfigHolder.Instance.dynamicConfiguration;
    private StaticConfiguration m_staticConfiguration;
    private NativeArray<int> m_states;
    private NativeArray<int> m_statesCopy;
    private NativeArray<int>[] m_sums;
    private NativeArray<JobHandle> m_sumJobs;
    private JobHandle m_conJobHandle;
    private JobHandle m_copyJobHandle;
    private float m_simulationTime;
    private Stages m_stages;

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

        m_staticConfiguration.sumRange = maxCount; //TODO: From test observations, this is the best value for sumRange, ie 1 job for entire sum operation.
        var sumJobCount = maxCount / m_staticConfiguration.sumRange;
        sumJobCount = sumJobCount <= 0 ? 1 : sumJobCount;
        m_sumJobs = new NativeArray<JobHandle>(sumJobCount, Allocator.Persistent);
        m_sums = new NativeArray<int>[sumJobCount];
        for (var i = 0; i < sumJobCount; i++)
        {
            m_sums[i] = new NativeArray<int>(1, Allocator.Persistent);
        }

        UpdateBounds();

        m_stages = Stages.Idle;
    }

    private void Update()
    {
        m_simulationTime += Time.deltaTime;
        switch (m_stages)
        {
            case Stages.Idle:
                if (m_simulationTime >= m_dynamicConfiguration.simulationTickRate)
                {
                    m_simulationTime = 0;
                    m_stages = Stages.CompleteConJob;
                }
                break;
            case Stages.CompleteConJob:
                m_conJobHandle.Complete();
                m_stages = Stages.CompleteSumJob;
                break;
            case Stages.CompleteSumJob:
                CompleteSumJob();
                m_stages = Stages.ExecuteCopyJob;
                break;
            case Stages.ExecuteCopyJob:
                ExecuteCopyJob();
                m_stages = Stages.ScheduleConJob;
                break;
            case Stages.ScheduleConJob:
                ScheduleConJob();
                m_stages = Stages.ScheduleSumJob;
                break;
            case Stages.ScheduleSumJob:
                ScheduleSumJob();
                m_stages = Stages.Idle;
                break;
        }

        UpdateBounds();

        UnityEngine.Profiling.Profiler.BeginSample("ConwaySimulation :: " + m_stages);
        UnityEngine.Profiling.Profiler.EndSample();
    }

    private void UpdateBounds()
    {
        var whd = new Vector3(width, height, depth) * m_dynamicConfiguration.cellSize;
        var whds = new Vector3(width - 1, height - 1, depth - 1) * m_dynamicConfiguration.spacing;
        boundsSize = whd + whds;
        center = boundsSize / 2;
    }

    private void ScheduleConJob()
    {
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

    private void ScheduleSumJob()
    {
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
    }

    private void CompleteSumJob()
    {
        JobHandle.CompleteAll(m_sumJobs);

        var aliveCellsCountTemp = 0;
        for (var i = 0; i < m_sums.Length; i++)
        {
            aliveCellsCountTemp += m_sums[i][0];
        }
        aliveCellsCount = aliveCellsCountTemp;
    }

    private void ExecuteCopyJob()
    {
        var copyJob = new CopyJob
        {
            states = m_states,
            statesCopy = m_statesCopy
        };
        m_copyJobHandle = copyJob.Schedule(maxCount, 64);

        m_copyJobHandle.Complete();
    }

    private void OnDestroy()
    {
        m_conJobHandle.Complete();
        JobHandle.CompleteAll(m_sumJobs);
        m_copyJobHandle.Complete();

        m_states.Dispose();
        m_statesCopy.Dispose();
        m_sumJobs.Dispose();

        for (var i = 0; i < m_sums.Length; i++)
        {
            m_sums[i].Dispose();
        }
    }

    [BurstCompile]
    public struct CopyJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<int> states;
        public NativeArray<int> statesCopy;

        public void Execute(int index)
        {
            statesCopy[index] = states[index];
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
