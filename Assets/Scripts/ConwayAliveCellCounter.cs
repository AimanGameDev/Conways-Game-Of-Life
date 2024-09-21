using System;
using Unity.Collections;

public interface IConwayAliveCellCounter : IDisposable
{
    void ScheduleJob(NativeArray<int> states);
    void CompleteJob(out int aliveCellsCount);
}