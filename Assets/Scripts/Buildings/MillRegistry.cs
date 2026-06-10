using System.Collections.Generic;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Buildings
{
    public static class MillRegistry
    {
        static readonly List<Mill> mills = new List<Mill>();

        public static void Register(Mill mill)
        {
            if (mill == null || mills.Contains(mill))
                return;

            mills.Add(mill);
        }

        public static void Unregister(Mill mill)
        {
            if (mill == null)
                return;

            mills.Remove(mill);
        }

        public static bool TryGetNearestFoodDepositPosition(Unit unit, float depositStandRadius, out Vector3 position)
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

            for (int i = 0; i < mills.Count; i++)
            {
                Mill mill = mills[i];
                if (!IsActiveDropOff(mill, unit.Team))
                    continue;

                Vector3 millPosition = mill.GetDepositPosition();
                Vector3 flatMillPosition = millPosition;
                flatMillPosition.y = 0f;
                float distanceSq = (flatMillPosition - unitPosition).sqrMagnitude;
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                bestCenter = millPosition;
                found = true;
            }

            if (!found)
                return false;

            position = UnitPositionOffsets.ApplyRingOffset(bestCenter, unit, depositStandRadius);
            return true;
        }

        static bool IsActiveDropOff(Mill mill, UnitTeam team)
        {
            if (mill == null || !mill.gameObject.activeInHierarchy || mill.Team != team)
                return false;

            BuildingHealth health = mill.GetComponent<BuildingHealth>();
            return health == null || health.IsAlive;
        }
    }
}
