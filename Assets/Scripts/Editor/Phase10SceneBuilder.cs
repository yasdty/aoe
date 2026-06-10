using AoE.RTS.AI;
using AoE.RTS.Buildings;
using AoE.RTS.Camera;
using AoE.RTS.Combat;
using AoE.RTS.Commands;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Input;
using AoE.RTS.Selection;
using AoE.RTS.Spatial;
using AoE.RTS.Units;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace AoE.RTS.EditorTools
{
    public static class Phase10SceneBuilder
    {
        const string ScenePath = "Assets/Scenes/Phase10.unity";
        const float DefaultAttackWaveIntervalSeconds = 30f;
        const float DefaultBarracksBuildDelaySeconds = 60f;
        const float DefaultRelaxedFirstAttackGraceSeconds = 120f;
        const float DefaultRelaxedBarracksBuildDelaySeconds = 90f;
        const float DefaultRelaxedAttackWaveIntervalSeconds = 300f;

        static readonly Vector3 PlayerTownCenterPosition = Vector3.zero;
        static readonly Vector3 CpuTownCenterPosition = new Vector3(0f, 0f, -60f);
        static readonly Vector3 CameraFocus = new Vector3(0f, 0f, -30f);
        static readonly Vector3 SandboxGroundScale = new Vector3(18f, 1f, 18f);
        static readonly Vector3 SandboxGroundPosition = new Vector3(0f, 0f, -30f);

        static readonly Vector3[] TreePositions =
        {
            new Vector3(12f, 0f, 8f),
            new Vector3(-14f, 0f, 10f),
            new Vector3(18f, 0f, -6f),
            new Vector3(-10f, 0f, -12f),
            new Vector3(22f, 0f, 14f),
            new Vector3(-20f, 0f, -8f),
            new Vector3(8f, 0f, -18f),
            new Vector3(-16f, 0f, 18f),
            new Vector3(0f, 0f, 12f),
            new Vector3(6f, 0f, 16f),
            new Vector3(-8f, 0f, 14f),
            new Vector3(14f, 0f, 0f),
            new Vector3(-18f, 0f, 2f),
            new Vector3(4f, 0f, -8f),
            new Vector3(-6f, 0f, -6f),
            new Vector3(16f, 0f, 6f),
            new Vector3(-12f, 0f, 16f),
            new Vector3(20f, 0f, 4f),
            new Vector3(-22f, 0f, 6f),
            new Vector3(10f, 0f, 20f),
            new Vector3(-8f, 0f, 20f),
            new Vector3(24f, 0f, -4f),
            new Vector3(-18f, 0f, -4f),
            new Vector3(2f, 0f, 18f),
            new Vector3(-4f, 0f, -14f),
            new Vector3(0f, 0f, -18f),
            new Vector3(10f, 0f, -24f),
            new Vector3(-12f, 0f, -22f),
            new Vector3(6f, 0f, -32f),
            new Vector3(-8f, 0f, -34f),
            new Vector3(0f, 0f, -36f),
            new Vector3(14f, 0f, -38f),
            new Vector3(-14f, 0f, -40f),
            new Vector3(8f, 0f, -44f),
            new Vector3(-6f, 0f, -46f),
            new Vector3(0f, 0f, -48f),
            new Vector3(12f, 0f, -52f),
            new Vector3(-12f, 0f, -54f),
            new Vector3(8f, 0f, -58f),
            new Vector3(-8f, 0f, -56f),
            new Vector3(16f, 0f, -62f),
            new Vector3(-16f, 0f, -64f),
            new Vector3(6f, 0f, -68f),
            new Vector3(-6f, 0f, -70f),
            new Vector3(0f, 0f, -72f),
            new Vector3(14f, 0f, -66f),
            new Vector3(-14f, 0f, -68f),
            new Vector3(10f, 0f, -74f),
            new Vector3(-10f, 0f, -76f)
        };

        static readonly Vector3[] PlayerVillagerPositions =
        {
            new Vector3(-5f, 1f, 5f),
            new Vector3(5f, 1f, 5f),
            new Vector3(0f, 1f, 8f)
        };

        static readonly Vector3[] CpuVillagerPositions =
        {
            new Vector3(-5f, 1f, -55f),
            new Vector3(5f, 1f, -55f),
            new Vector3(0f, 1f, -68f)
        };

        static readonly Vector3[] PlayerBerryBushPositions =
        {
            new Vector3(10f, 0f, 6f),
            new Vector3(-8f, 0f, 8f),
            new Vector3(6f, 0f, -6f),
            new Vector3(12f, 0f, 4f),
            new Vector3(-10f, 0f, 5f)
        };

        static readonly Vector3[] CpuBerryBushPositions =
        {
            new Vector3(-8f, 0f, -53f),
            new Vector3(8f, 0f, -57f),
            new Vector3(0f, 0f, -63f),
            new Vector3(-10f, 0f, -58f),
            new Vector3(10f, 0f, -62f)
        };

        static readonly Vector3[] PlayerDeerPositions =
        {
            new Vector3(7f, 0f, 8f),
            new Vector3(-6f, 0f, 7f),
            new Vector3(5f, 0f, 5f),
            new Vector3(8f, 0f, 10f)
        };

        static readonly Vector3[] PlayerSheepPositions =
        {
            new Vector3(-2f, 0f, 10f),
            new Vector3(9f, 0f, 3f),
            new Vector3(-4f, 0f, 12f)
        };

        static readonly Vector3[] PlayerBoarPositions =
        {
            new Vector3(4f, 0f, 11f),
            new Vector3(-4f, 0f, 9f),
            new Vector3(6f, 0f, 12f)
        };

        static readonly Vector3[] CpuBoarPositions =
        {
            new Vector3(-5f, 0f, -55f),
            new Vector3(5f, 0f, -61f)
        };

        static readonly Vector3[] CpuDeerPositions =
        {
            new Vector3(-6f, 0f, -51f),
            new Vector3(6f, 0f, -59f),
            new Vector3(0f, 0f, -65f)
        };

        static readonly Vector3[] CpuSheepPositions =
        {
            new Vector3(0f, 0f, -67f),
            new Vector3(5f, 0f, -64f)
        };

        static readonly Vector3[] PlayerGoldMinePositions =
        {
            new Vector3(16f, 0f, 12f),
            new Vector3(20f, 0f, 18f)
        };

        static readonly Vector3[] PlayerStoneMinePositions =
        {
            new Vector3(-16f, 0f, 12f),
            new Vector3(-20f, 0f, 18f)
        };

        static readonly Vector3[] CpuGoldMinePositions =
        {
            new Vector3(10f, 0f, -55f),
            new Vector3(18f, 0f, -70f)
        };

        static readonly Vector3[] CpuStoneMinePositions =
        {
            new Vector3(-10f, 0f, -57f),
            new Vector3(-18f, 0f, -72f)
        };

        [MenuItem("AoE/Setup Phase10 Scene", true)]
        static bool ValidateSetupPhase10Scene() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Setup Phase10 Scene")]
        public static void SetupPhase10Scene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            Phase1SceneBuilder.EnsureLayers();
            UnitData villagerData = Phase1SceneBuilder.EnsureDefaultUnitData();
            UnitData militiaData = Phase1SceneBuilder.EnsureMilitiaData();
            UnitData spearmanData = Phase1SceneBuilder.EnsureSpearmanData();
            UnitData archerData = Phase1SceneBuilder.EnsureArcherData();
            UnitData cavalryData = Phase1SceneBuilder.EnsureCavalryData();
            UnitData scoutData = Phase1SceneBuilder.EnsureScoutData();
            BuildingData townCenterData = Phase1SceneBuilder.EnsureTownCenterData(villagerData);
            ResourceNodeData treeData = Phase1SceneBuilder.EnsureDefaultTreeData();
            FoodNodeData berryBushData = Phase1SceneBuilder.EnsureDefaultBerryBushData();
            FoodNodeData deerData = Phase1SceneBuilder.EnsureDefaultDeerData();
            FoodNodeData sheepData = Phase1SceneBuilder.EnsureDefaultSheepData();
            FoodNodeData boarData = Phase1SceneBuilder.EnsureDefaultBoarData();
            MineralNodeData goldMineData = Phase1SceneBuilder.EnsureDefaultGoldMineData();
            MineralNodeData stoneMineData = Phase1SceneBuilder.EnsureDefaultStoneMineData();
            PlacedBuildingData houseData = Phase1SceneBuilder.EnsureHouseData();
            PlacedBuildingData barracksData = Phase1SceneBuilder.EnsureBarracksData(militiaData, spearmanData);
            PlacedBuildingData archeryRangeData = Phase1SceneBuilder.EnsureArcheryRangeData(archerData);
            PlacedBuildingData stableData = Phase1SceneBuilder.EnsureStableData(cavalryData, scoutData);
            PlacedBuildingData blacksmithData = Phase1SceneBuilder.EnsureBlacksmithData();
            PlacedBuildingData palisadeWallData = Phase1SceneBuilder.EnsurePalisadeWallData();
            PlacedBuildingData stoneWallData = Phase1SceneBuilder.EnsureStoneWallData();
            PlacedBuildingData watchTowerData = Phase1SceneBuilder.EnsureWatchTowerData();
            PlacedBuildingData marketData = Phase1SceneBuilder.EnsureMarketData();
            MarketTradeData marketTradeData = Phase1SceneBuilder.EnsureMarketTradeData();
            PlacedBuildingData millData = Phase1SceneBuilder.EnsureMillData();
            UnitData manAtArmsData = Phase1SceneBuilder.EnsureManAtArmsData();
            TechnologyData infantryUpgradeTech = Phase1SceneBuilder.EnsureInfantryUpgradeTech(militiaData, manAtArmsData);
            Phase1SceneBuilder.EnsureFarmData();
            Phase1SceneBuilder.EnsureLumberCampData();
            Phase1SceneBuilder.EnsureMiningCampData();
            Phase1SceneBuilder.EnsureMillData();
            Phase1SceneBuilder.EnsureFeudalAgeData();
            InputActionAsset inputActions = RTSInputActionsFactory.EnsureAsset();
            if (inputActions == null)
            {
                Debug.LogError("Failed to create RTSInputActions. Setup aborted.");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Phase1SceneBuilder.CreateLighting();
            CreateSandboxGround();
            GameObject playerTownCenter = Phase1SceneBuilder.CreateTownCenter(townCenterData, PlayerTownCenterPosition);
            GameObject cpuTownCenter = CreateCpuTownCenter(townCenterData);
            CreateTrees(treeData);
            CreateBerryBushes(berryBushData);
            CreateHuntableAnimals(deerData, sheepData);
            CreateBoars(boarData);
            CreateMineralMines(goldMineData, stoneMineData);
            CreateStartingVillagers(villagerData, PlayerVillagerPositions, UnitTeam.Player);
            CreateStartingVillagers(villagerData, CpuVillagerPositions, UnitTeam.Enemy);
            GameObject cameraRig = Phase1SceneBuilder.CreateCameraRig(inputActions);
            Phase1SceneBuilder.ApplyOverviewCamera(cameraRig.transform, CameraFocus);
            CreateManagers(
                inputActions,
                cameraRig.GetComponent<UnityEngine.Camera>(),
                houseData,
                barracksData,
                archeryRangeData,
                stableData,
                blacksmithData,
                palisadeWallData,
                stoneWallData,
                watchTowerData,
                marketData,
                marketTradeData,
                millData,
                infantryUpgradeTech,
                villagerData,
                militiaData);

            Phase1SceneBuilder.AssignInputActionsToReaders(inputActions);
            EditorSceneManager.SaveScene(scene, ScenePath);
            UnityEditor.Selection.activeGameObject = playerTownCenter;

            Debug.Log("Phase10 scene created at " + ScenePath);
        }

        [MenuItem("AoE/Add Huntable Animals (Phase10)", true)]
        static bool ValidateAddHuntableAnimals() => !EditorApplication.isPlaying;

        /// <summary>
        /// 既存 Phase10 シーンに Deer / Sheep を追加（フル Setup 不要）。
        /// Phase 24 以降、古い Phase10.unity を使っている場合に実行する。
        /// </summary>
        [MenuItem("AoE/Add Huntable Animals (Phase10)")]
        public static void AddHuntableAnimalsToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            Phase1SceneBuilder.EnsureLayers();
            FoodNodeData deerData = Phase1SceneBuilder.EnsureDefaultDeerData();
            FoodNodeData sheepData = Phase1SceneBuilder.EnsureDefaultSheepData();

            RemoveExistingHuntableAnimals();
            CreateHuntableAnimals(deerData, sheepData);
            ResetAllSheepToNeutral();
            EnsureAnimalManagers();

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added Deer/Sheep to the open scene. Save the scene (Ctrl+S) if needed.");
        }

        [MenuItem("AoE/Add Selection Info Panel (Phase10)", true)]
        static bool ValidateAddSelectionInfoPanel() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add Selection Info Panel (Phase10)")]
        public static void AddSelectionInfoPanelToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            SelectionManager selectionManager = Object.FindAnyObjectByType<SelectionManager>();
            if (selectionManager == null)
            {
                Debug.LogError("SelectionManager not found in the open scene.");
                return;
            }

            SelectionInfoPanelView existing = selectionManager.GetComponent<SelectionInfoPanelView>();
            if (existing != null)
            {
                Debug.Log("SelectionInfoPanelView already exists on SelectionManager.");
                return;
            }

            SelectionInfoPanelView infoPanel = selectionManager.gameObject.AddComponent<SelectionInfoPanelView>();
            SerializedObject serializedInfoPanel = new SerializedObject(infoPanel);
            serializedInfoPanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedInfoPanel.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added SelectionInfoPanelView. Save the scene (Ctrl+S) if needed.");
        }

        [MenuItem("AoE/Add Boars (Phase10)", true)]
        static bool ValidateAddBoars() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add Boars (Phase10)")]
        public static void AddBoarsToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            Phase1SceneBuilder.EnsureLayers();
            FoodNodeData boarData = Phase1SceneBuilder.EnsureDefaultBoarData();
            EnsureBoarManagers();

            BoarResource[] existing = Object.FindObjectsByType<BoarResource>();
            for (int i = 0; i < existing.Length; i++)
                Object.DestroyImmediate(existing[i].gameObject);

            CreateBoars(boarData);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added Boars to the open scene. Save the scene (Ctrl+S) if needed.");
        }

        static void EnsureBoarManagers()
        {
            if (Object.FindAnyObjectByType<BoarAggroManager>() == null)
            {
                GameObject systems = GameObject.Find("Systems");
                Transform parent = systems != null ? systems.transform : null;
                GameObject aggroObject = new GameObject("BoarAggroManager");
                if (parent != null)
                    aggroObject.transform.SetParent(parent);
                aggroObject.AddComponent<BoarAggroManager>();
            }

            if (Object.FindAnyObjectByType<BoarAttackManager>() == null)
            {
                GameObject systems = GameObject.Find("Systems");
                Transform parent = systems != null ? systems.transform : null;
                GameObject attackObject = new GameObject("BoarAttackManager");
                if (parent != null)
                    attackObject.transform.SetParent(parent);
                attackObject.AddComponent<BoarAttackManager>();
            }
        }

        [MenuItem("AoE/Reset Sheep to Neutral (Phase10)", true)]
        static bool ValidateResetSheepToNeutral() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Reset Sheep to Neutral (Phase10)")]
        public static void ResetSheepToNeutralInOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            ResetAllSheepToNeutral();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Reset all Sheep to Neutral. Save the scene (Ctrl+S) if needed.");
        }

        [MenuItem("AoE/Add Animal Locomotion (Phase10)", true)]
        static bool ValidateAddAnimalLocomotion() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add Animal Locomotion (Phase10)")]
        public static void AddAnimalLocomotionToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            EnsureAnimalManagers();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added AnimalDiscoveryManager + PassiveAnimalLocomotionManager. Save the scene (Ctrl+S) if needed.");
        }

        [MenuItem("AoE/Add Unit Aggro (Phase10)", true)]
        static bool ValidateAddUnitAggro() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add Unit Aggro (Phase10)")]
        public static void AddUnitAggroToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            EnsureUnitAggroManager();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added UnitAggroManager. Save the scene (Ctrl+S) if needed.");
        }

        [MenuItem("AoE/Add Stance & Attack-Move (Phase40)", true)]
        static bool ValidateAddStanceAttackMove() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add Stance & Attack-Move (Phase40)")]
        public static void AddStanceAttackMoveToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            InputActionAsset inputActions = RTSInputActionsFactory.EnsureAsset();
            if (inputActions == null)
            {
                Debug.LogError("Failed to create RTSInputActions. Setup aborted.");
                return;
            }

            Phase1SceneBuilder.AssignInputActionsToReaders(inputActions);
            EnsureAttackMoveManager();
            EnsureFormationMoveManager();

            SelectionManager selectionManager = Object.FindAnyObjectByType<SelectionManager>();
            if (selectionManager != null)
            {
                UnitStancePanelView existing = selectionManager.GetComponent<UnitStancePanelView>();
                if (existing == null)
                {
                    UnitStancePanelView stancePanel = selectionManager.gameObject.AddComponent<UnitStancePanelView>();
                    SerializedObject serializedStancePanel = new SerializedObject(stancePanel);
                    serializedStancePanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
                    serializedStancePanel.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            else
            {
                Debug.LogWarning("SelectionManager not found — UnitStancePanelView was not added.");
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added AttackMoveManager + UnitStancePanelView and refreshed input actions. Save the scene (Ctrl+S) if needed.");
        }

        [MenuItem("AoE/Add Blacksmith & Tech (Phase43)", true)]
        static bool ValidateAddBlacksmithTech() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add Blacksmith & Tech (Phase43)")]
        public static void AddBlacksmithTechToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            UnitData militiaData = Phase1SceneBuilder.EnsureMilitiaData();
            UnitData manAtArmsData = Phase1SceneBuilder.EnsureManAtArmsData();
            TechnologyData infantryUpgradeTech = Phase1SceneBuilder.EnsureInfantryUpgradeTech(militiaData, manAtArmsData);
            PlacedBuildingData blacksmithData = Phase1SceneBuilder.EnsureBlacksmithData();

            GameObject systems = GameObject.Find("Systems");
            Transform systemsTransform = systems != null ? systems.transform : null;
            if (Object.FindAnyObjectByType<BlacksmithResearchManager>() == null)
            {
                GameObject blacksmithResearchObject = new GameObject("BlacksmithResearchManager");
                if (systemsTransform != null)
                    blacksmithResearchObject.transform.SetParent(systemsTransform);
                blacksmithResearchObject.AddComponent<BlacksmithResearchManager>();
            }

            SelectionManager selectionManager = Object.FindAnyObjectByType<SelectionManager>();
            if (selectionManager != null)
            {
                BlacksmithPanelView blacksmithPanel = selectionManager.GetComponent<BlacksmithPanelView>();
                if (blacksmithPanel == null)
                    blacksmithPanel = selectionManager.gameObject.AddComponent<BlacksmithPanelView>();

                RTSInputReader inputReader = Object.FindAnyObjectByType<RTSInputReader>();
                SerializedObject serializedBlacksmithPanel = new SerializedObject(blacksmithPanel);
                serializedBlacksmithPanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
                serializedBlacksmithPanel.FindProperty("input").objectReferenceValue = inputReader;
                serializedBlacksmithPanel.FindProperty("infantryUpgradeTech").objectReferenceValue = infantryUpgradeTech;
                serializedBlacksmithPanel.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogWarning("SelectionManager not found — BlacksmithPanelView was not added.");
            }

            BuildingPlacementManager placementManager = Object.FindAnyObjectByType<BuildingPlacementManager>();
            if (placementManager != null)
            {
                SerializedObject serializedPlacement = new SerializedObject(placementManager);
                serializedPlacement.FindProperty("blacksmithData").objectReferenceValue = blacksmithData;
                serializedPlacement.ApplyModifiedPropertiesWithoutUndo();
            }

            ResourceHudView resourceHud = Object.FindAnyObjectByType<ResourceHudView>();
            if (resourceHud != null)
            {
                SerializedObject serializedResourceHud = new SerializedObject(resourceHud);
                serializedResourceHud.FindProperty("blacksmithData").objectReferenceValue = blacksmithData;
                serializedResourceHud.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added Blacksmith & Tech wiring. Save the scene (Ctrl+S) if needed.");
        }

        [MenuItem("AoE/Add Defense (Phase44)", true)]
        static bool ValidateAddDefense() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add Defense (Phase44)")]
        public static void AddDefenseToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            PlacedBuildingData palisadeWallData = Phase1SceneBuilder.EnsurePalisadeWallData();
            PlacedBuildingData stoneWallData = Phase1SceneBuilder.EnsureStoneWallData();
            PlacedBuildingData watchTowerData = Phase1SceneBuilder.EnsureWatchTowerData();

            GameObject systems = GameObject.Find("Systems");
            Transform systemsTransform = systems != null ? systems.transform : null;
            if (Object.FindAnyObjectByType<WatchTowerDefenseManager>() == null)
            {
                GameObject watchTowerDefenseObject = new GameObject("WatchTowerDefenseManager");
                if (systemsTransform != null)
                    watchTowerDefenseObject.transform.SetParent(systemsTransform);
                watchTowerDefenseObject.AddComponent<WatchTowerDefenseManager>();
            }

            BuildingPlacementManager placementManager = Object.FindAnyObjectByType<BuildingPlacementManager>();
            if (placementManager != null)
            {
                SerializedObject serializedPlacement = new SerializedObject(placementManager);
                serializedPlacement.FindProperty("palisadeWallData").objectReferenceValue = palisadeWallData;
                serializedPlacement.FindProperty("stoneWallData").objectReferenceValue = stoneWallData;
                serializedPlacement.FindProperty("watchTowerData").objectReferenceValue = watchTowerData;
                serializedPlacement.ApplyModifiedPropertiesWithoutUndo();
            }

            ResourceHudView resourceHud = Object.FindAnyObjectByType<ResourceHudView>();
            if (resourceHud != null)
            {
                SerializedObject serializedResourceHud = new SerializedObject(resourceHud);
                serializedResourceHud.FindProperty("palisadeWallData").objectReferenceValue = palisadeWallData;
                serializedResourceHud.FindProperty("stoneWallData").objectReferenceValue = stoneWallData;
                serializedResourceHud.FindProperty("watchTowerData").objectReferenceValue = watchTowerData;
                serializedResourceHud.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added Defense wiring. Save the scene (Ctrl+S) if needed.");
        }

        [MenuItem("AoE/Add Market (Phase45)", true)]
        static bool ValidateAddMarket() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add Market (Phase45)")]
        public static void AddMarketToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            PlacedBuildingData marketData = Phase1SceneBuilder.EnsureMarketData();
            MarketTradeData marketTradeData = Phase1SceneBuilder.EnsureMarketTradeData();

            SelectionManager selectionManager = Object.FindAnyObjectByType<SelectionManager>();
            if (selectionManager != null)
            {
                MarketPanelView marketPanel = selectionManager.GetComponent<MarketPanelView>();
                if (marketPanel == null)
                    marketPanel = selectionManager.gameObject.AddComponent<MarketPanelView>();

                SerializedObject serializedMarketPanel = new SerializedObject(marketPanel);
                serializedMarketPanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
                serializedMarketPanel.FindProperty("tradeRates").objectReferenceValue = marketTradeData;
                serializedMarketPanel.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogWarning("SelectionManager not found — MarketPanelView was not added.");
            }

            BuildingPlacementManager placementManager = Object.FindAnyObjectByType<BuildingPlacementManager>();
            if (placementManager != null)
            {
                SerializedObject serializedPlacement = new SerializedObject(placementManager);
                serializedPlacement.FindProperty("marketData").objectReferenceValue = marketData;
                serializedPlacement.ApplyModifiedPropertiesWithoutUndo();
            }

            ResourceHudView resourceHud = Object.FindAnyObjectByType<ResourceHudView>();
            if (resourceHud != null)
            {
                SerializedObject serializedResourceHud = new SerializedObject(resourceHud);
                serializedResourceHud.FindProperty("marketData").objectReferenceValue = marketData;
                serializedResourceHud.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added Market wiring. Save the scene (Ctrl+S) if needed.");
        }

        [MenuItem("AoE/Add Civilization (Phase46)", true)]
        static bool ValidateAddCivilization() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add Civilization (Phase46)")]
        public static void AddCivilizationToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            CivilizationData playerCivilization = Phase1SceneBuilder.EnsureDefaultPlayerCivilizationData();
            CivilizationData cpuCivilization = Phase1SceneBuilder.EnsureDefaultCpuCivilizationData();

            GameSessionManager sessionManager = Object.FindAnyObjectByType<GameSessionManager>();
            if (sessionManager != null)
            {
                SerializedObject serializedSession = new SerializedObject(sessionManager);
                serializedSession.FindProperty("playerCivilization").objectReferenceValue = playerCivilization;
                serializedSession.FindProperty("enemyCivilization").objectReferenceValue = cpuCivilization;
                serializedSession.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(sessionManager);
            }
            else
            {
                Debug.LogWarning("GameSessionManager not found — civilization data was not wired.");
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added Civilization wiring. Save the scene (Ctrl+S) if needed.");
        }

        static void EnsureAttackMoveManager()
        {
            if (Object.FindAnyObjectByType<AttackMoveManager>() != null)
                return;

            GameObject systems = GameObject.Find("Systems");
            Transform parent = systems != null ? systems.transform : null;
            GameObject attackMoveObject = new GameObject("AttackMoveManager");
            if (parent != null)
                attackMoveObject.transform.SetParent(parent);
            attackMoveObject.AddComponent<AttackMoveManager>();
        }

        static void EnsureFormationMoveManager()
        {
            if (Object.FindAnyObjectByType<FormationMoveManager>() != null)
                return;

            GameObject systems = GameObject.Find("Systems");
            Transform parent = systems != null ? systems.transform : null;
            GameObject formationObject = new GameObject("FormationMoveManager");
            if (parent != null)
                formationObject.transform.SetParent(parent);
            formationObject.AddComponent<FormationMoveManager>();
        }

        [MenuItem("AoE/Add Formation (Phase41)", true)]
        static bool ValidateAddFormation() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add Formation (Phase41)")]
        public static void AddFormationToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            EnsureFormationMoveManager();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added FormationMoveManager. Save the scene (Ctrl+S) if needed.");
        }

        static void EnsureUnitAggroManager()
        {
            if (Object.FindAnyObjectByType<UnitAggroManager>() != null)
                return;

            GameObject systems = GameObject.Find("Systems");
            Transform parent = systems != null ? systems.transform : null;
            GameObject aggroObject = new GameObject("UnitAggroManager");
            if (parent != null)
                aggroObject.transform.SetParent(parent);
            aggroObject.AddComponent<UnitAggroManager>();
        }

        static void EnsureAnimalManagers()
        {
            if (Object.FindAnyObjectByType<AnimalDiscoveryManager>() == null)
            {
                GameObject systems = GameObject.Find("Systems");
                Transform parent = systems != null ? systems.transform : null;
                GameObject discoveryObject = new GameObject("AnimalDiscoveryManager");
                if (parent != null)
                    discoveryObject.transform.SetParent(parent);
                discoveryObject.AddComponent<AnimalDiscoveryManager>();
            }

            if (Object.FindAnyObjectByType<PassiveAnimalLocomotionManager>() == null)
            {
                GameObject systems = GameObject.Find("Systems");
                Transform parent = systems != null ? systems.transform : null;
                GameObject locomotionObject = new GameObject("PassiveAnimalLocomotionManager");
                if (parent != null)
                    locomotionObject.transform.SetParent(parent);
                locomotionObject.AddComponent<PassiveAnimalLocomotionManager>();
            }
        }

        static void ResetAllSheepToNeutral()
        {
            SheepResource[] sheep = Object.FindObjectsByType<SheepResource>();
            for (int i = 0; i < sheep.Length; i++)
            {
                if (sheep[i] == null)
                    continue;

                sheep[i].ResetToNeutral();
                EditorUtility.SetDirty(sheep[i]);
            }
        }

        static void RemoveExistingHuntableAnimals()
        {
            DeerResource[] deer = Object.FindObjectsByType<DeerResource>();
            for (int i = 0; i < deer.Length; i++)
                Object.DestroyImmediate(deer[i].gameObject);

            SheepResource[] sheep = Object.FindObjectsByType<SheepResource>();
            for (int i = 0; i < sheep.Length; i++)
                Object.DestroyImmediate(sheep[i].gameObject);
        }

        static GameObject CreateCpuTownCenter(BuildingData townCenterData)
        {
            GameObject townCenterObject = Phase1SceneBuilder.CreateTownCenter(townCenterData, CpuTownCenterPosition);
            TownCenter townCenter = townCenterObject.GetComponent<TownCenter>();
            townCenter.SetTeam(UnitTeam.Enemy);
            EditorUtility.SetDirty(townCenter);
            return townCenterObject;
        }

        static void CreateStartingVillagers(UnitData villagerData, Vector3[] positions, UnitTeam team)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                GameObject unitObject = Phase1SceneBuilder.CreateUnit(
                    villagerData,
                    positions[i],
                    team);
                Unit unit = unitObject.GetComponent<Unit>();
                if (unit != null)
                    unit.SetTeam(team);
            }
        }

        [MenuItem("AoE/Sync AoE2 Classic Start (Phase10)", true)]
        static bool ValidateSyncAoE2ClassicStart() => !EditorApplication.isPlaying;

        /// <summary>
        /// 既存 Phase10 シーンを AoE2 Classic 開始（200F/200W、両チーム村民3）に揃える。
        /// </summary>
        [MenuItem("AoE/Sync AoE2 Classic Start (Phase10)")]
        public static void SyncAoE2ClassicStartToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            UnitData villagerData = Phase1SceneBuilder.EnsureDefaultUnitData();
            ApplyClassicStartingResourcesToScene();
            ApplyPhase42SceneWiring();
            EnsureStartingVillagerCount(villagerData, UnitTeam.Player, PlayerVillagerPositions);
            EnsureStartingVillagerCount(villagerData, UnitTeam.Enemy, CpuVillagerPositions);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Synced AoE2 Classic start: 200 Food + 200 Wood, 3 Villagers per team. Save the scene (Ctrl+S).");
        }

        public static void BatchSyncPhase10SceneClassicStart()
        {
            EditorSceneManager.OpenScene(ScenePath);
            SyncAoE2ClassicStartToOpenScene();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        static void ApplyClassicStartingResourcesToScene()
        {
            ResourceManager resourceManager = Object.FindAnyObjectByType<ResourceManager>();
            if (resourceManager == null)
            {
                Debug.LogWarning("ResourceManager not found — starting resources were not updated.");
                return;
            }

            SerializedObject serialized = new SerializedObject(resourceManager);
            serialized.FindProperty("initialPlayerFood").floatValue = 200f;
            serialized.FindProperty("initialPlayerWood").floatValue = 200f;
            serialized.FindProperty("initialEnemyFood").floatValue = 200f;
            serialized.FindProperty("initialEnemyWood").floatValue = 200f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(resourceManager);
        }

        static void ApplyPhase42SceneWiring()
        {
            AgeData feudalAgeData = Phase1SceneBuilder.EnsureFeudalAgeData();
            CivilizationData playerCivilization = Phase1SceneBuilder.EnsureDefaultPlayerCivilizationData();
            CivilizationData cpuCivilization = Phase1SceneBuilder.EnsureDefaultCpuCivilizationData();

            GameSessionManager sessionManager = Object.FindAnyObjectByType<GameSessionManager>();
            if (sessionManager != null)
            {
                SerializedObject serializedSession = new SerializedObject(sessionManager);
                serializedSession.FindProperty("balanceMode").enumValueIndex = (int)GameplayBalanceMode.Debug;
                serializedSession.FindProperty("cpuAttackPace").enumValueIndex = (int)CpuAttackPace.Relaxed;
                serializedSession.FindProperty("feudalAgeData").objectReferenceValue = feudalAgeData;
                serializedSession.FindProperty("playerCivilization").objectReferenceValue = playerCivilization;
                serializedSession.FindProperty("enemyCivilization").objectReferenceValue = cpuCivilization;
                serializedSession.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(sessionManager);
            }

            ProductionPanelView productionPanel = Object.FindAnyObjectByType<ProductionPanelView>();
            if (productionPanel != null)
            {
                SerializedObject serializedPanel = new SerializedObject(productionPanel);
                serializedPanel.FindProperty("feudalAgeData").objectReferenceValue = feudalAgeData;
                serializedPanel.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(productionPanel);
            }
        }

        static void EnsureStartingVillagerCount(UnitData villagerData, UnitTeam team, Vector3[] spawnPositions)
        {
            RemoveTeamUnitsWithoutData(team);

            Unit[] units = Object.FindObjectsByType<Unit>();
            var villagers = new System.Collections.Generic.List<Unit>(spawnPositions.Length);
            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (unit == null || unit.Team != team || unit.Data == null)
                    continue;

                if (unit.Data.displayName != "Villager")
                    continue;

                villagers.Add(unit);
            }

            for (int i = villagers.Count - 1; i >= spawnPositions.Length; i--)
                Object.DestroyImmediate(villagers[i].gameObject);

            int villagerCount = Mathf.Min(villagers.Count, spawnPositions.Length);
            for (int i = villagerCount; i < spawnPositions.Length; i++)
            {
                GameObject unitObject = Phase1SceneBuilder.CreateUnit(
                    villagerData,
                    spawnPositions[i],
                    team);
                Unit unit = unitObject.GetComponent<Unit>();
                if (unit != null)
                    unit.SetTeam(team);
            }
        }

        static void RemoveTeamUnitsWithoutData(UnitTeam team)
        {
            Unit[] units = Object.FindObjectsByType<Unit>();
            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (unit == null || unit.Team != team || unit.Data != null)
                    continue;

                Object.DestroyImmediate(unit.gameObject);
            }
        }

        static void CreateBerryBushes(FoodNodeData bushData)
        {
            for (int i = 0; i < PlayerBerryBushPositions.Length; i++)
                Phase1SceneBuilder.CreateBerryBush(bushData, PlayerBerryBushPositions[i]);

            for (int i = 0; i < CpuBerryBushPositions.Length; i++)
                Phase1SceneBuilder.CreateBerryBush(bushData, CpuBerryBushPositions[i]);
        }

        static void CreateHuntableAnimals(FoodNodeData deerData, FoodNodeData sheepData)
        {
            for (int i = 0; i < PlayerDeerPositions.Length; i++)
                Phase1SceneBuilder.CreateDeer(deerData, PlayerDeerPositions[i]);

            for (int i = 0; i < PlayerSheepPositions.Length; i++)
                Phase1SceneBuilder.CreateSheep(sheepData, PlayerSheepPositions[i]);

            for (int i = 0; i < CpuDeerPositions.Length; i++)
                Phase1SceneBuilder.CreateDeer(deerData, CpuDeerPositions[i]);

            for (int i = 0; i < CpuSheepPositions.Length; i++)
                Phase1SceneBuilder.CreateSheep(sheepData, CpuSheepPositions[i]);

            ResetAllSheepToNeutral();
        }

        static void CreateSandboxGround()
        {
            GameObject ground = Phase1SceneBuilder.CreateGround();
            ground.transform.localScale = SandboxGroundScale;
            ground.transform.position = SandboxGroundPosition;
        }

        static void CreateBoars(FoodNodeData boarData)
        {
            for (int i = 0; i < PlayerBoarPositions.Length; i++)
                Phase1SceneBuilder.CreateBoar(boarData, PlayerBoarPositions[i]);

            for (int i = 0; i < CpuBoarPositions.Length; i++)
                Phase1SceneBuilder.CreateBoar(boarData, CpuBoarPositions[i]);
        }

        static void CreateMineralMines(MineralNodeData goldMineData, MineralNodeData stoneMineData)
        {
            for (int i = 0; i < PlayerGoldMinePositions.Length; i++)
                Phase1SceneBuilder.CreateGoldMine(goldMineData, PlayerGoldMinePositions[i]);

            for (int i = 0; i < PlayerStoneMinePositions.Length; i++)
                Phase1SceneBuilder.CreateStoneMine(stoneMineData, PlayerStoneMinePositions[i]);

            for (int i = 0; i < CpuGoldMinePositions.Length; i++)
                Phase1SceneBuilder.CreateGoldMine(goldMineData, CpuGoldMinePositions[i]);

            for (int i = 0; i < CpuStoneMinePositions.Length; i++)
                Phase1SceneBuilder.CreateStoneMine(stoneMineData, CpuStoneMinePositions[i]);
        }

        static void CreateTrees(ResourceNodeData treeData)
        {
            for (int i = 0; i < TreePositions.Length; i++)
                Phase1SceneBuilder.CreateTree(treeData, TreePositions[i]);
        }

        static void CreateManagers(
            InputActionAsset inputActions,
            UnityEngine.Camera mainCamera,
            PlacedBuildingData houseData,
            PlacedBuildingData barracksData,
            PlacedBuildingData archeryRangeData,
            PlacedBuildingData stableData,
            PlacedBuildingData blacksmithData,
            PlacedBuildingData palisadeWallData,
            PlacedBuildingData stoneWallData,
            PlacedBuildingData watchTowerData,
            PlacedBuildingData marketData,
            MarketTradeData marketTradeData,
            PlacedBuildingData millData,
            TechnologyData infantryUpgradeTech,
            UnitData villagerData,
            UnitData militiaData)
        {
            GameObject systems = new GameObject("Systems");
            AgeData feudalAgeData = Phase1SceneBuilder.EnsureFeudalAgeData();
            CivilizationData playerCivilization = Phase1SceneBuilder.EnsureDefaultPlayerCivilizationData();
            CivilizationData cpuCivilization = Phase1SceneBuilder.EnsureDefaultCpuCivilizationData();
            GameSessionManager sessionManager = systems.AddComponent<GameSessionManager>();
            SerializedObject serializedSession = new SerializedObject(sessionManager);
            serializedSession.FindProperty("balanceMode").enumValueIndex = (int)GameplayBalanceMode.Debug;
            serializedSession.FindProperty("cpuAttackPace").enumValueIndex = (int)CpuAttackPace.Relaxed;
            serializedSession.FindProperty("feudalAgeData").objectReferenceValue = feudalAgeData;
            serializedSession.FindProperty("playerCivilization").objectReferenceValue = playerCivilization;
            serializedSession.FindProperty("enemyCivilization").objectReferenceValue = cpuCivilization;
            serializedSession.ApplyModifiedPropertiesWithoutUndo();

            GameObject simulationTickObject = new GameObject("SimulationTick");
            simulationTickObject.transform.SetParent(systems.transform);
            simulationTickObject.AddComponent<SimulationTick>();

            GameObject commandQueueObject = new GameObject("CommandQueue");
            commandQueueObject.transform.SetParent(systems.transform);
            commandQueueObject.AddComponent<CommandQueue>();

            GameObject unitPoolObject = new GameObject("UnitPool");
            unitPoolObject.transform.SetParent(systems.transform);
            UnitPool unitPool = unitPoolObject.AddComponent<UnitPool>();
            SerializedObject serializedUnitPool = new SerializedObject(unitPool);
            serializedUnitPool.FindProperty("prewarmVillagerData").objectReferenceValue = villagerData;
            serializedUnitPool.FindProperty("prewarmMilitiaData").objectReferenceValue = militiaData;
            serializedUnitPool.ApplyModifiedPropertiesWithoutUndo();

            GameObject buildingPoolObject = new GameObject("BuildingPool");
            buildingPoolObject.transform.SetParent(systems.transform);
            buildingPoolObject.AddComponent<BuildingPool>();

            GameObject unitManagerObject = new GameObject("UnitManager");
            unitManagerObject.transform.SetParent(systems.transform);
            unitManagerObject.AddComponent<UnitManager>();

            GameObject unitSpatialIndexObject = new GameObject("UnitSpatialIndex");
            unitSpatialIndexObject.transform.SetParent(systems.transform);
            unitSpatialIndexObject.AddComponent<UnitSpatialIndex>();

            GameObject treeSpatialIndexObject = new GameObject("TreeSpatialIndex");
            treeSpatialIndexObject.transform.SetParent(systems.transform);
            treeSpatialIndexObject.AddComponent<TreeSpatialIndex>();

            GameObject berrySpatialIndexObject = new GameObject("BerryBushSpatialIndex");
            berrySpatialIndexObject.transform.SetParent(systems.transform);
            berrySpatialIndexObject.AddComponent<BerryBushSpatialIndex>();

            GameObject attackManagerObject = new GameObject("AttackManager");
            attackManagerObject.transform.SetParent(systems.transform);
            attackManagerObject.AddComponent<AttackManager>();

            GameObject unitAggroManagerObject = new GameObject("UnitAggroManager");
            unitAggroManagerObject.transform.SetParent(systems.transform);
            unitAggroManagerObject.AddComponent<UnitAggroManager>();

            GameObject attackMoveManagerObject = new GameObject("AttackMoveManager");
            attackMoveManagerObject.transform.SetParent(systems.transform);
            attackMoveManagerObject.AddComponent<AttackMoveManager>();

            GameObject formationMoveManagerObject = new GameObject("FormationMoveManager");
            formationMoveManagerObject.transform.SetParent(systems.transform);
            formationMoveManagerObject.AddComponent<FormationMoveManager>();

            GameObject boarAggroManagerObject = new GameObject("BoarAggroManager");
            boarAggroManagerObject.transform.SetParent(systems.transform);
            boarAggroManagerObject.AddComponent<BoarAggroManager>();

            GameObject boarAttackManagerObject = new GameObject("BoarAttackManager");
            boarAttackManagerObject.transform.SetParent(systems.transform);
            boarAttackManagerObject.AddComponent<BoarAttackManager>();

            GameObject animalDiscoveryManagerObject = new GameObject("AnimalDiscoveryManager");
            animalDiscoveryManagerObject.transform.SetParent(systems.transform);
            animalDiscoveryManagerObject.AddComponent<AnimalDiscoveryManager>();

            GameObject passiveAnimalLocomotionManagerObject = new GameObject("PassiveAnimalLocomotionManager");
            passiveAnimalLocomotionManagerObject.transform.SetParent(systems.transform);
            passiveAnimalLocomotionManagerObject.AddComponent<PassiveAnimalLocomotionManager>();

            GameObject populationManagerObject = new GameObject("PopulationManager");
            populationManagerObject.transform.SetParent(systems.transform);
            populationManagerObject.AddComponent<PopulationManager>();

            GameObject productionManagerObject = new GameObject("ProductionManager");
            productionManagerObject.transform.SetParent(systems.transform);
            productionManagerObject.AddComponent<ProductionManager>();

            GameObject barracksProductionObject = new GameObject("BarracksProductionManager");
            barracksProductionObject.transform.SetParent(systems.transform);
            barracksProductionObject.AddComponent<BarracksProductionManager>();

            GameObject archeryRangeProductionObject = new GameObject("ArcheryRangeProductionManager");
            archeryRangeProductionObject.transform.SetParent(systems.transform);
            archeryRangeProductionObject.AddComponent<ArcheryRangeProductionManager>();

            GameObject stableProductionObject = new GameObject("StableProductionManager");
            stableProductionObject.transform.SetParent(systems.transform);
            stableProductionObject.AddComponent<StableProductionManager>();

            GameObject blacksmithResearchObject = new GameObject("BlacksmithResearchManager");
            blacksmithResearchObject.transform.SetParent(systems.transform);
            blacksmithResearchObject.AddComponent<BlacksmithResearchManager>();

            GameObject watchTowerDefenseObject = new GameObject("WatchTowerDefenseManager");
            watchTowerDefenseObject.transform.SetParent(systems.transform);
            watchTowerDefenseObject.AddComponent<WatchTowerDefenseManager>();

            GameObject resourceManagerObject = new GameObject("ResourceManager");
            resourceManagerObject.transform.SetParent(systems.transform);
            ResourceManager resourceManager = resourceManagerObject.AddComponent<ResourceManager>();
            SerializedObject serializedResourceManager = new SerializedObject(resourceManager);
            serializedResourceManager.FindProperty("initialPlayerFood").floatValue = 200f;
            serializedResourceManager.FindProperty("initialPlayerWood").floatValue = 200f;
            serializedResourceManager.FindProperty("initialEnemyFood").floatValue = 200f;
            serializedResourceManager.FindProperty("initialEnemyWood").floatValue = 200f;
            serializedResourceManager.ApplyModifiedPropertiesWithoutUndo();

            GameObject gatherManagerObject = new GameObject("GatherManager");
            gatherManagerObject.transform.SetParent(systems.transform);
            gatherManagerObject.AddComponent<GatherManager>();

            GameObject foodGatherManagerObject = new GameObject("FoodGatherManager");
            foodGatherManagerObject.transform.SetParent(systems.transform);
            foodGatherManagerObject.AddComponent<FoodGatherManager>();

            GameObject mineralGatherManagerObject = new GameObject("MineralGatherManager");
            mineralGatherManagerObject.transform.SetParent(systems.transform);
            mineralGatherManagerObject.AddComponent<MineralGatherManager>();

            GameObject placementManagerObject = new GameObject("BuildingPlacementManager");
            placementManagerObject.transform.SetParent(systems.transform);
            BuildingPlacementManager placementManager = placementManagerObject.AddComponent<BuildingPlacementManager>();

            GameObject cpuEconomyObject = new GameObject("CpuEconomyAiManager");
            cpuEconomyObject.transform.SetParent(systems.transform);
            CpuEconomyAiManager cpuEconomy = cpuEconomyObject.AddComponent<CpuEconomyAiManager>();

            GameObject cpuMilitaryObject = new GameObject("CpuMilitaryAiManager");
            cpuMilitaryObject.transform.SetParent(systems.transform);
            CpuMilitaryAiManager cpuMilitary = cpuMilitaryObject.AddComponent<CpuMilitaryAiManager>();

            GameObject selectionManagerObject = new GameObject("SelectionManager");
            selectionManagerObject.transform.SetParent(systems.transform);
            SelectionManager selectionManager = selectionManagerObject.AddComponent<SelectionManager>();
            selectionManagerObject.AddComponent<SelectionBoxView>();
            selectionManagerObject.AddComponent<ProductionPanelView>();
            selectionManagerObject.AddComponent<BarracksPanelView>();
            selectionManagerObject.AddComponent<ArcheryRangePanelView>();
            selectionManagerObject.AddComponent<StablePanelView>();
            selectionManagerObject.AddComponent<BlacksmithPanelView>();
            selectionManagerObject.AddComponent<MarketPanelView>();
            selectionManagerObject.AddComponent<UnitStancePanelView>();
            UnitHpBarView hpBarView = selectionManagerObject.AddComponent<UnitHpBarView>();
            SelectionInfoPanelView infoPanelView = selectionManagerObject.AddComponent<SelectionInfoPanelView>();
            ResourceHudView resourceHud = selectionManagerObject.AddComponent<ResourceHudView>();
            IdleUnitSelectionController idleSelection = selectionManagerObject.AddComponent<IdleUnitSelectionController>();
            IdleUnitHudView idleHud = selectionManagerObject.AddComponent<IdleUnitHudView>();
            ControlGroupManager controlGroupManager = selectionManagerObject.AddComponent<ControlGroupManager>();
            ControlGroupInputController controlGroupInput = selectionManagerObject.AddComponent<ControlGroupInputController>();
            selectionManagerObject.AddComponent<CpuHudView>();
            selectionManagerObject.AddComponent<GameTimeHudView>();
            selectionManagerObject.AddComponent<VictoryDefeatHudView>();

            RTSInputReader inputReader = mainCamera.GetComponent<RTSInputReader>();
            SerializedObject serializedSelection = new SerializedObject(selectionManager);
            serializedSelection.FindProperty("mainCamera").objectReferenceValue = mainCamera;
            serializedSelection.FindProperty("input").objectReferenceValue = inputReader;
            SerializedProperty boxView = serializedSelection.FindProperty("selectionBoxView");
            boxView.objectReferenceValue = selectionManagerObject.GetComponent<SelectionBoxView>();
            serializedSelection.ApplyModifiedPropertiesWithoutUndo();

            ProductionPanelView productionPanel = selectionManagerObject.GetComponent<ProductionPanelView>();
            SerializedObject serializedProductionPanel = new SerializedObject(productionPanel);
            serializedProductionPanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedProductionPanel.FindProperty("input").objectReferenceValue = inputReader;
            serializedProductionPanel.FindProperty("feudalAgeData").objectReferenceValue = feudalAgeData;
            serializedProductionPanel.ApplyModifiedPropertiesWithoutUndo();

            BarracksPanelView barracksPanel = selectionManagerObject.GetComponent<BarracksPanelView>();
            SerializedObject serializedBarracksPanel = new SerializedObject(barracksPanel);
            serializedBarracksPanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedBarracksPanel.FindProperty("input").objectReferenceValue = inputReader;
            serializedBarracksPanel.ApplyModifiedPropertiesWithoutUndo();

            ArcheryRangePanelView archeryRangePanel = selectionManagerObject.GetComponent<ArcheryRangePanelView>();
            SerializedObject serializedArcheryRangePanel = new SerializedObject(archeryRangePanel);
            serializedArcheryRangePanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedArcheryRangePanel.FindProperty("input").objectReferenceValue = inputReader;
            serializedArcheryRangePanel.ApplyModifiedPropertiesWithoutUndo();

            StablePanelView stablePanel = selectionManagerObject.GetComponent<StablePanelView>();
            SerializedObject serializedStablePanel = new SerializedObject(stablePanel);
            serializedStablePanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedStablePanel.FindProperty("input").objectReferenceValue = inputReader;
            serializedStablePanel.ApplyModifiedPropertiesWithoutUndo();

            BlacksmithPanelView blacksmithPanel = selectionManagerObject.GetComponent<BlacksmithPanelView>();
            SerializedObject serializedBlacksmithPanel = new SerializedObject(blacksmithPanel);
            serializedBlacksmithPanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedBlacksmithPanel.FindProperty("input").objectReferenceValue = inputReader;
            serializedBlacksmithPanel.FindProperty("infantryUpgradeTech").objectReferenceValue = infantryUpgradeTech;
            serializedBlacksmithPanel.ApplyModifiedPropertiesWithoutUndo();

            MarketPanelView marketPanel = selectionManagerObject.GetComponent<MarketPanelView>();
            SerializedObject serializedMarketPanel = new SerializedObject(marketPanel);
            serializedMarketPanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedMarketPanel.FindProperty("tradeRates").objectReferenceValue = marketTradeData;
            serializedMarketPanel.ApplyModifiedPropertiesWithoutUndo();

            UnitStancePanelView stancePanel = selectionManagerObject.GetComponent<UnitStancePanelView>();
            SerializedObject serializedStancePanel = new SerializedObject(stancePanel);
            serializedStancePanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedStancePanel.ApplyModifiedPropertiesWithoutUndo();

            RTSCameraController cameraController = mainCamera.GetComponent<RTSCameraController>();
            SerializedObject serializedIdleSelection = new SerializedObject(idleSelection);
            serializedIdleSelection.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedIdleSelection.FindProperty("input").objectReferenceValue = inputReader;
            serializedIdleSelection.FindProperty("cameraController").objectReferenceValue = cameraController;
            serializedIdleSelection.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedIdleHud = new SerializedObject(idleHud);
            serializedIdleHud.FindProperty("idleSelectionController").objectReferenceValue = idleSelection;
            serializedIdleHud.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedControlGroup = new SerializedObject(controlGroupManager);
            serializedControlGroup.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedControlGroup.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedControlGroupInput = new SerializedObject(controlGroupInput);
            serializedControlGroupInput.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedControlGroupInput.FindProperty("controlGroupManager").objectReferenceValue = controlGroupManager;
            serializedControlGroupInput.FindProperty("input").objectReferenceValue = inputReader;
            serializedControlGroupInput.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedHpBar = new SerializedObject(hpBarView);
            serializedHpBar.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedHpBar.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedInfoPanel = new SerializedObject(infoPanelView);
            serializedInfoPanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedInfoPanel.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedPlacement = new SerializedObject(placementManager);
            serializedPlacement.FindProperty("mainCamera").objectReferenceValue = mainCamera;
            serializedPlacement.FindProperty("input").objectReferenceValue = inputReader;
            serializedPlacement.FindProperty("houseData").objectReferenceValue = houseData;
            serializedPlacement.FindProperty("barracksData").objectReferenceValue = barracksData;
            serializedPlacement.FindProperty("archeryRangeData").objectReferenceValue = archeryRangeData;
            serializedPlacement.FindProperty("stableData").objectReferenceValue = stableData;
            serializedPlacement.FindProperty("blacksmithData").objectReferenceValue = blacksmithData;
            serializedPlacement.FindProperty("palisadeWallData").objectReferenceValue = palisadeWallData;
            serializedPlacement.FindProperty("stoneWallData").objectReferenceValue = stoneWallData;
            serializedPlacement.FindProperty("watchTowerData").objectReferenceValue = watchTowerData;
            serializedPlacement.FindProperty("marketData").objectReferenceValue = marketData;
            serializedPlacement.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedResourceHud = new SerializedObject(resourceHud);
            serializedResourceHud.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedResourceHud.FindProperty("houseData").objectReferenceValue = houseData;
            serializedResourceHud.FindProperty("barracksData").objectReferenceValue = barracksData;
            serializedResourceHud.FindProperty("archeryRangeData").objectReferenceValue = archeryRangeData;
            serializedResourceHud.FindProperty("stableData").objectReferenceValue = stableData;
            serializedResourceHud.FindProperty("blacksmithData").objectReferenceValue = blacksmithData;
            serializedResourceHud.FindProperty("palisadeWallData").objectReferenceValue = palisadeWallData;
            serializedResourceHud.FindProperty("stoneWallData").objectReferenceValue = stoneWallData;
            serializedResourceHud.FindProperty("watchTowerData").objectReferenceValue = watchTowerData;
            serializedResourceHud.FindProperty("marketData").objectReferenceValue = marketData;
            serializedResourceHud.FindProperty("millData").objectReferenceValue = millData;
            serializedResourceHud.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedCpuEconomy = new SerializedObject(cpuEconomy);
            serializedCpuEconomy.FindProperty("houseData").objectReferenceValue = houseData;
            serializedCpuEconomy.FindProperty("millData").objectReferenceValue = millData;
            PlacedBuildingData miningCampData = Phase1SceneBuilder.EnsureMiningCampData();
            PlacedBuildingData farmData = Phase1SceneBuilder.EnsureFarmData();
            serializedCpuEconomy.FindProperty("miningCampData").objectReferenceValue = miningCampData;
            serializedCpuEconomy.FindProperty("farmData").objectReferenceValue = farmData;
            serializedCpuEconomy.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedCpuMilitary = new SerializedObject(cpuMilitary);
            serializedCpuMilitary.FindProperty("barracksData").objectReferenceValue = barracksData;
            serializedCpuMilitary.FindProperty("archeryRangeData").objectReferenceValue = archeryRangeData;
            serializedCpuMilitary.FindProperty("stableData").objectReferenceValue = stableData;
            serializedCpuMilitary.FindProperty("barracksBuildDelaySeconds").floatValue = DefaultBarracksBuildDelaySeconds;
            serializedCpuMilitary.FindProperty("attackWaveIntervalSeconds").floatValue = DefaultAttackWaveIntervalSeconds;
            serializedCpuMilitary.FindProperty("relaxedFirstAttackGraceSeconds").floatValue = DefaultRelaxedFirstAttackGraceSeconds;
            serializedCpuMilitary.FindProperty("relaxedBarracksBuildDelaySeconds").floatValue = DefaultRelaxedBarracksBuildDelaySeconds;
            serializedCpuMilitary.FindProperty("relaxedAttackWaveIntervalSeconds").floatValue = DefaultRelaxedAttackWaveIntervalSeconds;
            serializedCpuMilitary.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(resourceHud);
            EditorUtility.SetDirty(placementManager);
            EditorUtility.SetDirty(cpuEconomy);
            EditorUtility.SetDirty(cpuMilitary);
        }
    }
}
