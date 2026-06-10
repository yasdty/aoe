using AoE.RTS.Buildings;
using AoE.RTS.Economy;
using AoE.RTS.Units;

namespace AoE.RTS.AI
{
    public static class CpuAiCoordination
    {
        public const float BarracksBuildDelaySeconds = 60f;
        public const UnitTeam CpuTeam = UnitTeam.Enemy;

        public static bool ShouldDeferEconomyBuildings()
        {
            return !BarracksProductionManager.HasBarracksForTeam(CpuTeam);
        }

        public static bool HasWoodReserveForBarracks(float scaledExtraCost = 0f)
        {
            if (BarracksProductionManager.HasBarracksForTeam(CpuTeam))
                return true;

            PlacedBuildingData barracksData = null;
            barracksData = PlacedBuildingDataResolver.ResolveBarracks(ref barracksData);
            float scaledBarracksCost = barracksData != null ? barracksData.ScaledWoodCost : 0f;
            return ResourceManager.GetWood(CpuTeam) >= scaledBarracksCost + scaledExtraCost;
        }

        public static bool HasActiveCpuConstruction()
        {
            return BuildingPlacementManager.HasActiveConstructionForTeam(CpuTeam);
        }
    }
}
