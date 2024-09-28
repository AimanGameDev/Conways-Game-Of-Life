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
        public bool deferredUpdate;
        public bool useQuads;
        public bool useFoldingCounter;
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
    public bool markViewDirty => m_markViewDirty;
    public bool useQuads => m_staticConfiguration.useQuads;

    private DynamicConfiguration m_dynamicConfiguration => ConwaySimulationConfigHolder.Instance.dynamicConfiguration;
    private StaticConfiguration m_staticConfiguration;
    private NativeArray<int> m_states;
    private NativeArray<int> m_statesCopy;
    private JobHandle m_conJobHandle;
    private JobHandle m_copyJobHandle;
    private IConwayAliveCellCounter m_aliveCellCounter;
    private float m_simulationTime;
    private Stages m_stages;
    private bool m_markViewDirty;

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

        m_staticConfiguration.sumRange = maxCount / 256;
        m_aliveCellCounter = new ConwayAliveCellParallelDividedLinearCounter(maxCount, m_staticConfiguration.sumRange);

        UpdateBounds();

        m_stages = Stages.Idle;
    }

    private void Update()
    {
        m_simulationTime += Time.deltaTime;

        if (m_staticConfiguration.deferredUpdate)
        {
            UpdateDeferred();
        }
        else
        {
            UpdateImmediate();
        }

        UpdateBounds();
    }

    private void UpdateImmediate()
    {
        m_markViewDirty = false;
        if (m_simulationTime >= m_dynamicConfiguration.simulationTickRate)
        {
            m_simulationTime = 0;

            m_conJobHandle.Complete();
            CompleteSumJob();
            ExecuteCopyJob();
            ScheduleConJob();
            ScheduleSumJob();

            m_markViewDirty = true;
        }
    }

    private void UpdateDeferred()
    {
        m_markViewDirty = false;
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
                m_markViewDirty = true;
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
            cellWidth = m_staticConfiguration.width,
            cellHeight = m_staticConfiguration.height,
            cellDepth = m_staticConfiguration.depth,
            minPopulationCutoff = m_dynamicConfiguration.minPopulationCutoff,
            maxPopulationThreshold = m_dynamicConfiguration.maxPopulationThreshold,
            states = m_states,
            statesCopy = m_statesCopy,
            reproductionStateCount = m_dynamicConfiguration.adjanceLiveCellCountForRevival
        };
        m_conJobHandle = conJob.Schedule(maxCount, 64);
        generationCount++;
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

    private void ScheduleSumJob()
    {
        m_aliveCellCounter.ScheduleJob(m_statesCopy);
    }

    private void CompleteSumJob()
    {
        m_aliveCellCounter.CompleteJob(out var aliveCellsCountTemp);
        aliveCellsCount = aliveCellsCountTemp;
    }

    private void OnDestroy()
    {
        m_conJobHandle.Complete();
        m_copyJobHandle.Complete();

        m_aliveCellCounter.Dispose();

        m_states.Dispose();
        m_statesCopy.Dispose();
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
    public struct ConJob : IJobParallelFor
    {
        [ReadOnly]
        public int cellWidth;
        [ReadOnly]
        public int cellHeight;
        [ReadOnly]
        public int cellDepth;
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
            int width = cellWidth;
            int height = cellHeight;
            int depth = cellDepth;
            int k = index / (width * height);
            int remainder = index % (width * height);
            int j = remainder / width;
            int i = remainder % width;
            int validAndAliveAdjacentIndicesCount = 0;

            for (int dx = -1; dx <= 1; dx++)
            {
                int vadx = 0;
                for (int dy = -1; dy <= 1; dy++)
                {
                    int vady = 0;
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        int iA = i + dx;
                        int jA = j + dy;
                        int kA = k + dz;
                        int isCenter = math.select(0, 1, dx == 0 & dy == 0 & dz == 0);
                        int isValidCoordinate = math.select(0, 1, iA >= 0 & iA < width & jA >= 0 & jA < height & kA >= 0 & kA < depth);
                        int adjacentIndex = iA + jA * width + kA * width * height;
                        int isValidIndex = (1 - isCenter) * isValidCoordinate;
                        int indexToAccess = math.select(index, adjacentIndex, 1 == isValidIndex);
                        vady += isValidIndex * statesCopy[indexToAccess];
                    }
                    vadx += vady;
                }
                validAndAliveAdjacentIndicesCount += vadx;
            }

            var currentStateValue = states[index];
            var canReproduce = math.select(0, 1, validAndAliveAdjacentIndicesCount == reproductionStateCount);
            var isWithinPopulationRange = math.select(0, 1, validAndAliveAdjacentIndicesCount >= minPopulationCutoff & validAndAliveAdjacentIndicesCount <= maxPopulationThreshold);
            var canLive = (1 - currentStateValue) * canReproduce + currentStateValue * isWithinPopulationRange;
            states[index] = canLive;
        }
    }
}
