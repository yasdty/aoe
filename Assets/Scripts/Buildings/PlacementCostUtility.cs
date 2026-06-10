using AoE.RTS.Economy;
using AoE.RTS.Units;

namespace AoE.RTS.Buildings
{
    public static class PlacementCostUtility
    {
        public static bool CanAfford(UnitTeam team, PlacedBuildingData data)
        {
            if (data == null)
                return false;

            if (data.ScaledWoodCost > 0f && ResourceManager.GetWood(team) < data.ScaledWoodCost)
                return false;

            if (data.ScaledStoneCost > 0f && ResourceManager.GetStone(team) < data.ScaledStoneCost)
                return false;

            return true;
        }

        public static bool TrySpend(UnitTeam team, PlacedBuildingData data)
        {
            if (data == null || !CanAfford(team, data))
                return false;

            float woodCost = data.ScaledWoodCost;
            float stoneCost = data.ScaledStoneCost;

            if (woodCost > 0f && !ResourceManager.TrySpendWood(team, woodCost))
                return false;

            if (stoneCost > 0f && !ResourceManager.TrySpendStone(team, stoneCost))
            {
                if (woodCost > 0f)
                    ResourceManager.AddWood(team, woodCost);
                return false;
            }

            return true;
        }
    }
}
