using AoE.RTS.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AoE.RTS.Units
{
    public static class UnitDataResolver
    {
        static UnitData cachedDefaultVillager;
        static UnitData cachedMilitia;

        public static UnitData ResolveDefaultVillager(ref UnitData cached)
        {
            if (cached != null)
                return cached;

#if UNITY_EDITOR
            cached = AssetDatabase.LoadAssetAtPath<UnitData>(GameAssetPaths.DefaultUnitData);
#endif
            return cached;
        }

        public static UnitData ResolveMilitia(ref UnitData cached)
        {
            if (cached != null)
                return cached;

#if UNITY_EDITOR
            cached = AssetDatabase.LoadAssetAtPath<UnitData>(GameAssetPaths.MilitiaData);
#endif
            return cached;
        }

        public static void EnsureUnitHasData(Unit unit)
        {
            if (unit == null || unit.Data != null)
                return;

            UnitData villager = ResolveDefaultVillager(ref cachedDefaultVillager);
            if (villager == null)
                return;

            unit.SetData(villager);
        }
    }
}
