using AoE.RTS.Buildings;
using UnityEngine;

namespace AoE.RTS.Core
{
    public static class PlayerBuildingQueries
    {
        public static bool HasBarracksForPlayer(PlayerId playerId) =>
            GetBarracksForPlayer(playerId) != null;

        public static Barracks GetBarracksForPlayer(PlayerId playerId)
        {
            Barracks[] barracks = Object.FindObjectsByType<Barracks>();
            for (int i = 0; i < barracks.Length; i++)
            {
                Barracks building = barracks[i];
                if (building == null || !building.gameObject.activeInHierarchy)
                    continue;

                BuildingHealth health = building.GetComponent<BuildingHealth>();
                if (health != null && health.IsAlive && health.OwnerId == playerId)
                    return building;
            }

            return null;
        }

        public static bool HasArcheryRangeForPlayer(PlayerId playerId) =>
            GetArcheryRangeForPlayer(playerId) != null;

        public static ArcheryRange GetArcheryRangeForPlayer(PlayerId playerId)
        {
            ArcheryRange[] ranges = Object.FindObjectsByType<ArcheryRange>();
            for (int i = 0; i < ranges.Length; i++)
            {
                ArcheryRange building = ranges[i];
                if (building == null || !building.gameObject.activeInHierarchy)
                    continue;

                BuildingHealth health = building.GetComponent<BuildingHealth>();
                if (health != null && health.IsAlive && health.OwnerId == playerId)
                    return building;
            }

            return null;
        }

        public static bool HasStableForPlayer(PlayerId playerId) =>
            GetStableForPlayer(playerId) != null;

        public static Stable GetStableForPlayer(PlayerId playerId)
        {
            Stable[] stables = Object.FindObjectsByType<Stable>();
            for (int i = 0; i < stables.Length; i++)
            {
                Stable building = stables[i];
                if (building == null || !building.gameObject.activeInHierarchy)
                    continue;

                BuildingHealth health = building.GetComponent<BuildingHealth>();
                if (health != null && health.IsAlive && health.OwnerId == playerId)
                    return building;
            }

            return null;
        }
    }
}
