using System;
using System.Collections.Generic;
using AoE.RTS.Spatial;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Spatial
{
    public class UnitSpatialIndex : MonoBehaviour
    {
        static UnitSpatialIndex instance;

        [SerializeField] float cellSize = 12f;
        [SerializeField] float defaultMaxSearchRadius = 256f;

        SpatialHashGrid<Unit> grid;

        static Func<Unit, Vector3> UnitPosition => static unit => unit.transform.position;

        void Awake()
        {
            instance = this;
            grid = new SpatialHashGrid<Unit>(cellSize);
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public static void Register(Unit unit)
        {
            if (instance == null || unit == null)
                return;

            instance.grid.Insert(unit, unit.transform.position);
        }

        public static void Unregister(Unit unit)
        {
            if (instance == null || unit == null)
                return;

            instance.grid.Remove(unit);
        }

        public static void UpdatePosition(Unit unit, Vector3 worldPosition)
        {
            if (instance == null || unit == null)
                return;

            instance.grid.Update(unit, worldPosition);
        }

        public static Unit FindNearestUnit(Vector3 origin, UnitTeam team, Func<Unit, bool> extraFilter = null)
        {
            if (instance == null)
                return null;

            bool TryFind(out Unit nearest)
            {
                return instance.grid.TryFindNearest(
                    origin,
                    instance.defaultMaxSearchRadius,
                    UnitPosition,
                    unit => IsMatchingUnit(unit, team, extraFilter),
                    out nearest);
            }

            return TryFind(out Unit result) ? result : null;
        }

        public static void QueryInWorldBounds(
            float minX,
            float maxX,
            float minZ,
            float maxZ,
            List<Unit> results,
            Func<Unit, bool> filter)
        {
            if (instance == null || results == null)
                return;

            instance.grid.QueryInBounds(minX, maxX, minZ, maxZ, results, filter);
        }

        static bool IsMatchingUnit(Unit unit, UnitTeam team, Func<Unit, bool> extraFilter)
        {
            if (unit == null || !unit.IsAlive || unit.Team != team)
                return false;

            return extraFilter == null || extraFilter(unit);
        }
    }
}
