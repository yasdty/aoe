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
            cached.woodCost = 30f;
            cached.buildTime = 25f;
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
            cached.woodCost = 175f;
            cached.buildTime = 50f;
            cached.footprintWidth = 6f;
            cached.footprintDepth = 6f;
            cached.buildingHeight = 3.5f;
            cached.housingProvided = 0;
            cached.trainTime = 21f;
            cached.trainWoodCost = 0f;
            cached.trainFoodCost = 60f;
            cached.secondaryTrainTime = 22f;
            cached.secondaryTrainWoodCost = 22f;
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
            cached.buildTime = 25f;
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
            cached.buildTime = 25f;
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
            cached.buildTime = 25f;
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
            cached.buildTime = 25f;
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
            cached.requiredAge = GameAge.Feudal;
            cached.woodCost = 175f;
            cached.buildTime = 50f;
            cached.footprintWidth = 6f;
            cached.footprintDepth = 6f;
            cached.buildingHeight = 3.5f;
            cached.housingProvided = 0;
            cached.trainTime = 35f;
            cached.trainWoodCost = 35f;
            cached.trainFoodCost = 35f;
            cached.maxHp = 300f;
            return cached;
        }

        public static PlacedBuildingData ResolveBlacksmith(ref PlacedBuildingData cached)
        {
            if (cached != null)
                return cached;

#if UNITY_EDITOR
            cached = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(GameAssetPaths.DefaultBlacksmithData);
            if (cached != null)
                return cached;
#endif

            cached = ScriptableObject.CreateInstance<PlacedBuildingData>();
            cached.kind = PlacedBuildingKind.Blacksmith;
            cached.displayName = "Blacksmith";
            cached.requiredAge = GameAge.Feudal;
            cached.woodCost = 150f;
            cached.buildTime = 40f;
            cached.footprintWidth = 6f;
            cached.footprintDepth = 6f;
            cached.buildingHeight = 3.5f;
            cached.housingProvided = 0;
            cached.maxHp = 300f;
            cached.defaultColor = new Color(0.5f, 0.5f, 0.55f);
            cached.selectedColor = new Color(0.85f, 0.85f, 0.95f);
            return cached;
        }

        public static PlacedBuildingData ResolvePalisadeWall(ref PlacedBuildingData cached)
        {
            if (cached != null)
                return cached;

#if UNITY_EDITOR
            cached = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(GameAssetPaths.DefaultPalisadeWallData);
            if (cached != null)
                return cached;
#endif

            cached = ScriptableObject.CreateInstance<PlacedBuildingData>();
            cached.kind = PlacedBuildingKind.PalisadeWall;
            cached.displayName = "Palisade Wall";
            cached.requiredAge = GameAge.Dark;
            cached.woodCost = 2f;
            cached.buildTime = 7f;
            cached.footprintWidth = 1f;
            cached.footprintDepth = 4f;
            cached.buildingHeight = 2f;
            cached.housingProvided = 0;
            cached.maxHp = 250f;
            cached.defaultColor = new Color(0.6f, 0.45f, 0.25f);
            return cached;
        }

        public static PlacedBuildingData ResolveStoneWall(ref PlacedBuildingData cached)
        {
            if (cached != null)
                return cached;

#if UNITY_EDITOR
            cached = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(GameAssetPaths.DefaultStoneWallData);
            if (cached != null)
                return cached;
#endif

            cached = ScriptableObject.CreateInstance<PlacedBuildingData>();
            cached.kind = PlacedBuildingKind.StoneWall;
            cached.displayName = "Stone Wall";
            cached.requiredAge = GameAge.Feudal;
            cached.stoneCost = 5f;
            cached.buildTime = 8f;
            cached.footprintWidth = 1f;
            cached.footprintDepth = 4f;
            cached.buildingHeight = 2.5f;
            cached.housingProvided = 0;
            cached.maxHp = 900f;
            cached.meleeArmor = 0f;
            cached.pierceArmor = 8f;
            cached.defaultColor = new Color(0.55f, 0.55f, 0.58f);
            return cached;
        }

        public static PlacedBuildingData ResolveGate(ref PlacedBuildingData cached)
        {
            if (cached != null)
                return cached;

#if UNITY_EDITOR
            cached = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(GameAssetPaths.DefaultGateData);
            if (cached != null)
                return cached;
#endif

            cached = ScriptableObject.CreateInstance<PlacedBuildingData>();
            cached.kind = PlacedBuildingKind.Gate;
            cached.displayName = "Gate";
            cached.requiredAge = GameAge.Feudal;
            cached.woodCost = 30f;
            cached.buildTime = 12f;
            cached.footprintWidth = 2f;
            cached.footprintDepth = 4f;
            cached.buildingHeight = 2.5f;
            cached.housingProvided = 0;
            cached.maxHp = 950f;
            cached.meleeArmor = 0f;
            cached.pierceArmor = 8f;
            cached.defaultColor = new Color(0.62f, 0.5f, 0.35f);
            return cached;
        }

        public static PlacedBuildingData ResolveWatchTower(ref PlacedBuildingData cached)
        {
            if (cached != null)
                return cached;

#if UNITY_EDITOR
            cached = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(GameAssetPaths.DefaultWatchTowerData);
            if (cached != null)
                return cached;
#endif

            cached = ScriptableObject.CreateInstance<PlacedBuildingData>();
            cached.kind = PlacedBuildingKind.WatchTower;
            cached.displayName = "Watch Tower";
            cached.requiredAge = GameAge.Feudal;
            cached.stoneCost = 125f;
            cached.buildTime = 80f;
            cached.footprintWidth = 4f;
            cached.footprintDepth = 4f;
            cached.buildingHeight = 5f;
            cached.housingProvided = 0;
            cached.maxHp = 1020f;
            cached.meleeArmor = 0f;
            cached.pierceArmor = 8f;
            cached.towerAttack = 5f;
            cached.towerAttackRange = 7f;
            cached.towerAttackCooldown = 2f;
            cached.defaultColor = new Color(0.5f, 0.52f, 0.55f);
            cached.selectedColor = new Color(0.85f, 0.8f, 0.55f);
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
            cached.requiredAge = GameAge.Feudal;
            cached.woodCost = 175f;
            cached.buildTime = 50f;
            cached.footprintWidth = 6f;
            cached.footprintDepth = 6f;
            cached.buildingHeight = 3.5f;
            cached.housingProvided = 0;
            cached.trainTime = 30f;
            cached.trainWoodCost = 75f;
            cached.trainFoodCost = 60f;
            cached.secondaryTrainTime = 30f;
            cached.secondaryTrainWoodCost = 0f;
            cached.secondaryTrainFoodCost = 80f;
            cached.maxHp = 300f;
            return cached;
        }

        public static PlacedBuildingData ResolveMarket(ref PlacedBuildingData cached)
        {
            if (cached != null)
                return cached;

#if UNITY_EDITOR
            cached = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(GameAssetPaths.DefaultMarketData);
            if (cached != null)
                return cached;
#endif

            cached = ScriptableObject.CreateInstance<PlacedBuildingData>();
            cached.kind = PlacedBuildingKind.Market;
            cached.displayName = "Market";
            cached.requiredAge = GameAge.Feudal;
            cached.woodCost = 175f;
            cached.buildTime = 60f;
            cached.footprintWidth = 6f;
            cached.footprintDepth = 6f;
            cached.buildingHeight = 3.5f;
            cached.housingProvided = 0;
            cached.maxHp = 400f;
            cached.defaultColor = new Color(0.58f, 0.48f, 0.32f);
            cached.selectedColor = new Color(0.85f, 0.8f, 0.55f);
            return cached;
        }

        public static PlacedBuildingData ResolveTownCenterPlacement(ref PlacedBuildingData cached)
        {
            if (cached != null)
                return cached;

#if UNITY_EDITOR
            cached = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(GameAssetPaths.DefaultTownCenterPlacementData);
            if (cached != null)
                return cached;
#endif

            cached = ScriptableObject.CreateInstance<PlacedBuildingData>();
            cached.kind = PlacedBuildingKind.TownCenter;
            cached.displayName = "Town Center";
            cached.requiredAge = GameAge.Feudal;
            cached.woodCost = 275f;
            cached.stoneCost = 100f;
            cached.buildTime = 150f;
            cached.footprintWidth = 8f;
            cached.footprintDepth = 8f;
            cached.buildingHeight = 4f;
            cached.housingProvided = 0;
            cached.maxHp = 400f;
            cached.defaultColor = new Color(0.75f, 0.65f, 0.45f);
            cached.selectedColor = new Color(0.95f, 0.85f, 0.35f);
            return cached;
        }
    }
}
