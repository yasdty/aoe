using AoE.RTS.Units;

namespace AoE.RTS.Combat
{
    public static class CombatFeedbackClassifier
    {
        public static bool IsRangedUnit(Unit unit)
        {
            if (unit == null || unit.Data == null)
                return false;

            return unit.Data.attackDamageType == AttackDamageType.Pierce && unit.Data.attackRange > 3f;
        }
    }
}
