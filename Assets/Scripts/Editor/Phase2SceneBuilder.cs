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
    public static class Phase2SceneBuilder
    {
        const string ScenePath = "Assets/Scenes/Phase2.unity";

        [MenuItem("AoE/Setup Phase2 Scene")]
        public static void SetupPhase2Scene()
        {
            Phase1SceneBuilder.EnsureLayers();
            UnitData unitData = Phase1SceneBuilder.EnsureDefaultUnitData();
            InputActionAsset inputActions = RTSInputActionsFactory.EnsureAsset();
            if (inputActions == null)
            {
                Debug.LogError("Failed to create RTSInputActions. Setup aborted.");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Phase1SceneBuilder.CreateLighting();
            Phase1SceneBuilder.CreateGround();
            CreateUnitGrid(unitData);
            GameObject cameraRig = Phase1SceneBuilder.CreateCameraRig(inputActions);
            CreateManagers(inputActions, cameraRig.GetComponent<UnityEngine.Camera>());

            Phase1SceneBuilder.AssignInputActionsToReaders(inputActions);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("Phase2 scene created at " + ScenePath);
        }

        static void CreateUnitGrid(UnitData unitData)
        {
            const int gridSize = 3;
            const float spacing = 3f;
            float origin = -(gridSize - 1) * spacing * 0.5f;

            for (int row = 0; row < gridSize; row++)
            {
                for (int column = 0; column < gridSize; column++)
                {
                    Vector3 position = new Vector3(origin + column * spacing, 1f, origin + row * spacing);
                    GameObject unitObject = Phase1SceneBuilder.CreateUnit(unitData, position);
                    unitObject.name = $"Unit_{row}_{column}";
                }
            }
        }

        static void CreateManagers(InputActionAsset inputActions, UnityEngine.Camera mainCamera)
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
    }
}
