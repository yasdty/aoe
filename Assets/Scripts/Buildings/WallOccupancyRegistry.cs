using System.Collections.Generic;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Buildings
{
    public static class WallOccupancyRegistry
    {
        const float CellSize = 1f;

        struct OccupancyEntry
        {
            public int id;
            public Vector3 center;
            public float halfWidth;
            public float halfDepth;
            public float rotationY;
            public UnitTeam team;
            public WallOccupancyKind kind;
            public bool active;
        }

        static readonly List<OccupancyEntry> entries = new List<OccupancyEntry>();
        static readonly Dictionary<long, List<int>> cellToEntryIds = new Dictionary<long, List<int>>();
        static int nextId = 1;

        public static int Register(
            Vector3 center,
            float width,
            float depth,
            float rotationY,
            UnitTeam team,
            WallOccupancyKind kind)
        {
            int id = nextId++;
            var entry = new OccupancyEntry
            {
                id = id,
                center = new Vector3(center.x, 0f, center.z),
                halfWidth = Mathf.Max(0.5f, width * 0.5f),
                halfDepth = Mathf.Max(0.5f, depth * 0.5f),
                rotationY = rotationY,
                team = team,
                kind = kind,
                active = true
            };
            entries.Add(entry);
            AddEntryToCells(entry);
            return id;
        }

        public static void Unregister(int id)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].id != id || !entries[i].active)
                    continue;

                RemoveEntryFromCells(entries[i]);
                OccupancyEntry cleared = entries[i];
                cleared.active = false;
                entries[i] = cleared;
                return;
            }
        }

        public static bool IsMovementBlocked(Vector3 worldPosition, UnitTeam unitTeam, float sampleRadius = 0.5f)
        {
            if (entries.Count == 0)
                return false;

            if (IsPointBlocked(worldPosition, unitTeam))
                return true;

            if (sampleRadius <= 0f)
                return false;

            const int sampleCount = 6;
            for (int i = 0; i < sampleCount; i++)
            {
                float angle = i * (Mathf.PI * 2f / sampleCount);
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * sampleRadius;
                if (IsPointBlocked(worldPosition + offset, unitTeam))
                    return true;
            }

            return false;
        }

        public static bool IsMovementBlockedAlongPath(Vector3 from, Vector3 to, UnitTeam unitTeam)
        {
            from.y = 0f;
            to.y = 0f;
            float distance = Vector3.Distance(from, to);
            if (distance <= 0.001f)
                return IsMovementBlocked(from, unitTeam);

            int steps = Mathf.Max(1, Mathf.CeilToInt(distance / 0.35f));
            for (int i = 1; i <= steps; i++)
            {
                Vector3 sample = Vector3.Lerp(from, to, i / (float)steps);
                if (IsMovementBlocked(sample, unitTeam))
                    return true;
            }

            return false;
        }

        public static bool IsAdjacentToWall(Vector3 worldPosition, float maxDistance)
        {
            float maxDistanceSq = maxDistance * maxDistance;
            for (int i = 0; i < entries.Count; i++)
            {
                OccupancyEntry entry = entries[i];
                if (!entry.active || entry.kind != WallOccupancyKind.Wall)
                    continue;

                Vector3 delta = entry.center - worldPosition;
                delta.y = 0f;
                if (delta.sqrMagnitude <= maxDistanceSq)
                    return true;
            }

            return false;
        }

        static bool IsPointBlocked(Vector3 worldPosition, UnitTeam unitTeam)
        {
            GetCellCoords(worldPosition, out int gx, out int gz);
            for (int dz = -1; dz <= 1; dz++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    long key = CellKey(gx + dx, gz + dz);
                    if (!cellToEntryIds.TryGetValue(key, out List<int> ids))
                        continue;

                    for (int i = 0; i < ids.Count; i++)
                    {
                        OccupancyEntry entry = GetEntry(ids[i]);
                        if (!entry.active)
                            continue;

                        if (!ContainsPoint(entry, worldPosition))
                            continue;

                        if (entry.kind == WallOccupancyKind.Gate && entry.team == unitTeam)
                            return false;

                        return true;
                    }
                }
            }

            return false;
        }

        static OccupancyEntry GetEntry(int id)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].id == id)
                    return entries[i];
            }

            return default;
        }

        static void AddEntryToCells(OccupancyEntry entry)
        {
            foreach ((int gx, int gz) in GetCoveredCells(entry))
            {
                long key = CellKey(gx, gz);
                if (!cellToEntryIds.TryGetValue(key, out List<int> ids))
                {
                    ids = new List<int>(2);
                    cellToEntryIds[key] = ids;
                }

                ids.Add(entry.id);
            }
        }

        static void RemoveEntryFromCells(OccupancyEntry entry)
        {
            foreach ((int gx, int gz) in GetCoveredCells(entry))
            {
                long key = CellKey(gx, gz);
                if (!cellToEntryIds.TryGetValue(key, out List<int> ids))
                    continue;

                for (int i = ids.Count - 1; i >= 0; i--)
                {
                    if (ids[i] == entry.id)
                        ids.RemoveAt(i);
                }

                if (ids.Count == 0)
                    cellToEntryIds.Remove(key);
            }
        }

        static IEnumerable<(int gx, int gz)> GetCoveredCells(OccupancyEntry entry)
        {
            GetOrientedExtents(entry, out float minX, out float maxX, out float minZ, out float maxZ);
            int minGx = Mathf.FloorToInt(minX / CellSize);
            int maxGx = Mathf.FloorToInt(maxX / CellSize);
            int minGz = Mathf.FloorToInt(minZ / CellSize);
            int maxGz = Mathf.FloorToInt(maxZ / CellSize);

            Vector3 samplePoint = new Vector3();
            for (int gz = minGz; gz <= maxGz; gz++)
            {
                for (int gx = minGx; gx <= maxGx; gx++)
                {
                    samplePoint.x = (gx + 0.5f) * CellSize;
                    samplePoint.z = (gz + 0.5f) * CellSize;
                    if (ContainsPoint(entry, samplePoint))
                        yield return (gx, gz);
                }
            }
        }

        static void GetOrientedExtents(
            OccupancyEntry entry,
            out float minX,
            out float maxX,
            out float minZ,
            out float maxZ)
        {
            Vector3[] corners =
            {
                new Vector3(-entry.halfWidth, 0f, -entry.halfDepth),
                new Vector3(entry.halfWidth, 0f, -entry.halfDepth),
                new Vector3(entry.halfWidth, 0f, entry.halfDepth),
                new Vector3(-entry.halfWidth, 0f, entry.halfDepth)
            };

            float radians = entry.rotationY * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            minX = float.MaxValue;
            maxX = float.MinValue;
            minZ = float.MaxValue;
            maxZ = float.MinValue;

            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 rotated = new Vector3(
                    corners[i].x * cos - corners[i].z * sin,
                    0f,
                    corners[i].x * sin + corners[i].z * cos);
                rotated += entry.center;

                minX = Mathf.Min(minX, rotated.x);
                maxX = Mathf.Max(maxX, rotated.x);
                minZ = Mathf.Min(minZ, rotated.z);
                maxZ = Mathf.Max(maxZ, rotated.z);
            }
        }

        static bool ContainsPoint(OccupancyEntry entry, Vector3 worldPosition)
        {
            Vector3 local = worldPosition - entry.center;
            local.y = 0f;

            float radians = -entry.rotationY * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            float localX = local.x * cos - local.z * sin;
            float localZ = local.x * sin + local.z * cos;

            return Mathf.Abs(localX) <= entry.halfWidth && Mathf.Abs(localZ) <= entry.halfDepth;
        }

        static void GetCellCoords(Vector3 worldPosition, out int gx, out int gz)
        {
            gx = Mathf.FloorToInt(worldPosition.x / CellSize);
            gz = Mathf.FloorToInt(worldPosition.z / CellSize);
        }

        static long CellKey(int gx, int gz)
        {
            return ((long)gx << 32) ^ (uint)gz;
        }
    }
}
