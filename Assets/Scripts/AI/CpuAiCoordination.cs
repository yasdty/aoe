using AoE.RTS.Buildings;
using AoE.RTS.Economy;
using AoE.RTS.Units;

namespace AoE.RTS.AI
{
    public static class CpuAiCoordination
    {
        public const float BarracksBuildDelaySeconds = 60f;
        public const UnitTeam CpuTeam = UnitTeam.Enemy;

        public static bool ShouldDeferEconomyBuildings(UnitTeam cpuTeam)
        {
            return !BarracksProductionManager.HasBarracksForTeam(cpuTeam);
        }

        public static bool ShouldDeferEconomyBuildings() => ShouldDeferEconomyBuildings(CpuTeam);

        public static bool HasWoodReserveForBarracks(UnitTeam cpuTeam, float scaledExtraCost = 0f)
        {
            if (BarracksProductionManager.HasBarracksForTeam(cpuTeam))
                return true;

            PlacedBuildingData barracksData = null;
            barracksData = PlacedBuildingDataResolver.ResolveBarracks(ref barracksData);
            float scaledBarracksCost = barracksData != null ? barracksData.ScaledWoodCost : 0f;
            return ResourceManager.GetWood(cpuTeam) >= scaledBarracksCost + scaledExtraCost;
        }

        public static bool HasWoodReserveForBarracks(float scaledExtraCost = 0f) =>
            HasWoodReserveForBarracks(CpuTeam, scaledExtraCost);

        public static bool HasActiveCpuConstruction(UnitTeam cpuTeam)
        {
            return BuildingPlacementManager.HasActiveConstructionForTeam(cpuTeam);
        }

        public static bool HasActiveCpuConstruction() => HasActiveCpuConstruction(CpuTeam);
    }
}
