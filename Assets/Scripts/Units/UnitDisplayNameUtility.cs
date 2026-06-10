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
            if (data == null)
                return "Unit";

            string name = data.displayName;
            if (!string.IsNullOrWhiteSpace(name) && name != "Unit")
                return name;

            return data.CanAttack ? "Militia" : "Villager";
        }
    }
}
