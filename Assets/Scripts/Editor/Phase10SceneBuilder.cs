using AoE.RTS.AI;
using AoE.RTS.Buildings;
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
            PlacedBuildingData houseData = Phase1SceneBuilder.EnsureHouseData();
            PlacedBuildingData barracksData = Phase1SceneBuilder.EnsureBarracksData(militiaData);
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
            CreateCpuVillagers(villagerData);
            GameObject cameraRig = Phase1SceneBuilder.CreateCameraRig(inputActions);
            Phase1SceneBuilder.ApplyOverviewCamera(cameraRig.transform, CameraFocus);
            CreateManagers(inputActions, cameraRig.GetComponent<UnityEngine.Camera>(), houseData, barracksData, villagerData, militiaData);

            Phase1SceneBuilder.AssignInputActionsToReaders(inputActions);
            EditorSceneManager.SaveScene(scene, ScenePath);
            UnityEditor.Selection.activeGameObject = playerTownCenter;

            Debug.Log("Phase10 scene created at " + ScenePath);
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
            ResourceHudView resourceHud = selectionManagerObject.AddComponent<ResourceHudView>();
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
            serializedBarracksPanel.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedHpBar = new SerializedObject(hpBarView);
            serializedHpBar.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedHpBar.ApplyModifiedPropertiesWithoutUndo();

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
            serializedResourceHud.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedCpuEconomy = new SerializedObject(cpuEconomy);
            serializedCpuEconomy.FindProperty("houseData").objectReferenceValue = houseData;
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
