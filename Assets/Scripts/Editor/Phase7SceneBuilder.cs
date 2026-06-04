using AoE.RTS.Buildings;
using AoE.RTS.Combat;
using AoE.RTS.Economy;
using AoE.RTS.Input;
using AoE.RTS.Selection;
using AoE.RTS.Units;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace AoE.RTS.EditorTools
{
    public static class Phase7SceneBuilder
    {
        const string ScenePath = "Assets/Scenes/Phase7.unity";

        static readonly Vector3[] TreePositions =
        {
            new Vector3(12f, 0f, 8f),
            new Vector3(-14f, 0f, 10f),
            new Vector3(18f, 0f, -6f),
            new Vector3(-10f, 0f, -12f),
            new Vector3(22f, 0f, 14f),
            new Vector3(-20f, 0f, -8f),
            new Vector3(8f, 0f, -18f),
            new Vector3(-16f, 0f, 18f)
        };

        static readonly Vector3[] EnemyPositions =
        {
            new Vector3(15f, 1f, 0f),
            new Vector3(18f, 1f, 4f)
        };

        [MenuItem("AoE/Setup Phase7 Scene", true)]
        static bool ValidateSetupPhase7Scene() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Setup Phase7 Scene")]
        public static void SetupPhase7Scene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            Phase1SceneBuilder.EnsureLayers();
            UnitData villagerData = Phase1SceneBuilder.EnsureDefaultUnitData();
            UnitData militiaData = Phase1SceneBuilder.EnsureMilitiaData();
            UnitData enemyData = Phase1SceneBuilder.EnsureEnemyDummyData();
            BuildingData townCenterData = Phase1SceneBuilder.EnsureTownCenterData(villagerData);
            ResourceNodeData treeData = Phase1SceneBuilder.EnsureDefaultTreeData();
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
            GameObject townCenter = Phase1SceneBuilder.CreateTownCenter(townCenterData, Vector3.zero);
            CreateTrees(treeData);
            CreateEnemyUnits(enemyData);
            GameObject cameraRig = Phase1SceneBuilder.CreateCameraRig(inputActions);
            Phase1SceneBuilder.ApplyOverviewCamera(cameraRig.transform, Vector3.zero);
            CreateManagers(inputActions, cameraRig.GetComponent<UnityEngine.Camera>(), houseData, barracksData);

            Phase1SceneBuilder.AssignInputActionsToReaders(inputActions);
            EditorSceneManager.SaveScene(scene, ScenePath);
            UnityEditor.Selection.activeGameObject = townCenter;

            Debug.Log("Phase7 scene created at " + ScenePath);
        }

        static void CreateTrees(ResourceNodeData treeData)
        {
            for (int i = 0; i < TreePositions.Length; i++)
                Phase1SceneBuilder.CreateTree(treeData, TreePositions[i]);
        }

        static void CreateEnemyUnits(UnitData enemyData)
        {
            for (int i = 0; i < EnemyPositions.Length; i++)
                Phase1SceneBuilder.CreateUnit(enemyData, EnemyPositions[i], UnitTeam.Enemy);
        }

        static void CreateManagers(
            InputActionAsset inputActions,
            UnityEngine.Camera mainCamera,
            PlacedBuildingData houseData,
            PlacedBuildingData barracksData)
        {
            GameObject systems = new GameObject("Systems");

            GameObject unitManagerObject = new GameObject("UnitManager");
            unitManagerObject.transform.SetParent(systems.transform);
            unitManagerObject.AddComponent<UnitManager>();

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

            GameObject placementManagerObject = new GameObject("BuildingPlacementManager");
            placementManagerObject.transform.SetParent(systems.transform);
            BuildingPlacementManager placementManager = placementManagerObject.AddComponent<BuildingPlacementManager>();

            GameObject selectionManagerObject = new GameObject("SelectionManager");
            selectionManagerObject.transform.SetParent(systems.transform);
            SelectionManager selectionManager = selectionManagerObject.AddComponent<SelectionManager>();
            selectionManagerObject.AddComponent<SelectionBoxView>();
            selectionManagerObject.AddComponent<ProductionPanelView>();
            selectionManagerObject.AddComponent<BarracksPanelView>();
            ResourceHudView resourceHud = selectionManagerObject.AddComponent<ResourceHudView>();

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
            EditorUtility.SetDirty(resourceHud);
            EditorUtility.SetDirty(placementManager);
        }
    }
}
