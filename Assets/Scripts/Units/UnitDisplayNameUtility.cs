using AoE.RTS.Core;

namespace AoE.RTS.Units
{
    public static class UnitDisplayNameUtility
    {
        public static string GetDisplayName(Unit unit)
        {
            return GetDisplayName(unit != null ? unit.Data : null);
        }

        public static string GetDisplayName(UnitData data)
        {
            return Localization.UnitName(data);
        }
    }
}
