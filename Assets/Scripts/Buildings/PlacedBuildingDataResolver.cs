using AoE.RTS.Core;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AoE.RTS.Buildings
{
    public static class PlacedBuildingDataResolver
    {
        public static PlacedBuildingData Resolve(ref PlacedBuildingData cached)
        {
            return ResolveHouse(ref cached);
        }

        public static PlacedBuildingData ResolveHouse(ref PlacedBuildingData cached)
        {
            if (cached != null)
                return cached;

#if UNITY_EDITOR
            cached = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(GameAssetPaths.DefaultHouseData);
            if (cached != null)
                return cached;
#endif

            cached = ScriptableObject.CreateInstance<PlacedBuildingData>();
            cached.kind = PlacedBuildingKind.House;
            cached.displayName = "House";
            cached.woodCost = 25f;
            cached.buildTime = 3f;
            cached.housingProvided = 5;
            return cached;
        }

        public static PlacedBuildingData ResolveBarracks(ref PlacedBuildingData cached)
        {
            if (cached != null)
                return cached;

#if UNITY_EDITOR
            cached = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(GameAssetPaths.DefaultBarracksData);
            if (cached != null)
                return cached;
#endif

            cached = ScriptableObject.CreateInstance<PlacedBuildingData>();
            cached.kind = PlacedBuildingKind.Barracks;
            cached.displayName = "Barracks";
            cached.woodCost = 50f;
            cached.buildTime = 5f;
            cached.footprintWidth = 6f;
            cached.footprintDepth = 6f;
            cached.buildingHeight = 3.5f;
            cached.housingProvided = 0;
            cached.trainTime = 3f;
            cached.trainWoodCost = 20f;
            return cached;
        }

        public static PlacedBuildingData ResolveFarm(ref PlacedBuildingData cached)
        {
            if (cached != null)
                return cached;

#if UNITY_EDITOR
            cached = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(GameAssetPaths.DefaultFarmData);
            if (cached != null)
                return cached;
#endif

            cached = ScriptableObject.CreateInstance<PlacedBuildingData>();
            cached.kind = PlacedBuildingKind.Farm;
            cached.displayName = "Farm";
            cached.woodCost = 60f;
            cached.buildTime = 8f;
            cached.housingProvided = 0;
            cached.foodCapacity = 250f;
            cached.maxHp = 100f;
            cached.defaultColor = new Color(0.35f, 0.7f, 0.25f);
            return cached;
        }

        public static PlacedBuildingData ResolveLumberCamp(ref PlacedBuildingData cached)
        {
            if (cached != null)
                return cached;

#if UNITY_EDITOR
            cached = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(GameAssetPaths.DefaultLumberCampData);
            if (cached != null)
                return cached;
#endif

            cached = ScriptableObject.CreateInstance<PlacedBuildingData>();
            cached.kind = PlacedBuildingKind.LumberCamp;
            cached.displayName = "Lumber Camp";
            cached.woodCost = 100f;
            cached.buildTime = 6f;
            cached.housingProvided = 0;
            cached.maxHp = 400f;
            cached.defaultColor = new Color(0.55f, 0.38f, 0.22f);
            return cached;
        }
    }
}
