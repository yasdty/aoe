using System.Collections.Generic;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Buildings
{
    public static class MiningCampRegistry
    {
        static readonly List<MiningCamp> camps = new List<MiningCamp>();

        public static void Register(MiningCamp miningCamp)
        {
            if (miningCamp == null || camps.Contains(miningCamp))
                return;

            camps.Add(miningCamp);
        }

        public static void Unregister(MiningCamp miningCamp)
        {
            if (miningCamp == null)
                return;

            camps.Remove(miningCamp);
        }

        public static bool TryGetNearestMineralDepositPosition(Unit unit, float depositStandRadius, out Vector3 position)
        {
            position = Vector3.zero;
            if (unit == null)
                return false;

            Vector3 unitPosition = unit.transform.position;
            unitPosition.y = 0f;

            bool found = false;
            float bestDistanceSq = float.MaxValue;
            Vector3 bestCenter = Vector3.zero;

            TownCenter townCenter = ProductionManager.GetNearestTownCenter(unit.OwnerId, unitPosition);
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
                MiningCamp miningCamp = camps[i];
                if (!IsActiveDropOff(miningCamp, unit))
                    continue;

                Vector3 campPosition = miningCamp.GetDepositPosition();
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

        static bool IsActiveDropOff(MiningCamp miningCamp, Unit unit)
        {
            if (miningCamp == null || !miningCamp.gameObject.activeInHierarchy || unit == null)
                return false;

            BuildingHealth health = miningCamp.GetComponent<BuildingHealth>();
            return health != null && health.IsAlive && health.OwnerId == unit.OwnerId;
        }
    }
}
