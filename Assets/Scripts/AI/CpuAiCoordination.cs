using AoE.RTS.Buildings;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Units;

namespace AoE.RTS.AI
{
    public static class CpuAiCoordination
    {
        public const float BarracksBuildDelaySeconds = 60f;
        public const UnitTeam CpuTeam = UnitTeam.Enemy;

        public static bool ShouldDeferEconomyBuildings(PlayerId cpuPlayerId) =>
            !PlayerBuildingQueries.HasBarracksForPlayer(cpuPlayerId);

        public static bool ShouldDeferEconomyBuildings(UnitTeam cpuTeam) =>
            ShouldDeferEconomyBuildings(PlayerIdMapping.FromLegacyTeam(cpuTeam));

        public static bool ShouldDeferEconomyBuildings() =>
            ShouldDeferEconomyBuildings(CpuTeam);

        public static bool HasWoodReserveForBarracks(PlayerId cpuPlayerId, float scaledExtraCost = 0f)
        {
            if (PlayerBuildingQueries.HasBarracksForPlayer(cpuPlayerId))
                return true;

            PlacedBuildingData barracksData = null;
            barracksData = PlacedBuildingDataResolver.ResolveBarracks(ref barracksData);
            float scaledBarracksCost = barracksData != null ? barracksData.ScaledWoodCost : 0f;
            return ResourceManager.GetWood(cpuPlayerId) >= scaledBarracksCost + scaledExtraCost;
        }

        public static bool HasWoodReserveForBarracks(UnitTeam cpuTeam, float scaledExtraCost = 0f) =>
            HasWoodReserveForBarracks(PlayerIdMapping.FromLegacyTeam(cpuTeam), scaledExtraCost);

        public static bool HasWoodReserveForBarracks(float scaledExtraCost = 0f) =>
            HasWoodReserveForBarracks(CpuTeam, scaledExtraCost);

        public static bool HasActiveCpuConstruction(PlayerId cpuPlayerId) =>
            BuildingPlacementManager.HasActiveConstructionForPlayer(cpuPlayerId);

        public static bool HasActiveCpuConstruction(UnitTeam cpuTeam) =>
            HasActiveCpuConstruction(PlayerIdMapping.FromLegacyTeam(cpuTeam));

        public static bool HasActiveCpuConstruction() => HasActiveCpuConstruction(CpuTeam);
    }
}
