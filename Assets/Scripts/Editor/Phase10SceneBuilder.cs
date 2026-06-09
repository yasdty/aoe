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

        static readonly Vector3 PlayerTownCenterPosition = Vector3.zero;
        static readonly Vector3 CpuTownCenterPosition = new Vector3(0f, 0f, -35f);
        static readonly Vector3 CameraFocus = new Vector3(0f, 0f, -17f);

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
            new Vector3(0f, 0f, -14f),
            new Vector3(10f, 0f, -22f),
            new Vector3(-12f, 0f, -20f),
            new Vector3(6f, 0f, -28f),
            new Vector3(-8f, 0f, -30f),
            new Vector3(0f, 0f, -32f),
            new Vector3(14f, 0f, -34f),
            new Vector3(-14f, 0f, -36f),
            new Vector3(8f, 0f, -40f),
            new Vector3(-6f, 0f, -42f),
            new Vector3(0f, 0f, -24f),
            new Vector3(-20f, 0f, -28f),
            new Vector3(20f, 0f, -26f)
        };

        static readonly Vector3[] CpuVillagerPositions =
        {
            new Vector3(-5f, 1f, -30f),
            new Vector3(5f, 1f, -30f),
            new Vector3(0f, 1f, -40f)
        };

        static readonly Vector3[] PlayerBerryBushPositions =
        {
            new Vector3(10f, 0f, 6f),
            new Vector3(-8f, 0f, 8f),
            new Vector3(6f, 0f, -6f)
        };

        static readonly Vector3[] CpuBerryBushPositions =
        {
            new Vector3(-6f, 0f, -28f),
            new Vector3(6f, 0f, -32f),
            new Vector3(0f, 0f, -38f)
        };

        static readonly Vector3[] PlayerDeerPositions =
        {
            new Vector3(7f, 0f, 8f),
            new Vector3(-6f, 0f, 7f),
            new Vector3(5f, 0f, 5f)
        };

        static readonly Vector3[] PlayerSheepPositions =
        {
            new Vector3(-2f, 0f, 10f),
            new Vector3(9f, 0f, 3f)
        };

        static readonly Vector3[] PlayerBoarPositions =
        {
            new Vector3(4f, 0f, 11f),
            new Vector3(-4f, 0f, 9f)
        };

        static readonly Vector3[] CpuDeerPositions =
        {
            new Vector3(-4f, 0f, -26f),
            new Vector3(4f, 0f, -34f)
        };

        static readonly Vector3[] CpuSheepPositions =
        {
            new Vector3(0f, 0f, -42f)
        };

        static readonly Vector3[] PlayerGoldMinePositions =
        {
            new Vector3(16f, 0f, 12f)
        };

        static readonly Vector3[] PlayerStoneMinePositions =
        {
            new Vector3(-16f, 0f, 12f)
        };

        static readonly Vector3[] CpuGoldMinePositions =
        {
            new Vector3(8f, 0f, -30f)
        };

        static readonly Vector3[] CpuStoneMinePositions =
        {
            new Vector3(-8f, 0f, -32f)
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
            BuildingData townCenterData = Phase1SceneBuilder.EnsureTownCenterData(villagerData);
            ResourceNodeData treeData = Phase1SceneBuilder.EnsureDefaultTreeData();
            FoodNodeData berryBushData = Phase1SceneBuilder.EnsureDefaultBerryBushData();
            FoodNodeData deerData = Phase1SceneBuilder.EnsureDefaultDeerData();
            FoodNodeData sheepData = Phase1SceneBuilder.EnsureDefaultSheepData();
            FoodNodeData boarData = Phase1SceneBuilder.EnsureDefaultBoarData();
            MineralNodeData goldMineData = Phase1SceneBuilder.EnsureDefaultGoldMineData();
            MineralNodeData stoneMineData = Phase1SceneBuilder.EnsureDefaultStoneMineData();
            PlacedBuildingData houseData = Phase1SceneBuilder.EnsureHouseData();
            PlacedBuildingData barracksData = Phase1SceneBuilder.EnsureBarracksData(militiaData);
            PlacedBuildingData millData = Phase1SceneBuilder.EnsureMillData();
            Phase1SceneBuilder.EnsureFarmData();
            Phase1SceneBuilder.EnsureLumberCampData();
            Phase1SceneBuilder.EnsureMiningCampData();
            Phase1SceneBuilder.EnsureMillData();
            InputActionAsset inputActions = RTSInputActionsFactory.EnsureAsset();
            if (inputActions == null)
            {
                Debug.LogError("Failed to create RTSInputActions. Setup aborted.");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Phase1SceneBuilder.CreateLighting();
            Phase1SceneBuilder.CreateGround();
            GameObject playerTownCenter = Phase1SceneBuilder.CreateTownCenter(townCenterData, PlayerTownCenterPosition);
            GameObject cpuTownCenter = CreateCpuTownCenter(townCenterData);
            CreateTrees(treeData);
            CreateBerryBushes(berryBushData);
            CreateHuntableAnimals(deerData, sheepData);
            CreateBoars(boarData);
            CreateMineralMines(goldMineData, stoneMineData);
            CreateCpuVillagers(villagerData);
            GameObject cameraRig = Phase1SceneBuilder.CreateCameraRig(inputActions);
            Phase1SceneBuilder.ApplyOverviewCamera(cameraRig.transform, CameraFocus);
            CreateManagers(inputActions, cameraRig.GetComponent<UnityEngine.Camera>(), houseData, barracksData, millData, villagerData, militiaData);

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

        static void CreateCpuVillagers(UnitData villagerData)
        {
            for (int i = 0; i < CpuVillagerPositions.Length; i++)
            {
                GameObject unitObject = Phase1SceneBuilder.CreateUnit(
                    villagerData,
                    CpuVillagerPositions[i],
                    UnitTeam.Enemy);
                Unit unit = unitObject.GetComponent<Unit>();
                if (unit != null)
                    unit.SetTeam(UnitTeam.Enemy);
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

        static void CreateBoars(FoodNodeData boarData)
        {
            for (int i = 0; i < PlayerBoarPositions.Length; i++)
                Phase1SceneBuilder.CreateBoar(boarData, PlayerBoarPositions[i]);
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
            PlacedBuildingData millData,
            UnitData villagerData,
            UnitData militiaData)
        {
            GameObject systems = new GameObject("Systems");
            systems.AddComponent<GameSessionManager>();

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

            GameObject resourceManagerObject = new GameObject("ResourceManager");
            resourceManagerObject.transform.SetParent(systems.transform);
            resourceManagerObject.AddComponent<ResourceManager>();

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
            serializedProductionPanel.ApplyModifiedPropertiesWithoutUndo();

            BarracksPanelView barracksPanel = selectionManagerObject.GetComponent<BarracksPanelView>();
            SerializedObject serializedBarracksPanel = new SerializedObject(barracksPanel);
            serializedBarracksPanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedBarracksPanel.FindProperty("input").objectReferenceValue = inputReader;
            serializedBarracksPanel.ApplyModifiedPropertiesWithoutUndo();

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
            serializedPlacement.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedResourceHud = new SerializedObject(resourceHud);
            serializedResourceHud.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedResourceHud.FindProperty("houseData").objectReferenceValue = houseData;
            serializedResourceHud.FindProperty("barracksData").objectReferenceValue = barracksData;
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
            serializedCpuMilitary.FindProperty("barracksBuildDelaySeconds").floatValue = DefaultBarracksBuildDelaySeconds;
            serializedCpuMilitary.FindProperty("attackWaveIntervalSeconds").floatValue = DefaultAttackWaveIntervalSeconds;
            serializedCpuMilitary.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(resourceHud);
            EditorUtility.SetDirty(placementManager);
            EditorUtility.SetDirty(cpuEconomy);
            EditorUtility.SetDirty(cpuMilitary);
        }
    }
}
