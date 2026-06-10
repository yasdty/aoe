using AoE.RTS.Buildings;
using AoE.RTS.Units;

namespace AoE.RTS.Economy
{
    public static class ProductionQueueRefundUtility
    {
        public static void RefundTownCenterJob(UnitTeam team, TownCenter townCenter, UnitData unitData)
        {
            if (townCenter == null || townCenter.Data == null || unitData == null)
                return;

            if (unitData != townCenter.Data.villagerUnitData)
                return;

            float food = townCenter.Data.ScaledVillagerFoodCost;
            if (food > 0f)
                ResourceManager.AddFood(team, food);
        }

        public static void RefundBarracksJob(UnitTeam team, Barracks barracks, UnitData unitData)
        {
            if (barracks == null || barracks.Data == null || unitData == null)
                return;

            PlacedBuildingData data = barracks.Data;
            UnitData primary = BarracksTraining.ResolvePrimaryTrainUnit(barracks);
            if (unitData == primary)
            {
                RefundWoodFood(team, data.ScaledTrainWoodCost, data.ScaledTrainFoodCost);
                return;
            }

            if (unitData == data.secondaryTrainUnitData)
                RefundWoodFood(team, data.ScaledSecondaryTrainWoodCost, data.ScaledSecondaryTrainFoodCost);
        }

        public static void RefundArcheryRangeJob(UnitTeam team, ArcheryRange archeryRange, UnitData unitData)
        {
            if (archeryRange == null || archeryRange.Data == null || unitData == null)
                return;

            PlacedBuildingData data = archeryRange.Data;
            if (unitData != data.trainUnitData)
                return;

            RefundWoodFood(team, data.ScaledTrainWoodCost, data.ScaledTrainFoodCost);
        }

        public static void RefundStableJob(UnitTeam team, Stable stable, UnitData unitData)
        {
            if (stable == null || stable.Data == null || unitData == null)
                return;

            PlacedBuildingData data = stable.Data;
            if (unitData == data.trainUnitData)
            {
                RefundWoodFood(team, data.ScaledTrainWoodCost, data.ScaledTrainFoodCost);
                return;
            }

            if (unitData == data.secondaryTrainUnitData)
                RefundWoodFood(team, data.ScaledSecondaryTrainWoodCost, data.ScaledSecondaryTrainFoodCost);
        }

        static void RefundWoodFood(UnitTeam team, float wood, float food)
        {
            if (wood > 0f)
                ResourceManager.AddWood(team, wood);

            if (food > 0f)
                ResourceManager.AddFood(team, food);
        }
    }
}
