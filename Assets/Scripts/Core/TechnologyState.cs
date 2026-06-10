using AoE.RTS.Units;

namespace AoE.RTS.Core
{
    public static class TechnologyState
    {
        static bool playerInfantryUpgrade;
        static bool enemyInfantryUpgrade;

        public static void Reset()
        {
            playerInfantryUpgrade = false;
            enemyInfantryUpgrade = false;
        }

        public static bool HasInfantryUpgrade(UnitTeam team)
        {
            return team == UnitTeam.Enemy ? enemyInfantryUpgrade : playerInfantryUpgrade;
        }

        public static void CompleteInfantryUpgrade(UnitTeam team)
        {
            if (team == UnitTeam.Enemy)
                enemyInfantryUpgrade = true;
            else
                playerInfantryUpgrade = true;
        }

        public static UnitData ResolveBarracksPrimaryUnit(UnitTeam team, UnitData defaultUnit, UnitData upgradedUnit)
        {
            if (HasInfantryUpgrade(team) && upgradedUnit != null)
                return upgradedUnit;

            return defaultUnit;
        }
    }
}
