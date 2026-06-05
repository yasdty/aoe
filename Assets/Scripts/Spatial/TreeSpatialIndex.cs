using System;
using System.Collections.Generic;
using AoE.RTS.Economy;
using AoE.RTS.Spatial;
using UnityEngine;

namespace AoE.RTS.Spatial
{
    public class TreeSpatialIndex : MonoBehaviour
    {
        static TreeSpatialIndex instance;

        [SerializeField] float cellSize = 12f;
        [SerializeField] float defaultMaxSearchRadius = 256f;
        [SerializeField] int rankedSearchBufferSize = 32;

        SpatialHashGrid<TreeResource> grid;
        readonly List<(TreeResource tree, float distanceSq)> rankedQueryBuffer = new List<(TreeResource, float)>();

        static Func<TreeResource, Vector3> TreePosition => static tree => tree.transform.position;

        void Awake()
        {
            instance = this;
            grid = new SpatialHashGrid<TreeResource>(cellSize);
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public static void Register(TreeResource tree)
        {
            if (instance == null || tree == null || tree.IsDepleted)
                return;

            instance.grid.Insert(tree, tree.transform.position);
        }

        public static void Unregister(TreeResource tree)
        {
            if (instance == null || tree == null)
                return;

            instance.grid.Remove(tree);
        }

        public static TreeResource FindNearestAvailable(Vector3 origin)
        {
            if (instance == null)
                return null;

            if (instance.grid.TryFindNearest(
                    origin,
                    instance.defaultMaxSearchRadius,
                    TreePosition,
                    IsAvailableTree,
                    out TreeResource nearest))
                return nearest;

            return null;
        }

        public static TreeResource FindRankedAvailable(Vector3 origin, int rank)
        {
            if (instance == null)
                return FindNearestAvailable(origin);

            int desiredCount = Mathf.Max(rank + 1, 1);
            int searchCount = Mathf.Max(desiredCount, instance.rankedSearchBufferSize);
            instance.grid.CollectNearest(
                origin,
                instance.defaultMaxSearchRadius,
                searchCount,
                TreePosition,
                IsAvailableTree,
                instance.rankedQueryBuffer);

            if (instance.rankedQueryBuffer.Count > rank)
                return instance.rankedQueryBuffer[rank].tree;

            return FindNearestAvailable(origin);
        }

        static bool IsAvailableTree(TreeResource tree)
        {
            return tree != null && !tree.IsDepleted;
        }
    }
}
