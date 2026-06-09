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
            cached.secondaryTrainTime = 4f;
            cached.secondaryTrainWoodCost = 25f;
            cached.secondaryTrainFoodCost = 35f;
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

        public static PlacedBuildingData ResolveMiningCamp(ref PlacedBuildingData cached)
        {
            if (cached != null)
                return cached;

#if UNITY_EDITOR
            cached = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(GameAssetPaths.DefaultMiningCampData);
            if (cached != null)
                return cached;
#endif

            cached = ScriptableObject.CreateInstance<PlacedBuildingData>();
            cached.kind = PlacedBuildingKind.MiningCamp;
            cached.displayName = "Mining Camp";
            cached.woodCost = 100f;
            cached.buildTime = 6f;
            cached.housingProvided = 0;
            cached.maxHp = 400f;
            cached.defaultColor = new Color(0.45f, 0.48f, 0.52f);
            return cached;
        }

        public static PlacedBuildingData ResolveMill(ref PlacedBuildingData cached)
        {
            if (cached != null)
                return cached;

#if UNITY_EDITOR
            cached = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(GameAssetPaths.DefaultMillData);
            if (cached != null)
                return cached;
#endif

            cached = ScriptableObject.CreateInstance<PlacedBuildingData>();
            cached.kind = PlacedBuildingKind.Mill;
            cached.displayName = "Mill";
            cached.woodCost = 100f;
            cached.buildTime = 6f;
            cached.housingProvided = 0;
            cached.maxHp = 400f;
            cached.defaultColor = new Color(0.62f, 0.52f, 0.38f);
            return cached;
        }

        public static PlacedBuildingData ResolveArcheryRange(ref PlacedBuildingData cached)
        {
            if (cached != null)
                return cached;

#if UNITY_EDITOR
            cached = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(GameAssetPaths.DefaultArcheryRangeData);
            if (cached != null)
                return cached;
#endif

            cached = ScriptableObject.CreateInstance<PlacedBuildingData>();
            cached.kind = PlacedBuildingKind.ArcheryRange;
            cached.displayName = "Archery Range";
            cached.woodCost = 150f;
            cached.buildTime = 40f;
            cached.footprintWidth = 6f;
            cached.footprintDepth = 6f;
            cached.buildingHeight = 3.5f;
            cached.housingProvided = 0;
            cached.trainTime = 3f;
            cached.trainWoodCost = 25f;
            cached.trainFoodCost = 25f;
            cached.maxHp = 300f;
            return cached;
        }

        public static PlacedBuildingData ResolveStable(ref PlacedBuildingData cached)
        {
            if (cached != null)
                return cached;

#if UNITY_EDITOR
            cached = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(GameAssetPaths.DefaultStableData);
            if (cached != null)
                return cached;
#endif

            cached = ScriptableObject.CreateInstance<PlacedBuildingData>();
            cached.kind = PlacedBuildingKind.Stable;
            cached.displayName = "Stable";
            cached.woodCost = 150f;
            cached.buildTime = 40f;
            cached.footprintWidth = 6f;
            cached.footprintDepth = 6f;
            cached.buildingHeight = 3.5f;
            cached.housingProvided = 0;
            cached.trainTime = 5f;
            cached.trainWoodCost = 20f;
            cached.trainFoodCost = 60f;
            cached.secondaryTrainTime = 6f;
            cached.secondaryTrainWoodCost = 0f;
            cached.secondaryTrainFoodCost = 80f;
            cached.maxHp = 300f;
            return cached;
        }
    }
}
