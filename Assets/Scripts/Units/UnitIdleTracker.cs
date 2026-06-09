using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Combat;
using AoE.RTS.Economy;
using UnityEngine;

namespace AoE.RTS.Units
{
    public static class UnitIdleTracker
    {
        static readonly List<Unit> scratchUnits = new List<Unit>(32);

        public static bool IsIdleVillager(Unit unit)
        {
            return IsIdleVillagerForTeam(unit, UnitTeam.Player);
        }

        public static bool IsIdleMilitary(Unit unit)
        {
            return IsIdleMilitaryForTeam(unit, UnitTeam.Player);
        }

        public static int CountIdleVillagers(UnitTeam team = UnitTeam.Player)
        {
            int count = 0;
            UnitManager.CopyUnitsTo(scratchUnits);
            for (int i = 0; i < scratchUnits.Count; i++)
            {
                if (IsIdleVillagerForTeam(scratchUnits[i], team))
                    count++;
            }

            return count;
        }

        public static int CountIdleMilitary(UnitTeam team = UnitTeam.Player)
        {
            int count = 0;
            UnitManager.CopyUnitsTo(scratchUnits);
            for (int i = 0; i < scratchUnits.Count; i++)
            {
                if (IsIdleMilitaryForTeam(scratchUnits[i], team))
                    count++;
            }

            return count;
        }

        public static void CopyIdleVillagersTo(List<Unit> buffer, UnitTeam team = UnitTeam.Player)
        {
            buffer.Clear();
            UnitManager.CopyUnitsTo(scratchUnits);
            for (int i = 0; i < scratchUnits.Count; i++)
            {
                Unit unit = scratchUnits[i];
                if (IsIdleVillagerForTeam(unit, team))
                    buffer.Add(unit);
            }

            buffer.Sort(CompareByEntityId);
        }

        public static void CopyIdleMilitaryTo(List<Unit> buffer, UnitTeam team = UnitTeam.Player)
        {
            buffer.Clear();
            UnitManager.CopyUnitsTo(scratchUnits);
            for (int i = 0; i < scratchUnits.Count; i++)
            {
                Unit unit = scratchUnits[i];
                if (IsIdleMilitaryForTeam(unit, team))
                    buffer.Add(unit);
            }

            buffer.Sort(CompareByEntityId);
        }

        static bool IsIdleVillagerForTeam(Unit unit, UnitTeam team)
        {
            if (unit == null || !unit.IsAlive || unit.Team != team || unit.CanAttack)
                return false;

            return IsEconomyIdle(unit);
        }

        static bool IsIdleMilitaryForTeam(Unit unit, UnitTeam team)
        {
            if (unit == null || !unit.IsAlive || unit.Team != team || !unit.CanAttack)
                return false;

            if (unit.HasMoveTarget)
                return false;

            if (AttackManager.IsUnitAttacking(unit))
                return false;

            if (BoarAttackManager.IsUnitAttackingBoar(unit))
                return false;

            return true;
        }

        static bool IsEconomyIdle(Unit unit)
        {
            if (unit.HasMoveTarget)
                return false;

            if (GatherManager.IsUnitGathering(unit))
                return false;

            if (FoodGatherManager.IsUnitGathering(unit))
                return false;

            if (MineralGatherManager.IsUnitGathering(unit))
                return false;

            if (BuildingPlacementManager.IsUnitBuilding(unit))
                return false;

            return true;
        }

        static int CompareByEntityId(Unit a, Unit b)
        {
            return a.GetEntityId().CompareTo(b.GetEntityId());
        }
    }
}
