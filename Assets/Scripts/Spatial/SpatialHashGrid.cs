using System;
using System.Collections.Generic;
using UnityEngine;

namespace AoE.RTS.Spatial
{
    public class SpatialHashGrid<T> where T : class
    {
        struct CellKey : IEquatable<CellKey>
        {
            public int x;
            public int z;

            public bool Equals(CellKey other) => x == other.x && z == other.z;

            public override bool Equals(object obj) => obj is CellKey other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    return (x * 397) ^ z;
                }
            }
        }

        readonly float cellSize;
        readonly float inverseCellSize;
        readonly Dictionary<CellKey, List<T>> cells = new Dictionary<CellKey, List<T>>();
        readonly Dictionary<T, CellKey> itemCells = new Dictionary<T, CellKey>();

        public SpatialHashGrid(float cellSize)
        {
            this.cellSize = Mathf.Max(0.01f, cellSize);
            inverseCellSize = 1f / this.cellSize;
        }

        public void Insert(T item, Vector3 worldPosition)
        {
            if (item == null)
                return;

            Remove(item);
            CellKey key = WorldToCell(worldPosition);
            itemCells[item] = key;
            GetOrCreateCellList(key).Add(item);
        }

        public void Remove(T item)
        {
            if (item == null || !itemCells.TryGetValue(item, out CellKey key))
                return;

            if (cells.TryGetValue(key, out List<T> list))
            {
                list.Remove(item);
                if (list.Count == 0)
                    cells.Remove(key);
            }

            itemCells.Remove(item);
        }

        public void Update(T item, Vector3 worldPosition)
        {
            if (item == null)
                return;

            CellKey newKey = WorldToCell(worldPosition);
            if (itemCells.TryGetValue(item, out CellKey oldKey) && oldKey.Equals(newKey))
                return;

            Remove(item);
            Insert(item, worldPosition);
        }

        public bool TryFindNearest(
            Vector3 origin,
            float maxRadius,
            Func<T, Vector3> getPosition,
            Func<T, bool> filter,
            out T nearest)
        {
            nearest = null;
            if (getPosition == null)
                return false;

            float maxRadiusSq = maxRadius * maxRadius;
            float bestDistanceSq = float.MaxValue;
            CellKey originCell = WorldToCell(origin);
            int maxRing = Mathf.CeilToInt(maxRadius * inverseCellSize);

            for (int ring = 0; ring <= maxRing; ring++)
            {
                QueryRing(originCell, ring, origin, maxRadiusSq, getPosition, filter, ref nearest, ref bestDistanceSq);

                if (nearest != null && ring * cellSize >= Mathf.Sqrt(bestDistanceSq))
                    break;
            }

            return nearest != null;
        }

        public void QueryInBounds(
            float minX,
            float maxX,
            float minZ,
            float maxZ,
            List<T> results,
            Func<T, bool> filter)
        {
            if (results == null)
                return;

            int minCellX = Mathf.FloorToInt(minX * inverseCellSize);
            int maxCellX = Mathf.FloorToInt(maxX * inverseCellSize);
            int minCellZ = Mathf.FloorToInt(minZ * inverseCellSize);
            int maxCellZ = Mathf.FloorToInt(maxZ * inverseCellSize);

            for (int x = minCellX; x <= maxCellX; x++)
            {
                for (int z = minCellZ; z <= maxCellZ; z++)
                {
                    CellKey key = new CellKey { x = x, z = z };
                    if (!cells.TryGetValue(key, out List<T> list))
                        continue;

                    for (int i = 0; i < list.Count; i++)
                    {
                        T item = list[i];
                        if (item == null || filter != null && !filter(item))
                            continue;

                        results.Add(item);
                    }
                }
            }
        }

        public void CollectNearest(
            Vector3 origin,
            float maxRadius,
            int count,
            Func<T, Vector3> getPosition,
            Func<T, bool> filter,
            List<(T item, float distanceSq)> results)
        {
            results.Clear();
            if (count <= 0 || getPosition == null)
                return;

            float maxRadiusSq = maxRadius * maxRadius;
            CellKey originCell = WorldToCell(origin);
            int maxRing = Mathf.CeilToInt(maxRadius * inverseCellSize);

            for (int ring = 0; ring <= maxRing && results.Count < count; ring++)
            {
                CollectRing(originCell, ring, origin, maxRadiusSq, getPosition, filter, results);

                if (results.Count >= count)
                    break;
            }

            results.Sort((a, b) => a.distanceSq.CompareTo(b.distanceSq));
            if (results.Count > count)
                results.RemoveRange(count, results.Count - count);
        }

        void QueryRing(
            CellKey originCell,
            int ring,
            Vector3 origin,
            float maxRadiusSq,
            Func<T, Vector3> getPosition,
            Func<T, bool> filter,
            ref T nearest,
            ref float bestDistanceSq)
        {
            if (ring == 0)
            {
                TryUpdateNearest(originCell, origin, maxRadiusSq, getPosition, filter, ref nearest, ref bestDistanceSq);
                return;
            }

            int x = originCell.x;
            int z = originCell.z;

            for (int dx = -ring; dx <= ring; dx++)
            {
                TryUpdateNearest(new CellKey { x = x + dx, z = z + ring }, origin, maxRadiusSq, getPosition, filter, ref nearest, ref bestDistanceSq);
                TryUpdateNearest(new CellKey { x = x + dx, z = z - ring }, origin, maxRadiusSq, getPosition, filter, ref nearest, ref bestDistanceSq);
            }

            for (int dz = -ring + 1; dz <= ring - 1; dz++)
            {
                TryUpdateNearest(new CellKey { x = x + ring, z = z + dz }, origin, maxRadiusSq, getPosition, filter, ref nearest, ref bestDistanceSq);
                TryUpdateNearest(new CellKey { x = x - ring, z = z + dz }, origin, maxRadiusSq, getPosition, filter, ref nearest, ref bestDistanceSq);
            }
        }

        void CollectRing(
            CellKey originCell,
            int ring,
            Vector3 origin,
            float maxRadiusSq,
            Func<T, Vector3> getPosition,
            Func<T, bool> filter,
            List<(T item, float distanceSq)> results)
        {
            if (ring == 0)
            {
                CollectCell(originCell, origin, maxRadiusSq, getPosition, filter, results);
                return;
            }

            int x = originCell.x;
            int z = originCell.z;

            for (int dx = -ring; dx <= ring; dx++)
            {
                CollectCell(new CellKey { x = x + dx, z = z + ring }, origin, maxRadiusSq, getPosition, filter, results);
                CollectCell(new CellKey { x = x + dx, z = z - ring }, origin, maxRadiusSq, getPosition, filter, results);
            }

            for (int dz = -ring + 1; dz <= ring - 1; dz++)
            {
                CollectCell(new CellKey { x = x + ring, z = z + dz }, origin, maxRadiusSq, getPosition, filter, results);
                CollectCell(new CellKey { x = x - ring, z = z + dz }, origin, maxRadiusSq, getPosition, filter, results);
            }
        }

        void TryUpdateNearest(
            CellKey key,
            Vector3 origin,
            float maxRadiusSq,
            Func<T, Vector3> getPosition,
            Func<T, bool> filter,
            ref T nearest,
            ref float bestDistanceSq)
        {
            if (!cells.TryGetValue(key, out List<T> list))
                return;

            for (int i = 0; i < list.Count; i++)
            {
                T item = list[i];
                if (item == null || filter != null && !filter(item))
                    continue;

                float distanceSq = HorizontalDistanceSq(origin, getPosition(item));
                if (distanceSq > maxRadiusSq || distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                nearest = item;
            }
        }

        void CollectCell(
            CellKey key,
            Vector3 origin,
            float maxRadiusSq,
            Func<T, Vector3> getPosition,
            Func<T, bool> filter,
            List<(T item, float distanceSq)> results)
        {
            if (!cells.TryGetValue(key, out List<T> list))
                return;

            for (int i = 0; i < list.Count; i++)
            {
                T item = list[i];
                if (item == null || filter != null && !filter(item))
                    continue;

                float distanceSq = HorizontalDistanceSq(origin, getPosition(item));
                if (distanceSq > maxRadiusSq)
                    continue;

                results.Add((item, distanceSq));
            }
        }

        List<T> GetOrCreateCellList(CellKey key)
        {
            if (!cells.TryGetValue(key, out List<T> list))
            {
                list = new List<T>();
                cells[key] = list;
            }

            return list;
        }

        CellKey WorldToCell(Vector3 worldPosition)
        {
            return new CellKey
            {
                x = Mathf.FloorToInt(worldPosition.x * inverseCellSize),
                z = Mathf.FloorToInt(worldPosition.z * inverseCellSize)
            };
        }

        static float HorizontalDistanceSq(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return dx * dx + dz * dz;
        }
    }
}
