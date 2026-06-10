using System.Collections.Generic;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Buildings
{
    public static class LumberCampRegistry
    {
        static readonly List<LumberCamp> camps = new List<LumberCamp>();

        public static void Register(LumberCamp lumberCamp)
        {
            if (lumberCamp == null || camps.Contains(lumberCamp))
                return;

            camps.Add(lumberCamp);
        }

        public static void Unregister(LumberCamp lumberCamp)
        {
            if (lumberCamp == null)
                return;

            camps.Remove(lumberCamp);
        }

        public static bool TryGetNearestWoodDepositPosition(Unit unit, float depositStandRadius, out Vector3 position)
        {
            position = Vector3.zero;
            if (unit == null)
                return false;

            Vector3 unitPosition = unit.transform.position;
            unitPosition.y = 0f;

            bool found = false;
            float bestDistanceSq = float.MaxValue;
            Vector3 bestCenter = Vector3.zero;

            TownCenter townCenter = ProductionManager.GetNearestTownCenter(unit.Team, unitPosition);
            if (townCenter != null)
            {
                Vector3 townCenterPosition = townCenter.transform.position;
                townCenterPosition.y = 0f;
                bestDistanceSq = (townCenterPosition - unitPosition).sqrMagnitude;
                bestCenter = townCenter.transform.position;
                bestCenter.y = 1f;
                found = true;
            }

            for (int i = 0; i < camps.Count; i++)
            {
                LumberCamp lumberCamp = camps[i];
                if (!IsActiveDropOff(lumberCamp, unit.Team))
                    continue;

                Vector3 campPosition = lumberCamp.GetDepositPosition();
                Vector3 flatCampPosition = campPosition;
                flatCampPosition.y = 0f;
                float distanceSq = (flatCampPosition - unitPosition).sqrMagnitude;
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                bestCenter = campPosition;
                found = true;
            }

            if (!found)
                return false;

            position = UnitPositionOffsets.ApplyRingOffset(bestCenter, unit, depositStandRadius);
            return true;
        }

        static bool IsActiveDropOff(LumberCamp lumberCamp, UnitTeam team)
        {
            if (lumberCamp == null || !lumberCamp.gameObject.activeInHierarchy || lumberCamp.Team != team)
                return false;

            BuildingHealth health = lumberCamp.GetComponent<BuildingHealth>();
            return health == null || health.IsAlive;
        }
    }
}
