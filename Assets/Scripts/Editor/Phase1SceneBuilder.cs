using AoE.RTS.Buildings;
using AoE.RTS.Camera;
using AoE.RTS.Combat;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Input;
using AoE.RTS.Selection;
using AoE.RTS.Units;
using AoE.RTS.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace AoE.RTS.EditorTools
{
    public static class Phase1SceneBuilder
    {
        const string ScenePath = "Assets/Scenes/Phase1.unity";
        const string UnitDataPath = "Assets/Data/UnitData/DefaultUnit.asset";
        const string TownCenterDataPath = "Assets/Data/BuildingData/TownCenterData.asset";
        const string DefaultTreeDataPath = GameAssetPaths.DefaultTreeData;
        const string DefaultBerryBushDataPath = GameAssetPaths.DefaultBerryBushData;
        const string DefaultDeerDataPath = GameAssetPaths.DefaultDeerData;
        const string DefaultSheepDataPath = GameAssetPaths.DefaultSheepData;
        const string DefaultBoarDataPath = GameAssetPaths.DefaultBoarData;
        const string DefaultGoldMineDataPath = GameAssetPaths.DefaultGoldMineData;
        const string DefaultStoneMineDataPath = GameAssetPaths.DefaultStoneMineData;
        const string DefaultHouseDataPath = GameAssetPaths.DefaultHouseData;
        const string DefaultFarmDataPath = GameAssetPaths.DefaultFarmData;
        const string DefaultLumberCampDataPath = GameAssetPaths.DefaultLumberCampData;
        const string DefaultMiningCampDataPath = GameAssetPaths.DefaultMiningCampData;
        const string DefaultMillDataPath = GameAssetPaths.DefaultMillData;
        const string DefaultBarracksDataPath = GameAssetPaths.DefaultBarracksData;
        const string DefaultArcheryRangeDataPath = GameAssetPaths.DefaultArcheryRangeData;
        const string DefaultStableDataPath = GameAssetPaths.DefaultStableData;
        const string MilitiaDataPath = GameAssetPaths.MilitiaData;
        const string ManAtArmsDataPath = GameAssetPaths.ManAtArmsData;
        const string InfantryUpgradeTechPath = GameAssetPaths.InfantryUpgradeTech;
        const string SpearmanDataPath = GameAssetPaths.SpearmanData;
        const string ArcherDataPath = GameAssetPaths.ArcherData;
        const string CavalryDataPath = GameAssetPaths.CavalryData;
        const string ScoutDataPath = GameAssetPaths.ScoutData;
        const string EnemyDummyDataPath = GameAssetPaths.EnemyDummyData;
        const string FeudalAgeDataPath = GameAssetPaths.FeudalAgeData;
        const string DefaultBlacksmithDataPath = GameAssetPaths.DefaultBlacksmithData;
        const string DefaultPalisadeWallDataPath = GameAssetPaths.DefaultPalisadeWallData;
        const string DefaultStoneWallDataPath = GameAssetPaths.DefaultStoneWallData;
        const string DefaultWatchTowerDataPath = GameAssetPaths.DefaultWatchTowerData;
        const string DefaultMarketDataPath = GameAssetPaths.DefaultMarketData;
        const string DefaultMarketTradeDataPath = GameAssetPaths.DefaultMarketTradeData;
        const string DefaultTownCenterPlacementDataPath = GameAssetPaths.DefaultTownCenterPlacementData;
        const string DefaultPlayerCivilizationDataPath = GameAssetPaths.DefaultPlayerCivilizationData;
        const string DefaultCpuCivilizationDataPath = GameAssetPaths.DefaultCpuCivilizationData;

        public static bool EnsureEditModeForSceneSetup()
        {
            if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
                return true;

            Debug.LogWarning("Stop Play mode before running AoE scene setup menus.");
            return false;
        }

        [MenuItem("AoE/Sync AoE2 Game Data", true)]
        static bool ValidateSyncAoe2GameData() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Sync AoE2 Game Data")]
        public static void SyncAoe2GameData()
        {
            if (!EnsureEditModeForSceneSetup())
                return;

            UnitData villagerData = EnsureDefaultUnitData();
            UnitData militiaData = EnsureMilitiaData();
            UnitData spearmanData = EnsureSpearmanData();
            UnitData archerData = EnsureArcherData();
            UnitData cavalryData = EnsureCavalryData();
            UnitData scoutData = EnsureScoutData();
            EnsureTownCenterData(villagerData);
            EnsureHouseData();
            EnsureFarmData();
            EnsureLumberCampData();
            EnsureMiningCampData();
            EnsureMillData();
            EnsureBarracksData(militiaData, spearmanData);
            EnsureArcheryRangeData(archerData);
            EnsureStableData(cavalryData, scoutData);
            UnitData manAtArmsData = EnsureManAtArmsData();
            EnsureBlacksmithData();
            EnsureInfantryUpgradeTech(militiaData, manAtArmsData);
            EnsurePalisadeWallData();
            EnsureStoneWallData();
            EnsureWatchTowerData();
            EnsureMarketData();
            EnsureMarketTradeData();
            EnsureDefaultPlayerCivilizationData();
            EnsureDefaultCpuCivilizationData();
            EnsureTownCenterPlacementData();
            EnsureFeudalAgeData();
            Debug.Log("Synced AoE2 game data assets.");
        }

        [MenuItem("AoE/Fix Phase1 Input References", true)]
        static bool ValidateFixPhase1InputReferences() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Fix Phase1 Input References")]
        public static void FixPhase1InputReferences()
        {
            if (!EnsureEditModeForSceneSetup())
                return;

            RTSInputActionsProjectSettings.ClearStaleProjectWideBinding();

            InputActionAsset inputActions;
            try
            {
                inputActions = RTSInputActionsFactory.EnsureAsset();
            }
            catch (System.Exception exception)
            {
                Debug.LogError("Failed to ensure RTSInputActions: " + exception.Message);
                return;
            }

            if (!System.IO.File.Exists(ScenePath))
            {
                Debug.LogWarning("Phase1 scene not found. Run AoE → Setup Phase1 Scene first.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            AssignInputActionsToReaders(inputActions);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                Debug.LogError("Failed to save Phase1 scene. Check Console for Input System import errors.");
                return;
            }

            Debug.Log("Phase1 input references updated.");
        }

        [MenuItem("AoE/Setup Phase1 Scene", true)]
        static bool ValidateSetupPhase1Scene() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Setup Phase1 Scene")]
        public static void SetupPhase1Scene()
        {
            if (!EnsureEditModeForSceneSetup())
                return;

            EnsureLayers();
            UnitData unitData = EnsureDefaultUnitData();
            InputActionAsset inputActions = RTSInputActionsFactory.EnsureAsset();
            if (inputActions == null)
            {
                Debug.LogError("Failed to create RTSInputActions. Setup aborted.");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateLighting();
            CreateGround();
            GameObject unit = CreateUnit(unitData, new Vector3(0f, 1f, 0f));
            GameObject cameraRig = CreateCameraRig(inputActions);
            CreateManagers(inputActions, cameraRig.GetComponent<UnityEngine.Camera>());

            AssignInputActionsToReaders(inputActions);
            EditorSceneManager.SaveScene(scene, ScenePath);
            UnityEditor.Selection.activeGameObject = unit;

            Debug.Log("Phase1 scene created at " + ScenePath);
        }

        public static void AssignInputActionsToReaders(InputActionAsset inputActions)
        {
            RTSInputReader[] readers = Object.FindObjectsByType<RTSInputReader>(FindObjectsInactive.Include);
            for (int i = 0; i < readers.Length; i++)
            {
                SerializedObject serialized = new SerializedObject(readers[i]);
                serialized.FindProperty("inputActions").objectReferenceValue = inputActions;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(readers[i]);
            }
        }

        public static void EnsureLayers()
        {
            RenderPipelineSetup.EnsureRenderPipeline();

            SerializedObject tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            SetLayerName(layers, 8, "Ground");
            SetLayerName(layers, 9, "Unit");
            SetLayerName(layers, 10, "Building");
            SetLayerName(layers, 11, "Resource");
            tagManager.ApplyModifiedProperties();
        }

        static void SetLayerName(SerializedProperty layers, int index, string layerName)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(index);
            if (string.IsNullOrEmpty(layer.stringValue))
                layer.stringValue = layerName;
        }

        public static UnitData EnsureDefaultUnitData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/UnitData"))
                AssetDatabase.CreateFolder("Assets/Data", "UnitData");

            UnitData existing = AssetDatabase.LoadAssetAtPath<UnitData>(UnitDataPath);
            if (existing != null)
            {
                bool dirty = SyncVillagerStats(existing);
                if (dirty)
                {
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }

                return existing;
            }

            UnitData data = ScriptableObject.CreateInstance<UnitData>();
            data.displayName = "Villager";
            AssetDatabase.CreateAsset(data, UnitDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static BuildingData EnsureTownCenterData(UnitData villagerData)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/BuildingData"))
                AssetDatabase.CreateFolder("Assets/Data", "BuildingData");

            BuildingData existing = AssetDatabase.LoadAssetAtPath<BuildingData>(TownCenterDataPath);
            if (existing != null)
            {
                if (existing.villagerUnitData == null && villagerData != null)
                {
                    existing.villagerUnitData = villagerData;
                    EditorUtility.SetDirty(existing);
                }

                if (existing.spawnClearance < 4f)
                {
                    existing.spawnForwardOffset = 8f;
                    existing.spawnClearance = 4f;
                    EditorUtility.SetDirty(existing);
                }

                if (existing.villagerTrainTime != 25f)
                {
                    existing.villagerTrainTime = 25f;
                    EditorUtility.SetDirty(existing);
                }

                if (existing.maxHp != 400f)
                {
                    existing.maxHp = 400f;
                    EditorUtility.SetDirty(existing);
                }

                if (existing.villagerFoodCost != 50f)
                {
                    existing.villagerFoodCost = 50f;
                    EditorUtility.SetDirty(existing);
                }

                AssetDatabase.SaveAssets();
                return existing;
            }

            BuildingData data = ScriptableObject.CreateInstance<BuildingData>();
            data.displayName = "Town Center";
            data.villagerTrainTime = 25f;
            data.villagerFoodCost = 50f;
            data.villagerUnitData = villagerData;
            data.spawnForwardOffset = 8f;
            data.spawnClearance = 4f;
            data.maxHp = 400f;
            AssetDatabase.CreateAsset(data, TownCenterDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static PlacedBuildingData EnsureHouseData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/BuildingData"))
                AssetDatabase.CreateFolder("Assets/Data", "BuildingData");

            PlacedBuildingData existing = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(DefaultHouseDataPath);
            if (existing != null)
            {
                bool dirty = false;
                if (existing.woodCost != 30f)
                {
                    existing.woodCost = 30f;
                    dirty = true;
                }

                if (existing.buildTime != 25f)
                {
                    existing.buildTime = 25f;
                    dirty = true;
                }

                if (existing.housingProvided != 5)
                {
                    existing.housingProvided = 5;
                    dirty = true;
                }

                if (existing.maxHp != 150f)
                {
                    existing.maxHp = 150f;
                    dirty = true;
                }

                if (existing.kind != PlacedBuildingKind.House)
                {
                    existing.kind = PlacedBuildingKind.House;
                    dirty = true;
                }

                if (dirty)
                {
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }

                return existing;
            }

            PlacedBuildingData data = ScriptableObject.CreateInstance<PlacedBuildingData>();
            data.kind = PlacedBuildingKind.House;
            data.displayName = "House";
            data.woodCost = 30f;
            data.buildTime = 25f;
            data.housingProvided = 5;
            data.maxHp = 150f;
            AssetDatabase.CreateAsset(data, DefaultHouseDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static PlacedBuildingData EnsureFarmData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/BuildingData"))
                AssetDatabase.CreateFolder("Assets/Data", "BuildingData");

            PlacedBuildingData existing = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(DefaultFarmDataPath);
            if (existing != null)
            {
                bool dirty = false;
                if (existing.kind != PlacedBuildingKind.Farm)
                {
                    existing.kind = PlacedBuildingKind.Farm;
                    dirty = true;
                }

                if (existing.woodCost != 60f)
                {
                    existing.woodCost = 60f;
                    dirty = true;
                }

                if (existing.buildTime != 25f)
                {
                    existing.buildTime = 25f;
                    dirty = true;
                }

                if (existing.foodCapacity != 250f)
                {
                    existing.foodCapacity = 250f;
                    dirty = true;
                }

                if (existing.housingProvided != 0)
                {
                    existing.housingProvided = 0;
                    dirty = true;
                }

                if (existing.maxHp != 100f)
                {
                    existing.maxHp = 100f;
                    dirty = true;
                }

                if (dirty)
                {
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }

                return existing;
            }

            PlacedBuildingData data = ScriptableObject.CreateInstance<PlacedBuildingData>();
            data.kind = PlacedBuildingKind.Farm;
            data.displayName = "Farm";
            data.woodCost = 60f;
            data.buildTime = 25f;
            data.housingProvided = 0;
            data.foodCapacity = 250f;
            data.maxHp = 100f;
            data.defaultColor = new Color(0.35f, 0.7f, 0.25f);
            AssetDatabase.CreateAsset(data, DefaultFarmDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static PlacedBuildingData EnsureLumberCampData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/BuildingData"))
                AssetDatabase.CreateFolder("Assets/Data", "BuildingData");

            PlacedBuildingData existing = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(DefaultLumberCampDataPath);
            if (existing != null)
            {
                bool dirty = false;
                if (existing.kind != PlacedBuildingKind.LumberCamp)
                {
                    existing.kind = PlacedBuildingKind.LumberCamp;
                    dirty = true;
                }

                if (existing.woodCost != 100f)
                {
                    existing.woodCost = 100f;
                    dirty = true;
                }

                if (existing.buildTime != 25f)
                {
                    existing.buildTime = 25f;
                    dirty = true;
                }

                if (existing.housingProvided != 0)
                {
                    existing.housingProvided = 0;
                    dirty = true;
                }

                if (existing.maxHp != 400f)
                {
                    existing.maxHp = 400f;
                    dirty = true;
                }

                if (dirty)
                {
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }

                return existing;
            }

            PlacedBuildingData data = ScriptableObject.CreateInstance<PlacedBuildingData>();
            data.kind = PlacedBuildingKind.LumberCamp;
            data.displayName = "Lumber Camp";
            data.woodCost = 100f;
            data.buildTime = 25f;
            data.housingProvided = 0;
            data.maxHp = 400f;
            data.defaultColor = new Color(0.55f, 0.38f, 0.22f);
            AssetDatabase.CreateAsset(data, DefaultLumberCampDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static PlacedBuildingData EnsureMiningCampData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/BuildingData"))
                AssetDatabase.CreateFolder("Assets/Data", "BuildingData");

            PlacedBuildingData existing = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(DefaultMiningCampDataPath);
            if (existing != null)
            {
                bool dirty = false;
                if (existing.kind != PlacedBuildingKind.MiningCamp)
                {
                    existing.kind = PlacedBuildingKind.MiningCamp;
                    dirty = true;
                }

                if (existing.woodCost != 100f)
                {
                    existing.woodCost = 100f;
                    dirty = true;
                }

                if (existing.buildTime != 25f)
                {
                    existing.buildTime = 25f;
                    dirty = true;
                }

                if (existing.housingProvided != 0)
                {
                    existing.housingProvided = 0;
                    dirty = true;
                }

                if (existing.maxHp != 400f)
                {
                    existing.maxHp = 400f;
                    dirty = true;
                }

                if (dirty)
                {
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }

                return existing;
            }

            PlacedBuildingData data = ScriptableObject.CreateInstance<PlacedBuildingData>();
            data.kind = PlacedBuildingKind.MiningCamp;
            data.displayName = "Mining Camp";
            data.woodCost = 100f;
            data.buildTime = 25f;
            data.housingProvided = 0;
            data.maxHp = 400f;
            data.defaultColor = new Color(0.45f, 0.48f, 0.52f);
            AssetDatabase.CreateAsset(data, DefaultMiningCampDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static PlacedBuildingData EnsureMillData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/BuildingData"))
                AssetDatabase.CreateFolder("Assets/Data", "BuildingData");

            PlacedBuildingData existing = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(DefaultMillDataPath);
            if (existing != null)
            {
                bool dirty = false;
                if (existing.kind != PlacedBuildingKind.Mill)
                {
                    existing.kind = PlacedBuildingKind.Mill;
                    dirty = true;
                }

                if (existing.woodCost != 100f)
                {
                    existing.woodCost = 100f;
                    dirty = true;
                }

                if (existing.buildTime != 25f)
                {
                    existing.buildTime = 25f;
                    dirty = true;
                }

                if (existing.housingProvided != 0)
                {
                    existing.housingProvided = 0;
                    dirty = true;
                }

                if (existing.maxHp != 400f)
                {
                    existing.maxHp = 400f;
                    dirty = true;
                }

                if (dirty)
                {
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }

                return existing;
            }

            PlacedBuildingData data = ScriptableObject.CreateInstance<PlacedBuildingData>();
            data.kind = PlacedBuildingKind.Mill;
            data.displayName = "Mill";
            data.woodCost = 100f;
            data.buildTime = 25f;
            data.housingProvided = 0;
            data.maxHp = 400f;
            data.defaultColor = new Color(0.62f, 0.52f, 0.38f);
            AssetDatabase.CreateAsset(data, DefaultMillDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static UnitData EnsureMilitiaData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/UnitData"))
                AssetDatabase.CreateFolder("Assets/Data", "UnitData");

            UnitData existing = AssetDatabase.LoadAssetAtPath<UnitData>(MilitiaDataPath);
            if (existing != null)
            {
                bool dirty = SyncMilitiaStats(existing);
                if (dirty)
                {
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }

                return existing;
            }

            UnitData data = ScriptableObject.CreateInstance<UnitData>();
            data.displayName = "Militia";
            SyncMilitiaStats(data);
            AssetDatabase.CreateAsset(data, MilitiaDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static UnitData EnsureEnemyDummyData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/UnitData"))
                AssetDatabase.CreateFolder("Assets/Data", "UnitData");

            UnitData existing = AssetDatabase.LoadAssetAtPath<UnitData>(EnemyDummyDataPath);
            if (existing != null)
            {
                bool dirty = SyncEnemyDummyStats(existing);
                if (dirty)
                {
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }

                return existing;
            }

            UnitData data = ScriptableObject.CreateInstance<UnitData>();
            data.displayName = "Enemy Dummy";
            SyncEnemyDummyStats(data);
            AssetDatabase.CreateAsset(data, EnemyDummyDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static PlacedBuildingData EnsureBarracksData(UnitData militiaData, UnitData spearmanData = null)
        {
            if (spearmanData == null)
                spearmanData = EnsureSpearmanData();

            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/BuildingData"))
                AssetDatabase.CreateFolder("Assets/Data", "BuildingData");

            PlacedBuildingData existing = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(DefaultBarracksDataPath);
            if (existing != null)
            {
                bool dirty = false;
                if (existing.kind != PlacedBuildingKind.Barracks)
                {
                    existing.kind = PlacedBuildingKind.Barracks;
                    dirty = true;
                }

                if (existing.trainUnitData != militiaData && militiaData != null)
                {
                    existing.trainUnitData = militiaData;
                    dirty = true;
                }

                if (existing.secondaryTrainUnitData != spearmanData && spearmanData != null)
                {
                    existing.secondaryTrainUnitData = spearmanData;
                    dirty = true;
                }

                if (existing.secondaryTrainTime != 22f) { existing.secondaryTrainTime = 22f; dirty = true; }
                if (existing.secondaryTrainWoodCost != 22f) { existing.secondaryTrainWoodCost = 22f; dirty = true; }
                if (existing.secondaryTrainFoodCost != 35f) { existing.secondaryTrainFoodCost = 35f; dirty = true; }
                if (existing.woodCost != 175f) { existing.woodCost = 175f; dirty = true; }
                if (existing.buildTime != 50f) { existing.buildTime = 50f; dirty = true; }
                if (existing.trainTime != 21f) { existing.trainTime = 21f; dirty = true; }
                if (existing.trainWoodCost != 0f) { existing.trainWoodCost = 0f; dirty = true; }
                if (existing.trainFoodCost != 60f) { existing.trainFoodCost = 60f; dirty = true; }
                if (existing.requiredAge != GameAge.Dark) { existing.requiredAge = GameAge.Dark; dirty = true; }

                if (existing.maxHp != 200f)
                {
                    existing.maxHp = 200f;
                    dirty = true;
                }

                if (dirty)
                {
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }

                return existing;
            }

            PlacedBuildingData data = ScriptableObject.CreateInstance<PlacedBuildingData>();
            data.kind = PlacedBuildingKind.Barracks;
            data.displayName = "Barracks";
            data.woodCost = 175f;
            data.buildTime = 50f;
            data.footprintWidth = 6f;
            data.footprintDepth = 6f;
            data.buildingHeight = 3.5f;
            data.housingProvided = 0;
            data.trainUnitData = militiaData;
            data.trainTime = 21f;
            data.trainWoodCost = 0f;
            data.trainFoodCost = 60f;
            data.secondaryTrainUnitData = spearmanData;
            data.secondaryTrainTime = 22f;
            data.secondaryTrainWoodCost = 22f;
            data.secondaryTrainFoodCost = 35f;
            data.spawnClearance = 4f;
            data.defaultColor = new Color(0.55f, 0.35f, 0.32f);
            data.selectedColor = new Color(0.95f, 0.55f, 0.35f);
            data.maxHp = 200f;
            AssetDatabase.CreateAsset(data, DefaultBarracksDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static UnitData EnsureSpearmanData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/UnitData"))
                AssetDatabase.CreateFolder("Assets/Data", "UnitData");

            UnitData existing = AssetDatabase.LoadAssetAtPath<UnitData>(SpearmanDataPath);
            if (existing != null)
            {
                bool dirty = SyncSpearmanStats(existing);
                if (dirty)
                {
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }

                return existing;
            }

            UnitData data = ScriptableObject.CreateInstance<UnitData>();
            data.displayName = "Spearman";
            SyncSpearmanStats(data);
            AssetDatabase.CreateAsset(data, SpearmanDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static UnitData EnsureArcherData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/UnitData"))
                AssetDatabase.CreateFolder("Assets/Data", "UnitData");

            UnitData existing = AssetDatabase.LoadAssetAtPath<UnitData>(ArcherDataPath);
            if (existing != null)
            {
                bool dirty = SyncArcherStats(existing);
                if (dirty)
                {
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }

                return existing;
            }

            UnitData data = ScriptableObject.CreateInstance<UnitData>();
            data.displayName = "Archer";
            SyncArcherStats(data);
            AssetDatabase.CreateAsset(data, ArcherDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static PlacedBuildingData EnsureArcheryRangeData(UnitData archerData)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/BuildingData"))
                AssetDatabase.CreateFolder("Assets/Data", "BuildingData");

            PlacedBuildingData existing = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(DefaultArcheryRangeDataPath);
            if (existing != null)
            {
                bool dirty = false;
                if (existing.kind != PlacedBuildingKind.ArcheryRange)
                {
                    existing.kind = PlacedBuildingKind.ArcheryRange;
                    dirty = true;
                }

                if (existing.trainUnitData != archerData && archerData != null)
                {
                    existing.trainUnitData = archerData;
                    dirty = true;
                }

                if (existing.woodCost != 175f) { existing.woodCost = 175f; dirty = true; }
                if (existing.buildTime != 50f) { existing.buildTime = 50f; dirty = true; }
                if (existing.maxHp != 300f) { existing.maxHp = 300f; dirty = true; }
                if (existing.trainTime != 35f) { existing.trainTime = 35f; dirty = true; }
                if (existing.trainWoodCost != 35f) { existing.trainWoodCost = 35f; dirty = true; }
                if (existing.trainFoodCost != 35f) { existing.trainFoodCost = 35f; dirty = true; }
                if (existing.requiredAge != GameAge.Feudal) { existing.requiredAge = GameAge.Feudal; dirty = true; }

                if (dirty)
                {
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }

                return existing;
            }

            PlacedBuildingData data = ScriptableObject.CreateInstance<PlacedBuildingData>();
            data.kind = PlacedBuildingKind.ArcheryRange;
            data.displayName = "Archery Range";
            data.woodCost = 175f;
            data.buildTime = 50f;
            data.footprintWidth = 6f;
            data.footprintDepth = 6f;
            data.buildingHeight = 3.5f;
            data.housingProvided = 0;
            data.requiredAge = GameAge.Feudal;
            data.trainUnitData = archerData;
            data.trainTime = 35f;
            data.trainWoodCost = 35f;
            data.trainFoodCost = 35f;
            data.spawnClearance = 4f;
            data.defaultColor = new Color(0.45f, 0.55f, 0.38f);
            data.selectedColor = new Color(0.75f, 0.9f, 0.45f);
            data.maxHp = 300f;
            AssetDatabase.CreateAsset(data, DefaultArcheryRangeDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static UnitData EnsureCavalryData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/UnitData"))
                AssetDatabase.CreateFolder("Assets/Data", "UnitData");

            UnitData existing = AssetDatabase.LoadAssetAtPath<UnitData>(CavalryDataPath);
            if (existing != null)
            {
                bool dirty = SyncCavalryStats(existing);
                if (dirty)
                {
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }

                return existing;
            }

            UnitData data = ScriptableObject.CreateInstance<UnitData>();
            data.displayName = "Cavalry";
            SyncCavalryStats(data);
            AssetDatabase.CreateAsset(data, CavalryDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static UnitData EnsureScoutData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/UnitData"))
                AssetDatabase.CreateFolder("Assets/Data", "UnitData");

            UnitData existing = AssetDatabase.LoadAssetAtPath<UnitData>(ScoutDataPath);
            if (existing != null)
            {
                bool dirty = SyncScoutStats(existing);
                if (dirty)
                {
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }

                return existing;
            }

            UnitData data = ScriptableObject.CreateInstance<UnitData>();
            data.displayName = "Scout";
            SyncScoutStats(data);
            AssetDatabase.CreateAsset(data, ScoutDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static PlacedBuildingData EnsureStableData(UnitData cavalryData, UnitData scoutData)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/BuildingData"))
                AssetDatabase.CreateFolder("Assets/Data", "BuildingData");

            PlacedBuildingData existing = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(DefaultStableDataPath);
            if (existing != null)
            {
                bool dirty = false;
                if (existing.kind != PlacedBuildingKind.Stable)
                {
                    existing.kind = PlacedBuildingKind.Stable;
                    dirty = true;
                }

                if (existing.trainUnitData != cavalryData && cavalryData != null)
                {
                    existing.trainUnitData = cavalryData;
                    dirty = true;
                }

                if (existing.secondaryTrainUnitData != scoutData && scoutData != null)
                {
                    existing.secondaryTrainUnitData = scoutData;
                    dirty = true;
                }

                if (existing.woodCost != 175f) { existing.woodCost = 175f; dirty = true; }
                if (existing.buildTime != 50f) { existing.buildTime = 50f; dirty = true; }
                if (existing.maxHp != 300f) { existing.maxHp = 300f; dirty = true; }
                if (existing.trainTime != 30f) { existing.trainTime = 30f; dirty = true; }
                if (existing.trainWoodCost != 75f) { existing.trainWoodCost = 75f; dirty = true; }
                if (existing.trainFoodCost != 60f) { existing.trainFoodCost = 60f; dirty = true; }
                if (existing.secondaryTrainTime != 30f) { existing.secondaryTrainTime = 30f; dirty = true; }
                if (existing.secondaryTrainWoodCost != 0f) { existing.secondaryTrainWoodCost = 0f; dirty = true; }
                if (existing.secondaryTrainFoodCost != 80f) { existing.secondaryTrainFoodCost = 80f; dirty = true; }
                if (existing.requiredAge != GameAge.Feudal) { existing.requiredAge = GameAge.Feudal; dirty = true; }

                if (dirty)
                {
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }

                return existing;
            }

            PlacedBuildingData data = ScriptableObject.CreateInstance<PlacedBuildingData>();
            data.kind = PlacedBuildingKind.Stable;
            data.displayName = "Stable";
            data.woodCost = 175f;
            data.buildTime = 50f;
            data.footprintWidth = 6f;
            data.footprintDepth = 6f;
            data.buildingHeight = 3.5f;
            data.housingProvided = 0;
            data.requiredAge = GameAge.Feudal;
            data.trainUnitData = cavalryData;
            data.trainTime = 30f;
            data.trainWoodCost = 75f;
            data.trainFoodCost = 60f;
            data.secondaryTrainUnitData = scoutData;
            data.secondaryTrainTime = 30f;
            data.secondaryTrainWoodCost = 0f;
            data.secondaryTrainFoodCost = 80f;
            data.spawnClearance = 4f;
            data.defaultColor = new Color(0.55f, 0.45f, 0.28f);
            data.selectedColor = new Color(0.9f, 0.75f, 0.35f);
            data.maxHp = 300f;
            AssetDatabase.CreateAsset(data, DefaultStableDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static AgeData EnsureFeudalAgeData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/AgeData"))
                AssetDatabase.CreateFolder("Assets/Data", "AgeData");

            AgeData existing = AssetDatabase.LoadAssetAtPath<AgeData>(FeudalAgeDataPath);
            if (existing != null)
            {
                bool dirty = false;
                if (existing.targetAge != GameAge.Feudal) { existing.targetAge = GameAge.Feudal; dirty = true; }
                if (existing.displayName != "Feudal Age") { existing.displayName = "Feudal Age"; dirty = true; }
                if (existing.upgradeFoodCost != 500f) { existing.upgradeFoodCost = 500f; dirty = true; }
                if (existing.upgradeGoldCost != 300f) { existing.upgradeGoldCost = 300f; dirty = true; }
                if (dirty)
                {
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }

                return existing;
            }

            AgeData data = ScriptableObject.CreateInstance<AgeData>();
            data.targetAge = GameAge.Feudal;
            data.displayName = "Feudal Age";
            data.upgradeFoodCost = 500f;
            data.upgradeGoldCost = 300f;
            AssetDatabase.CreateAsset(data, FeudalAgeDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static UnitData EnsureManAtArmsData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/UnitData"))
                AssetDatabase.CreateFolder("Assets/Data", "UnitData");

            UnitData existing = AssetDatabase.LoadAssetAtPath<UnitData>(ManAtArmsDataPath);
            if (existing != null)
            {
                bool dirty = SyncManAtArmsStats(existing);
                if (dirty)
                {
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }

                return existing;
            }

            UnitData data = ScriptableObject.CreateInstance<UnitData>();
            data.displayName = "Man-at-Arms";
            SyncManAtArmsStats(data);
            AssetDatabase.CreateAsset(data, ManAtArmsDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static TechnologyData EnsureInfantryUpgradeTech(UnitData militiaData = null, UnitData manAtArmsData = null)
        {
            if (militiaData == null)
                militiaData = EnsureMilitiaData();
            if (manAtArmsData == null)
                manAtArmsData = EnsureManAtArmsData();

            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/TechnologyData"))
                AssetDatabase.CreateFolder("Assets/Data", "TechnologyData");

            TechnologyData existing = AssetDatabase.LoadAssetAtPath<TechnologyData>(InfantryUpgradeTechPath);
            if (existing != null)
            {
                bool dirty = SyncInfantryUpgradeTech(existing, militiaData, manAtArmsData);
                if (dirty)
                {
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }

                return existing;
            }

            TechnologyData data = ScriptableObject.CreateInstance<TechnologyData>();
            data.displayName = "Infantry Upgrade";
            SyncInfantryUpgradeTech(data, militiaData, manAtArmsData);
            AssetDatabase.CreateAsset(data, InfantryUpgradeTechPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static PlacedBuildingData EnsureBlacksmithData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/BuildingData"))
                AssetDatabase.CreateFolder("Assets/Data", "BuildingData");

            PlacedBuildingData existing = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(DefaultBlacksmithDataPath);
            if (existing != null)
            {
                bool dirty = false;
                if (existing.kind != PlacedBuildingKind.Blacksmith) { existing.kind = PlacedBuildingKind.Blacksmith; dirty = true; }
                if (existing.displayName != "Blacksmith") { existing.displayName = "Blacksmith"; dirty = true; }
                if (existing.woodCost != 150f) { existing.woodCost = 150f; dirty = true; }
                if (existing.buildTime != 40f) { existing.buildTime = 40f; dirty = true; }
                if (existing.requiredAge != GameAge.Feudal) { existing.requiredAge = GameAge.Feudal; dirty = true; }
                if (existing.maxHp != 300f) { existing.maxHp = 300f; dirty = true; }
                if (dirty)
                {
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }

                return existing;
            }

            PlacedBuildingData data = ScriptableObject.CreateInstance<PlacedBuildingData>();
            data.kind = PlacedBuildingKind.Blacksmith;
            data.displayName = "Blacksmith";
            data.woodCost = 150f;
            data.buildTime = 40f;
            data.requiredAge = GameAge.Feudal;
            data.footprintWidth = 6f;
            data.footprintDepth = 6f;
            data.buildingHeight = 3.5f;
            data.housingProvided = 0;
            data.maxHp = 300f;
            data.defaultColor = new Color(0.5f, 0.5f, 0.55f);
            data.selectedColor = new Color(0.85f, 0.85f, 0.95f);
            AssetDatabase.CreateAsset(data, DefaultBlacksmithDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static PlacedBuildingData EnsureMarketData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/BuildingData"))
                AssetDatabase.CreateFolder("Assets/Data", "BuildingData");

            PlacedBuildingData existing = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(DefaultMarketDataPath);
            if (existing != null)
            {
                bool dirty = false;
                if (existing.kind != PlacedBuildingKind.Market) { existing.kind = PlacedBuildingKind.Market; dirty = true; }
                if (existing.displayName != "Market") { existing.displayName = "Market"; dirty = true; }
                if (existing.woodCost != 175f) { existing.woodCost = 175f; dirty = true; }
                if (existing.buildTime != 60f) { existing.buildTime = 60f; dirty = true; }
                if (existing.requiredAge != GameAge.Feudal) { existing.requiredAge = GameAge.Feudal; dirty = true; }
                if (existing.maxHp != 400f) { existing.maxHp = 400f; dirty = true; }
                if (dirty)
                {
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }

                return existing;
            }

            PlacedBuildingData data = ScriptableObject.CreateInstance<PlacedBuildingData>();
            data.kind = PlacedBuildingKind.Market;
            data.displayName = "Market";
            data.woodCost = 175f;
            data.buildTime = 60f;
            data.requiredAge = GameAge.Feudal;
            data.footprintWidth = 6f;
            data.footprintDepth = 6f;
            data.buildingHeight = 3.5f;
            data.housingProvided = 0;
            data.maxHp = 400f;
            data.defaultColor = new Color(0.58f, 0.48f, 0.32f);
            data.selectedColor = new Color(0.85f, 0.8f, 0.55f);
            AssetDatabase.CreateAsset(data, DefaultMarketDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static MarketTradeData EnsureMarketTradeData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/MarketData"))
                AssetDatabase.CreateFolder("Assets/Data", "MarketData");

            MarketTradeData existing = AssetDatabase.LoadAssetAtPath<MarketTradeData>(DefaultMarketTradeDataPath);
            if (existing != null)
                return existing;

            MarketTradeData data = ScriptableObject.CreateInstance<MarketTradeData>();
            data.tradeUnitAmount = 100f;
            data.sellFoodGoldReceived = 50f;
            data.buyFoodGoldCost = 50f;
            data.sellWoodGoldReceived = 50f;
            data.buyWoodGoldCost = 50f;
            data.sellStoneGoldReceived = 50f;
            data.buyStoneGoldCost = 50f;
            AssetDatabase.CreateAsset(data, DefaultMarketTradeDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static CivilizationData EnsureDefaultPlayerCivilizationData()
        {
            return EnsureCivilizationData(
                DefaultPlayerCivilizationDataPath,
                "Franks (Demo)",
                CivilizationBonusKind.GatherRate,
                1.1f,
                1f);
        }

        public static CivilizationData EnsureDefaultCpuCivilizationData()
        {
            return EnsureCivilizationData(
                DefaultCpuCivilizationDataPath,
                "Standard",
                CivilizationBonusKind.GatherRate,
                1f,
                1f);
        }

        static CivilizationData EnsureCivilizationData(
            string assetPath,
            string displayName,
            CivilizationBonusKind bonusKind,
            float gatherRateMultiplier,
            float infantryHpMultiplier)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/CivilizationData"))
                AssetDatabase.CreateFolder("Assets/Data", "CivilizationData");

            CivilizationData existing = AssetDatabase.LoadAssetAtPath<CivilizationData>(assetPath);
            if (existing != null)
            {
                bool dirty = false;
                if (existing.displayName != displayName) { existing.displayName = displayName; dirty = true; }
                if (existing.bonusKind != bonusKind) { existing.bonusKind = bonusKind; dirty = true; }
                if (existing.gatherRateMultiplier != gatherRateMultiplier)
                {
                    existing.gatherRateMultiplier = gatherRateMultiplier;
                    dirty = true;
                }

                if (existing.infantryHpMultiplier != infantryHpMultiplier)
                {
                    existing.infantryHpMultiplier = infantryHpMultiplier;
                    dirty = true;
                }

                if (dirty)
                {
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }

                return existing;
            }

            CivilizationData data = ScriptableObject.CreateInstance<CivilizationData>();
            data.displayName = displayName;
            data.bonusKind = bonusKind;
            data.gatherRateMultiplier = gatherRateMultiplier;
            data.infantryHpMultiplier = infantryHpMultiplier;
            AssetDatabase.CreateAsset(data, assetPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static PlacedBuildingData EnsureTownCenterPlacementData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/BuildingData"))
                AssetDatabase.CreateFolder("Assets/Data", "BuildingData");

            PlacedBuildingData existing = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(
                DefaultTownCenterPlacementDataPath);
            if (existing != null)
            {
                bool dirty = false;
                if (existing.kind != PlacedBuildingKind.TownCenter) { existing.kind = PlacedBuildingKind.TownCenter; dirty = true; }
                if (existing.displayName != "Town Center") { existing.displayName = "Town Center"; dirty = true; }
                if (existing.requiredAge != GameAge.Feudal) { existing.requiredAge = GameAge.Feudal; dirty = true; }
                if (existing.woodCost != 275f) { existing.woodCost = 275f; dirty = true; }
                if (existing.stoneCost != 100f) { existing.stoneCost = 100f; dirty = true; }
                if (existing.buildTime != 150f) { existing.buildTime = 150f; dirty = true; }
                if (existing.footprintWidth != 8f) { existing.footprintWidth = 8f; dirty = true; }
                if (existing.footprintDepth != 8f) { existing.footprintDepth = 8f; dirty = true; }
                if (existing.buildingHeight != 4f) { existing.buildingHeight = 4f; dirty = true; }
                if (existing.maxHp != 400f) { existing.maxHp = 400f; dirty = true; }
                if (dirty)
                {
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }

                return existing;
            }

            PlacedBuildingData data = ScriptableObject.CreateInstance<PlacedBuildingData>();
            data.kind = PlacedBuildingKind.TownCenter;
            data.displayName = "Town Center";
            data.requiredAge = GameAge.Feudal;
            data.woodCost = 275f;
            data.stoneCost = 100f;
            data.buildTime = 150f;
            data.footprintWidth = 8f;
            data.footprintDepth = 8f;
            data.buildingHeight = 4f;
            data.housingProvided = 0;
            data.maxHp = 400f;
            data.defaultColor = new Color(0.75f, 0.65f, 0.45f);
            data.selectedColor = new Color(0.95f, 0.85f, 0.35f);
            AssetDatabase.CreateAsset(data, DefaultTownCenterPlacementDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static PlacedBuildingData EnsurePalisadeWallData()
        {
            return EnsureDefenseBuildingData(
                DefaultPalisadeWallDataPath,
                PlacedBuildingKind.PalisadeWall,
                "Palisade Wall",
                GameAge.Dark,
                woodCost: 2f,
                stoneCost: 0f,
                buildTime: 7f,
                footprintWidth: 1f,
                footprintDepth: 4f,
                buildingHeight: 2f,
                maxHp: 250f,
                meleeArmor: 0f,
                pierceArmor: 0f,
                towerAttack: 0f,
                defaultColor: new Color(0.6f, 0.45f, 0.25f));
        }

        public static PlacedBuildingData EnsureStoneWallData()
        {
            return EnsureDefenseBuildingData(
                DefaultStoneWallDataPath,
                PlacedBuildingKind.StoneWall,
                "Stone Wall",
                GameAge.Feudal,
                woodCost: 0f,
                stoneCost: 5f,
                buildTime: 8f,
                footprintWidth: 1f,
                footprintDepth: 4f,
                buildingHeight: 2.5f,
                maxHp: 900f,
                meleeArmor: 0f,
                pierceArmor: 8f,
                towerAttack: 0f,
                defaultColor: new Color(0.55f, 0.55f, 0.58f));
        }

        public static PlacedBuildingData EnsureWatchTowerData()
        {
            return EnsureDefenseBuildingData(
                DefaultWatchTowerDataPath,
                PlacedBuildingKind.WatchTower,
                "Watch Tower",
                GameAge.Feudal,
                woodCost: 0f,
                stoneCost: 125f,
                buildTime: 80f,
                footprintWidth: 4f,
                footprintDepth: 4f,
                buildingHeight: 5f,
                maxHp: 1020f,
                meleeArmor: 0f,
                pierceArmor: 8f,
                towerAttack: 5f,
                defaultColor: new Color(0.5f, 0.52f, 0.55f),
                selectedColor: new Color(0.85f, 0.8f, 0.55f),
                towerAttackRange: 7f,
                towerAttackCooldown: 2f);
        }

        static PlacedBuildingData EnsureDefenseBuildingData(
            string assetPath,
            PlacedBuildingKind kind,
            string displayName,
            GameAge requiredAge,
            float woodCost,
            float stoneCost,
            float buildTime,
            float footprintWidth,
            float footprintDepth,
            float buildingHeight,
            float maxHp,
            float meleeArmor,
            float pierceArmor,
            float towerAttack,
            Color defaultColor,
            Color? selectedColor = null,
            float towerAttackRange = 7f,
            float towerAttackCooldown = 2f)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/BuildingData"))
                AssetDatabase.CreateFolder("Assets/Data", "BuildingData");

            PlacedBuildingData existing = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(assetPath);
            if (existing != null)
            {
                bool dirty = SyncDefenseBuildingData(
                    existing,
                    kind,
                    displayName,
                    requiredAge,
                    woodCost,
                    stoneCost,
                    buildTime,
                    footprintWidth,
                    footprintDepth,
                    buildingHeight,
                    maxHp,
                    meleeArmor,
                    pierceArmor,
                    towerAttack,
                    defaultColor,
                    selectedColor,
                    towerAttackRange,
                    towerAttackCooldown);
                if (dirty)
                {
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }

                return existing;
            }

            PlacedBuildingData data = ScriptableObject.CreateInstance<PlacedBuildingData>();
            SyncDefenseBuildingData(
                data,
                kind,
                displayName,
                requiredAge,
                woodCost,
                stoneCost,
                buildTime,
                footprintWidth,
                footprintDepth,
                buildingHeight,
                maxHp,
                meleeArmor,
                pierceArmor,
                towerAttack,
                defaultColor,
                selectedColor,
                towerAttackRange,
                towerAttackCooldown);
            AssetDatabase.CreateAsset(data, assetPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        static bool SyncDefenseBuildingData(
            PlacedBuildingData data,
            PlacedBuildingKind kind,
            string displayName,
            GameAge requiredAge,
            float woodCost,
            float stoneCost,
            float buildTime,
            float footprintWidth,
            float footprintDepth,
            float buildingHeight,
            float maxHp,
            float meleeArmor,
            float pierceArmor,
            float towerAttack,
            Color defaultColor,
            Color? selectedColor,
            float towerAttackRange,
            float towerAttackCooldown)
        {
            bool dirty = false;
            if (data.kind != kind) { data.kind = kind; dirty = true; }
            if (data.displayName != displayName) { data.displayName = displayName; dirty = true; }
            if (data.requiredAge != requiredAge) { data.requiredAge = requiredAge; dirty = true; }
            if (data.woodCost != woodCost) { data.woodCost = woodCost; dirty = true; }
            if (data.stoneCost != stoneCost) { data.stoneCost = stoneCost; dirty = true; }
            if (data.buildTime != buildTime) { data.buildTime = buildTime; dirty = true; }
            if (data.footprintWidth != footprintWidth) { data.footprintWidth = footprintWidth; dirty = true; }
            if (data.footprintDepth != footprintDepth) { data.footprintDepth = footprintDepth; dirty = true; }
            if (data.buildingHeight != buildingHeight) { data.buildingHeight = buildingHeight; dirty = true; }
            if (data.housingProvided != 0) { data.housingProvided = 0; dirty = true; }
            if (data.maxHp != maxHp) { data.maxHp = maxHp; dirty = true; }
            if (data.meleeArmor != meleeArmor) { data.meleeArmor = meleeArmor; dirty = true; }
            if (data.pierceArmor != pierceArmor) { data.pierceArmor = pierceArmor; dirty = true; }
            if (data.towerAttack != towerAttack) { data.towerAttack = towerAttack; dirty = true; }
            if (data.towerAttackRange != towerAttackRange) { data.towerAttackRange = towerAttackRange; dirty = true; }
            if (data.towerAttackCooldown != towerAttackCooldown) { data.towerAttackCooldown = towerAttackCooldown; dirty = true; }
            if (data.defaultColor != defaultColor) { data.defaultColor = defaultColor; dirty = true; }
            if (selectedColor.HasValue && data.selectedColor != selectedColor.Value)
            {
                data.selectedColor = selectedColor.Value;
                dirty = true;
            }

            return dirty;
        }

        public static GameObject CreateTownCenter(BuildingData buildingData, Vector3 position)
        {
            const float buildingHeight = 4f;
            const float buildingWidth = 8f;
            const float groundClearance = 0.05f;

            Vector3 worldPosition = new Vector3(
                position.x,
                buildingHeight * 0.5f + groundClearance,
                position.z);

            GameObject townCenterObject = EntityVisualBuilder.CreateBuildingShell(
                "TownCenter",
                LayerMask.NameToLayer("Building"),
                worldPosition,
                new Vector3(buildingWidth, buildingHeight, buildingWidth),
                Vector3.zero,
                PlaceholderVisualKind.TownCenter);

            TownCenter townCenter = townCenterObject.AddComponent<TownCenter>();
            SerializedObject serializedTownCenter = new SerializedObject(townCenter);
            serializedTownCenter.Update();
            serializedTownCenter.FindProperty("data").objectReferenceValue = buildingData;
            serializedTownCenter.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(townCenter);
            EditorUtility.SetDirty(townCenterObject);

            return townCenterObject;
        }

        public static ResourceNodeData EnsureDefaultTreeData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/ResourceData"))
                AssetDatabase.CreateFolder("Assets/Data", "ResourceData");

            ResourceNodeData existing = AssetDatabase.LoadAssetAtPath<ResourceNodeData>(DefaultTreeDataPath);
            if (existing != null)
                return existing;

            ResourceNodeData data = ScriptableObject.CreateInstance<ResourceNodeData>();
            data.displayName = "Tree";
            data.initialWood = 100f;
            AssetDatabase.CreateAsset(data, DefaultTreeDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static FoodNodeData EnsureDefaultBerryBushData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/ResourceData"))
                AssetDatabase.CreateFolder("Assets/Data", "ResourceData");

            FoodNodeData existing = AssetDatabase.LoadAssetAtPath<FoodNodeData>(DefaultBerryBushDataPath);
            if (existing != null)
                return existing;

            FoodNodeData data = ScriptableObject.CreateInstance<FoodNodeData>();
            data.displayName = "Berry Bush";
            data.initialFood = 250f;
            AssetDatabase.CreateAsset(data, DefaultBerryBushDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static GameObject CreateBerryBush(FoodNodeData bushData, Vector3 position)
        {
            const float radius = 1.2f;
            const float groundClearance = 0.05f;

            Vector3 worldPosition = new Vector3(
                position.x,
                radius + groundClearance,
                position.z);

            GameObject bushObject = new GameObject("BerryBush");
            bushObject.layer = LayerMask.NameToLayer(GameLayers.ResourceLayerName);
            bushObject.transform.position = worldPosition;

            SphereCollider collider = bushObject.AddComponent<SphereCollider>();
            collider.radius = radius;

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "Visual";
            visual.transform.SetParent(bushObject.transform, false);
            visual.transform.localScale = Vector3.one * (radius * 2f);
            Object.DestroyImmediate(visual.GetComponent<Collider>());

            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null && bushData != null)
            {
                Material material = SceneMaterialFactory.CreateLitMaterial(bushData.defaultColor);
                renderer.sharedMaterial = material;
            }

            BerryBushResource berryBush = bushObject.AddComponent<BerryBushResource>();
            SerializedObject serializedBush = new SerializedObject(berryBush);
            serializedBush.FindProperty("data").objectReferenceValue = bushData;
            serializedBush.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(berryBush);
            EditorUtility.SetDirty(bushObject);

            return bushObject;
        }

        public static FoodNodeData EnsureDefaultDeerData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/ResourceData"))
                AssetDatabase.CreateFolder("Assets/Data", "ResourceData");

            FoodNodeData existing = AssetDatabase.LoadAssetAtPath<FoodNodeData>(DefaultDeerDataPath);
            if (existing != null)
                return existing;

            FoodNodeData data = ScriptableObject.CreateInstance<FoodNodeData>();
            data.displayName = "Deer";
            data.initialFood = 140f;
            data.defaultColor = new Color(0.55f, 0.38f, 0.22f);
            AssetDatabase.CreateAsset(data, DefaultDeerDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static FoodNodeData EnsureDefaultSheepData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/ResourceData"))
                AssetDatabase.CreateFolder("Assets/Data", "ResourceData");

            FoodNodeData existing = AssetDatabase.LoadAssetAtPath<FoodNodeData>(DefaultSheepDataPath);
            if (existing != null)
                return existing;

            FoodNodeData data = ScriptableObject.CreateInstance<FoodNodeData>();
            data.displayName = "Sheep";
            data.initialFood = 100f;
            data.defaultColor = new Color(0.88f, 0.88f, 0.85f);
            AssetDatabase.CreateAsset(data, DefaultSheepDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static FoodNodeData EnsureDefaultBoarData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/ResourceData"))
                AssetDatabase.CreateFolder("Assets/Data", "ResourceData");

            FoodNodeData existing = AssetDatabase.LoadAssetAtPath<FoodNodeData>(DefaultBoarDataPath);
            if (existing != null)
                return existing;

            FoodNodeData data = ScriptableObject.CreateInstance<FoodNodeData>();
            data.displayName = "Boar";
            data.initialFood = 340f;
            data.defaultColor = new Color(0.35f, 0.35f, 0.38f);
            AssetDatabase.CreateAsset(data, DefaultBoarDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static GameObject CreateDeer(FoodNodeData deerData, Vector3 position)
        {
            const float height = 2f;
            const float radius = 0.6f;
            const float groundClearance = 0.05f;

            Vector3 worldPosition = new Vector3(
                position.x,
                height * 0.5f + groundClearance,
                position.z);

            GameObject deerObject = new GameObject("Deer");
            deerObject.layer = LayerMask.NameToLayer(GameLayers.ResourceLayerName);
            deerObject.transform.position = worldPosition;

            CapsuleCollider collider = deerObject.AddComponent<CapsuleCollider>();
            collider.height = height;
            collider.radius = radius;

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(deerObject.transform, false);
            visual.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
            Object.DestroyImmediate(visual.GetComponent<Collider>());

            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null && deerData != null)
            {
                Material material = SceneMaterialFactory.CreateLitMaterial(deerData.defaultColor);
                renderer.sharedMaterial = material;
            }

            DeerResource deer = deerObject.AddComponent<DeerResource>();
            SerializedObject serializedDeer = new SerializedObject(deer);
            serializedDeer.FindProperty("data").objectReferenceValue = deerData;
            serializedDeer.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(deer);
            EditorUtility.SetDirty(deerObject);

            return deerObject;
        }

        public static GameObject CreateSheep(FoodNodeData sheepData, Vector3 position)
        {
            const float height = 1.4f;
            const float radius = 0.45f;
            const float groundClearance = 0.05f;

            Vector3 worldPosition = new Vector3(
                position.x,
                height * 0.5f + groundClearance,
                position.z);

            GameObject sheepObject = new GameObject("Sheep");
            sheepObject.layer = LayerMask.NameToLayer(GameLayers.ResourceLayerName);
            sheepObject.transform.position = worldPosition;

            CapsuleCollider collider = sheepObject.AddComponent<CapsuleCollider>();
            collider.height = height;
            collider.radius = radius;

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(sheepObject.transform, false);
            visual.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
            Object.DestroyImmediate(visual.GetComponent<Collider>());

            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null && sheepData != null)
            {
                Material material = SceneMaterialFactory.CreateLitMaterial(sheepData.defaultColor);
                renderer.sharedMaterial = material;
            }

            SheepResource sheep = sheepObject.AddComponent<SheepResource>();
            SerializedObject serializedSheep = new SerializedObject(sheep);
            serializedSheep.FindProperty("data").objectReferenceValue = sheepData;
            serializedSheep.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(sheep);
            EditorUtility.SetDirty(sheepObject);

            return sheepObject;
        }

        public static GameObject CreateBoar(FoodNodeData boarData, Vector3 position)
        {
            const float height = 1.8f;
            const float radius = 0.75f;
            const float groundClearance = 0.05f;

            Vector3 worldPosition = new Vector3(
                position.x,
                height * 0.5f + groundClearance,
                position.z);

            GameObject boarObject = new GameObject("Boar");
            boarObject.layer = LayerMask.NameToLayer(GameLayers.ResourceLayerName);
            boarObject.transform.position = worldPosition;

            CapsuleCollider collider = boarObject.AddComponent<CapsuleCollider>();
            collider.height = height;
            collider.radius = radius;

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(boarObject.transform, false);
            visual.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
            Object.DestroyImmediate(visual.GetComponent<Collider>());

            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null && boarData != null)
            {
                Material material = SceneMaterialFactory.CreateLitMaterial(boarData.defaultColor);
                renderer.sharedMaterial = material;
            }

            BoarResource boar = boarObject.AddComponent<BoarResource>();
            SerializedObject serializedBoar = new SerializedObject(boar);
            serializedBoar.FindProperty("data").objectReferenceValue = boarData;
            serializedBoar.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(boar);
            EditorUtility.SetDirty(boarObject);

            return boarObject;
        }

        public static MineralNodeData EnsureDefaultGoldMineData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/ResourceData"))
                AssetDatabase.CreateFolder("Assets/Data", "ResourceData");

            MineralNodeData existing = AssetDatabase.LoadAssetAtPath<MineralNodeData>(DefaultGoldMineDataPath);
            if (existing != null)
                return existing;

            MineralNodeData data = ScriptableObject.CreateInstance<MineralNodeData>();
            data.displayName = "Gold Mine";
            data.initialAmount = 800f;
            data.defaultColor = new Color(0.85f, 0.72f, 0.2f);
            AssetDatabase.CreateAsset(data, DefaultGoldMineDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static MineralNodeData EnsureDefaultStoneMineData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/ResourceData"))
                AssetDatabase.CreateFolder("Assets/Data", "ResourceData");

            MineralNodeData existing = AssetDatabase.LoadAssetAtPath<MineralNodeData>(DefaultStoneMineDataPath);
            if (existing != null)
                return existing;

            MineralNodeData data = ScriptableObject.CreateInstance<MineralNodeData>();
            data.displayName = "Stone Mine";
            data.initialAmount = 350f;
            data.defaultColor = new Color(0.55f, 0.55f, 0.58f);
            AssetDatabase.CreateAsset(data, DefaultStoneMineDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        public static GameObject CreateGoldMine(MineralNodeData mineData, Vector3 position)
        {
            return CreateMineralMine("GoldMine", mineData, position, typeof(GoldMineResource));
        }

        public static GameObject CreateStoneMine(MineralNodeData mineData, Vector3 position)
        {
            return CreateMineralMine("StoneMine", mineData, position, typeof(StoneMineResource));
        }

        static GameObject CreateMineralMine(string objectName, MineralNodeData mineData, Vector3 position, System.Type resourceType)
        {
            const float size = 3f;
            const float groundClearance = 0.05f;

            Vector3 worldPosition = new Vector3(
                position.x,
                size * 0.5f + groundClearance,
                position.z);

            GameObject mineObject = new GameObject(objectName);
            mineObject.layer = LayerMask.NameToLayer(GameLayers.ResourceLayerName);
            mineObject.transform.position = worldPosition;

            BoxCollider collider = mineObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(size, size, size);

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            visual.transform.SetParent(mineObject.transform, false);
            visual.transform.localScale = Vector3.one * size;
            Object.DestroyImmediate(visual.GetComponent<Collider>());

            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null && mineData != null)
            {
                Material material = SceneMaterialFactory.CreateLitMaterial(mineData.defaultColor);
                renderer.sharedMaterial = material;
            }

            Component resource = mineObject.AddComponent(resourceType);
            SerializedObject serializedMine = new SerializedObject(resource);
            serializedMine.FindProperty("data").objectReferenceValue = mineData;
            serializedMine.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(resource);
            EditorUtility.SetDirty(mineObject);

            return mineObject;
        }

        public static GameObject CreateTree(ResourceNodeData treeData, Vector3 position)
        {
            const float treeHeight = 4f;
            const float treeRadius = 0.6f;
            const float groundClearance = 0.05f;

            Vector3 worldPosition = new Vector3(
                position.x,
                treeHeight * 0.5f + groundClearance,
                position.z);

            GameObject treeObject = EntityVisualBuilder.CreateTreeShell(
                "Tree",
                worldPosition,
                treeHeight,
                treeRadius,
                PlaceholderVisualKind.Tree);

            TreeResource treeResource = treeObject.AddComponent<TreeResource>();
            SerializedObject serializedTree = new SerializedObject(treeResource);
            serializedTree.FindProperty("data").objectReferenceValue = treeData;
            serializedTree.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(treeResource);
            EditorUtility.SetDirty(treeObject);

            return treeObject;
        }

        public static GameObject CreateHouse(PlacedBuildingData houseData, Vector3 position)
        {
            Vector3 worldPosition = new Vector3(
                position.x,
                houseData.buildingHeight * 0.5f + 0.05f,
                position.z);

            GameObject houseObject = EntityVisualBuilder.CreateBuildingShell(
                "House",
                LayerMask.NameToLayer("Building"),
                worldPosition,
                new Vector3(houseData.footprintWidth, houseData.buildingHeight, houseData.footprintDepth),
                Vector3.zero,
                PlaceholderVisualKind.House);

            House house = houseObject.AddComponent<House>();
            SerializedObject serializedHouse = new SerializedObject(house);
            serializedHouse.FindProperty("data").objectReferenceValue = houseData;
            serializedHouse.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(house);
            EditorUtility.SetDirty(houseObject);

            return houseObject;
        }

        public static void CreateLighting()
        {
            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        public static GameObject CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.layer = LayerMask.NameToLayer("Ground");
            ground.transform.localScale = new Vector3(10f, 1f, 10f);

            Renderer renderer = ground.GetComponent<Renderer>();
            renderer.sharedMaterial = SceneMaterialFactory.CreateLitMaterial(new Color(0.35f, 0.55f, 0.3f));

            return ground;
        }

        public static GameObject CreateUnit(UnitData unitData, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            string unitName = unitData != null ? unitData.displayName : "Unit";
            PlaceholderVisualKind visualKind = EntityVisualBuilder.GetUnitVisualKind(unitData);
            GameObject unitObject = EntityVisualBuilder.CreateUnitShell(unitName, position, visualKind);

            Unit unit = unitObject.AddComponent<Unit>();
            if (unitData != null)
                unit.SetData(unitData);
            unit.SetTeam(team);

            SerializedObject serializedUnit = new SerializedObject(unit);
            serializedUnit.FindProperty("data").objectReferenceValue = unitData;
            serializedUnit.FindProperty("team").enumValueIndex = (int)team;
            serializedUnit.ApplyModifiedPropertiesWithoutUndo();

            return unitObject;
        }

        public static GameObject CreateCameraRig(InputActionAsset inputActions)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<UnityEngine.Camera>();

            RTSInputReader inputReader = cameraObject.AddComponent<RTSInputReader>();
            SerializedObject serializedInput = new SerializedObject(inputReader);
            serializedInput.FindProperty("inputActions").objectReferenceValue = inputActions;
            serializedInput.ApplyModifiedPropertiesWithoutUndo();

            RTSCameraController cameraController = cameraObject.AddComponent<RTSCameraController>();
            SerializedObject serializedCamera = new SerializedObject(cameraController);
            serializedCamera.FindProperty("input").objectReferenceValue = inputReader;
            serializedCamera.ApplyModifiedPropertiesWithoutUndo();

            ApplyOverviewCamera(cameraObject.transform, Vector3.zero);

            return cameraObject;
        }

        public static void ApplyOverviewCamera(Transform cameraTransform, Vector3 focusPoint)
        {
            const float startHeight = 45f;
            const float startPitch = 55f;
            const float startYaw = -45f;

            Quaternion rotation = Quaternion.Euler(startPitch, startYaw, 0f);
            cameraTransform.rotation = rotation;

            Vector3 forward = rotation * Vector3.forward;
            if (Mathf.Abs(forward.y) > 0.001f)
            {
                float distance = (focusPoint.y - startHeight) / forward.y;
                cameraTransform.position = focusPoint - forward * distance;
            }

            RTSCameraController controller = cameraTransform.GetComponent<RTSCameraController>();
            if (controller == null)
                return;

            SerializedObject serializedCamera = new SerializedObject(controller);
            serializedCamera.FindProperty("startFocusPoint").vector3Value = focusPoint;
            serializedCamera.FindProperty("startHeight").floatValue = startHeight;
            serializedCamera.FindProperty("startPitch").floatValue = startPitch;
            serializedCamera.FindProperty("startYaw").floatValue = startYaw;
            serializedCamera.FindProperty("applyStartViewOnLoad").boolValue = true;
            serializedCamera.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
        }

        static void CreateManagers(InputActionAsset inputActions, UnityEngine.Camera mainCamera)
        {
            CreateManagersPhase1(inputActions, mainCamera);
        }

        public static void CreateManagersPhase1(InputActionAsset inputActions, UnityEngine.Camera mainCamera)
        {
            GameObject systems = new GameObject("Systems");

            GameObject unitManagerObject = new GameObject("UnitManager");
            unitManagerObject.transform.SetParent(systems.transform);
            unitManagerObject.AddComponent<UnitManager>();

            GameObject selectionManagerObject = new GameObject("SelectionManager");
            selectionManagerObject.transform.SetParent(systems.transform);
            SelectionManager selectionManager = selectionManagerObject.AddComponent<SelectionManager>();
            selectionManagerObject.AddComponent<SelectionBoxView>();

            RTSInputReader inputReader = mainCamera.GetComponent<RTSInputReader>();
            SerializedObject serializedSelection = new SerializedObject(selectionManager);
            serializedSelection.FindProperty("mainCamera").objectReferenceValue = mainCamera;
            serializedSelection.FindProperty("input").objectReferenceValue = inputReader;
            SerializedProperty boxView = serializedSelection.FindProperty("selectionBoxView");
            boxView.objectReferenceValue = selectionManagerObject.GetComponent<SelectionBoxView>();
            serializedSelection.ApplyModifiedPropertiesWithoutUndo();
        }

        static bool SyncMilitiaStats(UnitData data)
        {
            bool dirty = false;
            if (data.displayName != "Militia") { data.displayName = "Militia"; dirty = true; }
            if (data.maxHp != 40f) { data.maxHp = 40f; dirty = true; }
            if (data.moveSpeed != 5f) { data.moveSpeed = 5f; dirty = true; }
            if (data.attack != 4f) { data.attack = 4f; dirty = true; }
            dirty |= SyncUnitCombatProfile(data, AttackDamageType.Melee, 0f, 0f, UnitArmorClass.Infantry);
            if (data.attackRange != 2f) { data.attackRange = 2f; dirty = true; }
            if (data.attackCooldown != 1f) { data.attackCooldown = 1f; dirty = true; }
            if (data.team != UnitTeam.Player) { data.team = UnitTeam.Player; dirty = true; }
            return dirty;
        }

        static bool SyncManAtArmsStats(UnitData data)
        {
            bool dirty = false;
            if (data.displayName != "Man-at-Arms") { data.displayName = "Man-at-Arms"; dirty = true; }
            if (data.maxHp != 45f) { data.maxHp = 45f; dirty = true; }
            if (data.moveSpeed != 5f) { data.moveSpeed = 5f; dirty = true; }
            if (data.attack != 6f) { data.attack = 6f; dirty = true; }
            dirty |= SyncUnitCombatProfile(data, AttackDamageType.Melee, 0f, 1f, UnitArmorClass.Infantry);
            if (data.attackRange != 2f) { data.attackRange = 2f; dirty = true; }
            if (data.attackCooldown != 1f) { data.attackCooldown = 1f; dirty = true; }
            if (data.team != UnitTeam.Player) { data.team = UnitTeam.Player; dirty = true; }
            if (data.defaultColor != new Color(0.35f, 0.42f, 0.58f))
            {
                data.defaultColor = new Color(0.35f, 0.42f, 0.58f);
                dirty = true;
            }

            return dirty;
        }

        static bool SyncInfantryUpgradeTech(TechnologyData data, UnitData militiaData, UnitData manAtArmsData)
        {
            bool dirty = false;
            if (data.kind != TechnologyKind.InfantryUpgrade) { data.kind = TechnologyKind.InfantryUpgrade; dirty = true; }
            if (data.displayName != "Infantry Upgrade") { data.displayName = "Infantry Upgrade"; dirty = true; }
            if (data.prerequisiteAge != GameAge.Feudal) { data.prerequisiteAge = GameAge.Feudal; dirty = true; }
            if (data.foodCost != 100f) { data.foodCost = 100f; dirty = true; }
            if (data.goldCost != 50f) { data.goldCost = 50f; dirty = true; }
            if (data.researchTimeSeconds != 75f) { data.researchTimeSeconds = 75f; dirty = true; }
            if (data.outputUnitData != manAtArmsData) { data.outputUnitData = manAtArmsData; dirty = true; }
            if (data.replacesUnitData != militiaData) { data.replacesUnitData = militiaData; dirty = true; }
            return dirty;
        }

        static bool SyncSpearmanStats(UnitData data)
        {
            bool dirty = false;
            if (data.displayName != "Spearman") { data.displayName = "Spearman"; dirty = true; }
            if (data.maxHp != 45f) { data.maxHp = 45f; dirty = true; }
            if (data.moveSpeed != 5f) { data.moveSpeed = 5f; dirty = true; }
            if (data.attack != 3f) { data.attack = 3f; dirty = true; }
            dirty |= SyncUnitCombatProfile(data, AttackDamageType.Melee, 0f, 0f, UnitArmorClass.Infantry);
            if (data.attackRange != 2f) { data.attackRange = 2f; dirty = true; }
            if (data.attackCooldown != 1f) { data.attackCooldown = 1f; dirty = true; }
            if (data.team != UnitTeam.Player) { data.team = UnitTeam.Player; dirty = true; }
            if (data.defaultColor != new Color(0.31f, 0.44f, 0.63f))
            {
                data.defaultColor = new Color(0.31f, 0.44f, 0.63f);
                dirty = true;
            }

            return dirty;
        }

        static bool SyncArcherStats(UnitData data)
        {
            bool dirty = false;
            if (data.displayName != "Archer") { data.displayName = "Archer"; dirty = true; }
            if (data.maxHp != 30f) { data.maxHp = 30f; dirty = true; }
            if (data.moveSpeed != 5f) { data.moveSpeed = 5f; dirty = true; }
            if (data.attack != 4f) { data.attack = 4f; dirty = true; }
            dirty |= SyncUnitCombatProfile(data, AttackDamageType.Pierce, 0f, 0f, UnitArmorClass.Infantry);
            if (data.attackRange != 6f) { data.attackRange = 6f; dirty = true; }
            if (data.attackCooldown != 2f) { data.attackCooldown = 2f; dirty = true; }
            if (data.team != UnitTeam.Player) { data.team = UnitTeam.Player; dirty = true; }
            if (data.defaultColor != new Color(0.25f, 0.65f, 0.35f))
            {
                data.defaultColor = new Color(0.25f, 0.65f, 0.35f);
                dirty = true;
            }

            return dirty;
        }

        static bool SyncCavalryStats(UnitData data)
        {
            bool dirty = false;
            if (data.displayName != "Cavalry") { data.displayName = "Cavalry"; dirty = true; }
            if (data.maxHp != 45f) { data.maxHp = 45f; dirty = true; }
            if (data.moveSpeed != 7f) { data.moveSpeed = 7f; dirty = true; }
            if (data.attack != 6f) { data.attack = 6f; dirty = true; }
            dirty |= SyncUnitCombatProfile(data, AttackDamageType.Melee, 0f, 2f, UnitArmorClass.Cavalry);
            if (data.attackRange != 2f) { data.attackRange = 2f; dirty = true; }
            if (data.attackCooldown != 1f) { data.attackCooldown = 1f; dirty = true; }
            if (data.team != UnitTeam.Player) { data.team = UnitTeam.Player; dirty = true; }
            if (data.defaultColor != new Color(0.65f, 0.45f, 0.25f))
            {
                data.defaultColor = new Color(0.65f, 0.45f, 0.25f);
                dirty = true;
            }

            return dirty;
        }

        static bool SyncScoutStats(UnitData data)
        {
            bool dirty = false;
            if (data.displayName != "Scout") { data.displayName = "Scout"; dirty = true; }
            if (data.maxHp != 30f) { data.maxHp = 30f; dirty = true; }
            if (data.moveSpeed != 9f) { data.moveSpeed = 9f; dirty = true; }
            if (data.attack != 3f) { data.attack = 3f; dirty = true; }
            dirty |= SyncUnitCombatProfile(data, AttackDamageType.Melee, 0f, 1f, UnitArmorClass.Cavalry);
            if (data.attackRange != 2f) { data.attackRange = 2f; dirty = true; }
            if (data.attackCooldown != 1.5f) { data.attackCooldown = 1.5f; dirty = true; }
            if (data.team != UnitTeam.Player) { data.team = UnitTeam.Player; dirty = true; }
            if (data.defaultColor != new Color(0.75f, 0.6f, 0.35f))
            {
                data.defaultColor = new Color(0.75f, 0.6f, 0.35f);
                dirty = true;
            }

            return dirty;
        }

        static bool SyncVillagerStats(UnitData data)
        {
            bool dirty = false;
            if (data.displayName != "Villager") { data.displayName = "Villager"; dirty = true; }
            dirty |= SyncUnitCombatProfile(data, AttackDamageType.Melee, 0f, 0f, UnitArmorClass.None);
            if (data.team != UnitTeam.Player) { data.team = UnitTeam.Player; dirty = true; }
            return dirty;
        }

        static bool SyncEnemyDummyStats(UnitData data)
        {
            bool dirty = false;
            if (data.displayName != "Enemy Dummy") { data.displayName = "Enemy Dummy"; dirty = true; }
            if (data.maxHp != 100f) { data.maxHp = 100f; dirty = true; }
            if (data.moveSpeed != 0f) { data.moveSpeed = 0f; dirty = true; }
            if (data.attack != 0f) { data.attack = 0f; dirty = true; }
            dirty |= SyncUnitCombatProfile(data, AttackDamageType.Melee, 0f, 0f, UnitArmorClass.None);
            if (data.team != UnitTeam.Enemy) { data.team = UnitTeam.Enemy; dirty = true; }
            return dirty;
        }

        static bool SyncUnitCombatProfile(
            UnitData data,
            AttackDamageType damageType,
            float meleeArmor,
            float pierceArmor,
            UnitArmorClass armorClass)
        {
            bool dirty = false;
            if (data.attackDamageType != damageType) { data.attackDamageType = damageType; dirty = true; }
            if (data.meleeArmor != meleeArmor) { data.meleeArmor = meleeArmor; dirty = true; }
            if (data.pierceArmor != pierceArmor) { data.pierceArmor = pierceArmor; dirty = true; }
            if (data.armorClass != armorClass) { data.armorClass = armorClass; dirty = true; }
            return dirty;
        }
    }
}
