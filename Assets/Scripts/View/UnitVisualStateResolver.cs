using AoE.RTS.Economy;
using AoE.RTS.Units;

namespace AoE.RTS.View
{
    public static class UnitVisualStateResolver
    {
        public static UnitVisualState Resolve(Unit unit)
        {
            if (unit == null || !unit.IsAlive)
                return UnitVisualState.Dead;

            if (IsGathering(unit))
                return UnitVisualState.Gather;

            switch (unit.State)
            {
                case UnitState.Attack:
                    return UnitVisualState.Attack;
                case UnitState.Move:
                    return UnitVisualState.Walk;
                case UnitState.Dead:
                    return UnitVisualState.Dead;
                default:
                    return UnitVisualState.Idle;
            }
        }

        static bool IsGathering(Unit unit)
        {
            return GatherManager.IsUnitGathering(unit)
                || FoodGatherManager.IsUnitGathering(unit)
                || MineralGatherManager.IsUnitGathering(unit);
        }
    }
}
