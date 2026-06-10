using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AoE.RTS.Core
{
    public static class TechnologyDataResolver
    {
        static TechnologyData cachedInfantryUpgrade;

        public static TechnologyData ResolveInfantryUpgrade(ref TechnologyData cached)
        {
            if (cached != null)
                return cached;

#if UNITY_EDITOR
            cached = AssetDatabase.LoadAssetAtPath<TechnologyData>(GameAssetPaths.InfantryUpgradeTech);
            if (cached != null)
                return cached;
#endif

            if (cachedInfantryUpgrade == null)
            {
                cachedInfantryUpgrade = ScriptableObject.CreateInstance<TechnologyData>();
                cachedInfantryUpgrade.kind = TechnologyKind.InfantryUpgrade;
                cachedInfantryUpgrade.displayName = "Infantry Upgrade";
                cachedInfantryUpgrade.prerequisiteAge = GameAge.Feudal;
                cachedInfantryUpgrade.foodCost = 100f;
                cachedInfantryUpgrade.goldCost = 50f;
                cachedInfantryUpgrade.researchTimeSeconds = 75f;
            }

            cached = cachedInfantryUpgrade;
            return cached;
        }
    }
}
