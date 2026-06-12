using AoE.RTS.Units;

namespace AoE.RTS.Combat
{
    public static class CombatDeathScheduler
    {
        public delegate void ReturnHandler(Unit unit, float delaySeconds);

        const float DefaultDelaySeconds = 0.3f;

        static ReturnHandler handler;

        public static void SetHandler(ReturnHandler returnHandler)
        {
            handler = returnHandler;
        }

        public static void ScheduleReturn(Unit unit, float delaySeconds = DefaultDelaySeconds)
        {
            if (unit == null)
                return;

            if (handler != null)
                handler(unit, delaySeconds);
            else
                UnitPool.Return(unit);
        }
    }
}
