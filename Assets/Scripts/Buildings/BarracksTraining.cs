using AoE.RTS.Core;
using AoE.RTS.Units;

namespace AoE.RTS.Buildings
{
    public static class BarracksTraining
    {
        static TechnologyData cachedInfantryTech;

        public static UnitData ResolvePrimaryTrainUnit(Barracks barracks)
        {
            if (barracks == null || barracks.Data == null)
                return null;

            TechnologyData tech = TechnologyDataResolver.ResolveInfantryUpgrade(ref cachedInfantryTech);
            UnitData upgraded = tech != null ? tech.outputUnitData : null;
            return TechnologyState.ResolveBarracksPrimaryUnit(
                barracks.Team,
                barracks.Data.trainUnitData,
                upgraded);
        }

        public static string GetPrimaryTrainDisplayName(Barracks barracks)
        {
            UnitData unit = ResolvePrimaryTrainUnit(barracks);
            return unit != null ? unit.displayName : "Militia";
        }
    }
}
