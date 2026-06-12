using System.Collections.Generic;
using AoE.RTS.Core;
using AoE.RTS.Spatial;
using UnityEngine;

namespace AoE.RTS.Units
{
    public class UnitManager : MonoBehaviour, ISimulationTickable
    {
        static UnitManager instance;
        readonly List<Unit> units = new List<Unit>();

        public static UnitManager Instance => instance;

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            SimulationTick.Register(this);
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;

            SimulationTick.Unregister(this);
        }

        public void TickSimulation(float fixedDeltaTime)
        {
            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                if (unit == null || !unit.IsAlive)
                    continue;

                unit.TickMovement(fixedDeltaTime);
            }

            UnitSeparation.Apply(units, fixedDeltaTime);

            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                if (unit == null || !unit.IsAlive)
                    continue;

                UnitSpatialIndex.UpdatePosition(unit, unit.transform.position);
            }
        }

        public static void Register(Unit unit)
        {
            if (instance == null || unit == null)
                return;

            if (!instance.units.Contains(unit))
                instance.units.Add(unit);

            UnitSpatialIndex.Register(unit);
            UnitSpatialIndex.UpdatePosition(unit, unit.transform.position);
        }

        public static void Unregister(Unit unit)
        {
            if (unit == null)
                return;

            UnitSpatialIndex.Unregister(unit);
            instance?.units.Remove(unit);
        }

        public static void CopyUnitsTo(List<Unit> buffer)
        {
            buffer.Clear();
            if (instance == null)
                return;

            buffer.AddRange(instance.units);
        }

        public static int UnitCount => instance != null ? instance.units.Count : 0;

        public static int PlayerUnitCount => GetUnitCountForTeam(UnitTeam.Player);

        public static int GetUnitCountForPlayer(PlayerId playerId)
        {
            if (instance == null)
                return 0;

            int count = 0;
            for (int i = 0; i < instance.units.Count; i++)
            {
                Unit unit = instance.units[i];
                if (unit != null && unit.IsAlive && unit.OwnerId == playerId)
                    count++;
            }

            return count;
        }

        public static int GetUnitCountForTeam(UnitTeam team)
        {
            if (instance == null)
                return 0;

            int count = 0;
            for (int i = 0; i < instance.units.Count; i++)
            {
                Unit unit = instance.units[i];
                if (unit != null && unit.IsAlive && unit.Team == team)
                    count++;
            }

            return count;
        }
    }
}
