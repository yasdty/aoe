using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Units;

namespace AoE.RTS.Buildings
{
    public static class PlacementCostUtility
    {
        public static bool CanAfford(PlayerId playerId, PlacedBuildingData data)
        {
            if (data == null)
                return false;

            if (data.ScaledWoodCost > 0f && ResourceManager.GetWood(playerId) < data.ScaledWoodCost)
                return false;

            if (data.ScaledStoneCost > 0f && ResourceManager.GetStone(playerId) < data.ScaledStoneCost)
                return false;

            return true;
        }

        public static bool CanAfford(UnitTeam team, PlacedBuildingData data) =>
            CanAfford(PlayerIdMapping.FromLegacyTeam(team), data);

        public static bool TrySpend(PlayerId playerId, PlacedBuildingData data)
        {
            if (data == null || !CanAfford(playerId, data))
                return false;

            float woodCost = data.ScaledWoodCost;
            float stoneCost = data.ScaledStoneCost;

            if (woodCost > 0f && !ResourceManager.TrySpendWood(playerId, woodCost))
                return false;

            if (stoneCost > 0f && !ResourceManager.TrySpendStone(playerId, stoneCost))
            {
                if (woodCost > 0f)
                    ResourceManager.AddWood(playerId, woodCost);
                return false;
            }

            return true;
        }

        public static bool TrySpend(UnitTeam team, PlacedBuildingData data) =>
            TrySpend(PlayerIdMapping.FromLegacyTeam(team), data);
    }
}
