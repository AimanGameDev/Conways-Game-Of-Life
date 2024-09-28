using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;

public class ConwayAliveCellParallelBinaryTreeCounter : IConwayAliveCellCounter
{
    public class SumBinaryTree
    {
        public NativeArray<int> values;
        public SumBinaryTree left;
        public SumBinaryTree right;
        public SumBinaryTree parent;
    }

    private List<SumBinaryTree> m_leafNodes;
    private List<SumBinaryTree> m_allNodes;
    private NativeArray<JobHandle> m_sumJobs;
    private Stack<SumBinaryTree> m_sumOperationNodeOrderStack;
    private Queue<SumBinaryTree> m_sumOperationsQueue;

    private SumBinaryTree m_root;
    private int m_range;

    public ConwayAliveCellParallelBinaryTreeCounter(int maxCount, int range)
    {
        m_range = range;

        var leafNodeCount = maxCount / range;
        leafNodeCount = leafNodeCount <= 0 ? 1 : leafNodeCount;

        m_leafNodes = new List<SumBinaryTree>(leafNodeCount);
        for (var i = 0; i < leafNodeCount; i++)
        {
            var leafNode = new SumBinaryTree();
            leafNode.values = new NativeArray<int>(range, Allocator.Persistent);
            leafNode.left = null;
            leafNode.right = null;
            leafNode.parent = null;
            m_leafNodes.Add(leafNode);
        }

        var treeCreatorQueue = new Queue<SumBinaryTree>(m_leafNodes);

        var nonLeafNodeCount = 0;
        while (treeCreatorQueue.Count > 1)
        {
            var left = treeCreatorQueue.Dequeue();
            var right = treeCreatorQueue.Dequeue();

            var parent = new SumBinaryTree();
            parent.values = new NativeArray<int>(range, Allocator.Persistent);
            parent.left = left;
            parent.right = right;
            parent.parent = null;
            left.parent = parent;
            right.parent = parent;

            nonLeafNodeCount++;

            treeCreatorQueue.Enqueue(parent);
        }

        m_root = treeCreatorQueue.Dequeue();

        m_allNodes = new List<SumBinaryTree>(leafNodeCount * 2 - 1);
        void InOrderTraversal(SumBinaryTree node)
        {
            if (node == null)
                return;

            InOrderTraversal(node.left);
            m_allNodes.Add(node);
            InOrderTraversal(node.right);
        }

        InOrderTraversal(m_root);

        m_sumJobs = new NativeArray<JobHandle>(nonLeafNodeCount, Allocator.Persistent);
        m_sumOperationNodeOrderStack = new Stack<SumBinaryTree>(leafNodeCount);
        m_sumOperationsQueue = new Queue<SumBinaryTree>(leafNodeCount);
    }

    public void ScheduleJob(NativeArray<int> states)
    {
        m_sumOperationsQueue.Clear();
        m_sumOperationNodeOrderStack.Clear();
        for (var i = 0; i < m_leafNodes.Count; i++)
        {
            var leafNode = m_leafNodes[i];
            var subArray = states.GetSubArray(i * m_range, m_range);
            leafNode.values.CopyFrom(subArray);
            m_sumOperationNodeOrderStack.Push(leafNode);
        }

        var sumJobsStartIndex = 0;
        while (m_sumOperationsQueue.Count != 1)
        {
            ScheduleAndComplete(in sumJobsStartIndex, out var jobsCount);
            sumJobsStartIndex += jobsCount;
        }
    }

    private void ScheduleAndComplete(in int sumJobsStartIndex, out int jobsCount)
    {
        jobsCount = 0;

        while (m_sumOperationsQueue.Count > 0)
        {
            var node = m_sumOperationsQueue.Dequeue();
            m_sumOperationNodeOrderStack.Push(node);
        }

        while (m_sumOperationNodeOrderStack.Count > 1)
        {
            var left = m_sumOperationNodeOrderStack.Pop();
            var right = m_sumOperationNodeOrderStack.Pop();

            var parallelSumJob = new ParallelSumJob
            {
                left = left.values,
                right = right.values,
                result = left.parent.values,
            };
            m_sumJobs[sumJobsStartIndex] = parallelSumJob.Schedule(m_range, 64);
            m_sumJobs[sumJobsStartIndex].Complete();

            jobsCount++;
            m_sumOperationsQueue.Enqueue(left.parent);
        }
    }

    public void CompleteJob(out int aliveCellsCount)
    {
        JobHandle.CompleteAll(m_sumJobs);

        var aliveCellsCountTemp = 0;
        var values = m_root.values;
        for (var i = 0; i < values.Length; i++)
        {
            aliveCellsCountTemp += values[i];
        }
        aliveCellsCount = aliveCellsCountTemp;
    }

    public void Dispose()
    {
        JobHandle.CompleteAll(m_sumJobs);

        m_sumJobs.Dispose();

        for (var i = 0; i < m_allNodes.Count; i++)
        {
            var node = m_allNodes[i];
            node.values.Dispose();
        }
    }

    [BurstCompile]
    public struct ParallelSumJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<int> left;
        [ReadOnly]
        public NativeArray<int> right;

        public NativeArray<int> result;

        public void Execute(int index)
        {
            result[index] = left[index] + right[index];
        }
    }
}
