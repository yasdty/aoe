using AoE.RTS.Units;

namespace AoE.RTS.Buildings
{
    public static class ProductionQueueDisplayUtility
    {
        public static string GetTownCenterEntryName(TownCenter townCenter, UnitData unitData)
        {
            if (townCenter?.Data?.villagerUnitData != null && unitData == townCenter.Data.villagerUnitData)
                return "Villager";

            // TC MVP は村民のみ — displayName 未設定の UnitData でも正しく表示する
            if (townCenter != null)
                return "Villager";

            return ResolveUnitDisplayName(unitData);
        }

        public static string GetBarracksEntryName(Barracks barracks, UnitData unitData)
        {
            if (barracks != null && barracks.Data != null && unitData != null)
            {
                UnitData primary = BarracksTraining.ResolvePrimaryTrainUnit(barracks);
                if (unitData == primary)
                    return BarracksTraining.GetPrimaryTrainDisplayName(barracks);

                if (unitData == barracks.Data.secondaryTrainUnitData)
                    return ResolveNamedUnit(barracks.Data.secondaryTrainUnitData, "Spearman");
            }

            return ResolveUnitDisplayName(unitData);
        }

        public static string GetArcheryRangeEntryName(ArcheryRange archeryRange, UnitData unitData)
        {
            if (archeryRange?.Data?.trainUnitData != null && unitData == archeryRange.Data.trainUnitData)
                return ResolveNamedUnit(archeryRange.Data.trainUnitData, "Archer");

            return ResolveUnitDisplayName(unitData);
        }

        public static string GetStableEntryName(Stable stable, UnitData unitData)
        {
            if (stable?.Data == null || unitData == null)
                return ResolveUnitDisplayName(unitData);

            if (unitData == stable.Data.trainUnitData)
                return ResolveNamedUnit(stable.Data.trainUnitData, "Cavalry");

            if (unitData == stable.Data.secondaryTrainUnitData)
                return ResolveNamedUnit(stable.Data.secondaryTrainUnitData, "Scout");

            return ResolveUnitDisplayName(unitData);
        }

        static string ResolveNamedUnit(UnitData unitData, string fallback)
        {
            string name = unitData != null ? unitData.displayName : null;
            if (string.IsNullOrWhiteSpace(name) || name == "Unit")
                return fallback;

            return name;
        }

        public static string ResolveUnitDisplayName(UnitData unitData)
        {
            if (unitData == null)
                return "Unit";

            string name = unitData.displayName;
            return string.IsNullOrWhiteSpace(name) ? "Unit" : name;
        }
    }
}
