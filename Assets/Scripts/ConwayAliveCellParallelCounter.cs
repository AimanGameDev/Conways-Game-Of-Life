using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

public class ConwayAliveCellParallelCounter : IConwayAliveCellCounter
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
    private int m_sumJobsIndex;
    private Stack<SumBinaryTree> m_sumOperationNodeOrderStack;
    private Queue<SumBinaryTree> m_sumOperationsQueue;

    private SumBinaryTree m_root;
    private int m_range;

    public ConwayAliveCellParallelCounter(int maxCount, int range)
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
        Debug.Log("Parallel :: ScheduleJob");

        m_sumOperationsQueue.Clear();
        m_sumOperationNodeOrderStack.Clear();
        for (var i = 0; i < m_leafNodes.Count; i++)
        {
            var leafNode = m_leafNodes[i];
            leafNode.values.CopyFrom(states.GetSubArray(i * m_range, m_range));
            m_sumOperationNodeOrderStack.Push(leafNode);
        }

        m_sumJobsIndex = 0;

        while (m_sumOperationsQueue.Count != 1)
        {
            ScheduleAndComplete();
        }

        var rootSumJob = new ParallelSumJob
        {
            left = m_root.left.values,
            right = m_root.right.values,
            result = m_root.values,
        };
        rootSumJob.Schedule(m_range, 64).Complete();
    }

    private void ScheduleAndComplete()
    {
        while (m_sumOperationsQueue.Count > 0)
        {
            var node = m_sumOperationsQueue.Dequeue();
            m_sumOperationNodeOrderStack.Push(node);
        }

        while (m_sumOperationNodeOrderStack.Count > 1)
        {
            var left = m_sumOperationNodeOrderStack.Pop();
            var right = m_sumOperationNodeOrderStack.Pop();

            var jobHandle = new ParallelSumJob
            {
                left = left.values,
                right = right.values,
                result = left.parent.values,
            }.Schedule(m_range, 64);

            jobHandle.Complete();

            m_sumOperationsQueue.Enqueue(left.parent);
        }
    }

    public void CompleteJob(out int aliveCellsCount)
    {
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
