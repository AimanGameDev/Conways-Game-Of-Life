using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

public class ConwayAliveCellLinearCounter : IConwayAliveCellCounter
{
    private NativeArray<int>[] m_sums;
    private NativeArray<JobHandle> m_sumJobs;
    private int m_range;

    public ConwayAliveCellLinearCounter(int maxCount, int range)
    {
        var sumJobCount = maxCount / range;
        sumJobCount = sumJobCount <= 0 ? 1 : sumJobCount;
        m_sumJobs = new NativeArray<JobHandle>(sumJobCount, Allocator.Persistent);
        m_sums = new NativeArray<int>[sumJobCount];
        for (var i = 0; i < sumJobCount; i++)
        {
            m_sums[i] = new NativeArray<int>(1, Allocator.Persistent);
        }

        m_range = range;
    }

    public void ScheduleJob(NativeArray<int> states)
    {
        var sumsCount = m_sums.Length;
        for (var i = 0; i < sumsCount; i++)
        {
            var slice = new NativeSlice<int>(states, i * m_range, m_range);
            var sumJob = new LinearSumJob
            {
                states = slice,
                sums = m_sums[i],
            };
            m_sumJobs[i] = sumJob.Schedule(m_sums[i].Length, 64);
        }
    }

    public void CompleteJob(out int sum)
    {
        JobHandle.CompleteAll(m_sumJobs);

        var aliveCellsCountTemp = 0;
        for (var i = 0; i < m_sums.Length; i++)
        {
            aliveCellsCountTemp += m_sums[i][0];
        }
        sum = aliveCellsCountTemp;
    }

    public void Dispose()
    {
        JobHandle.CompleteAll(m_sumJobs);

        m_sumJobs.Dispose();
        for (var i = 0; i < m_sums.Length; i++)
        {
            m_sums[i].Dispose();
        }
    }

    [BurstCompile]
    public struct LinearSumJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeSlice<int> states;
        public NativeArray<int> sums;

        public void Execute(int index)
        {
            int sum = 0;
            var length = states.Length;
            for (int i = 0; i < length; i++)
            {
                sum += states[i];
            }
            sums[index] = sum;
        }
    }
}
