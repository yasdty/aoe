using AoE.RTS.Camera;
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
    public static class Phase1SceneBuilder
    {
        const string ScenePath = "Assets/Scenes/Phase1.unity";
        const string UnitDataPath = "Assets/Data/UnitData/DefaultUnit.asset";
        [MenuItem("AoE/Fix Phase1 Input References")]
        public static void FixPhase1InputReferences()
        {
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

        [MenuItem("AoE/Setup Phase1 Scene")]
        public static void SetupPhase1Scene()
        {
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
            SerializedObject tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            SetLayerName(layers, 8, "Ground");
            SetLayerName(layers, 9, "Unit");
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
                return existing;

            UnitData data = ScriptableObject.CreateInstance<UnitData>();
            data.displayName = "Villager";
            AssetDatabase.CreateAsset(data, UnitDataPath);
            AssetDatabase.SaveAssets();
            return data;
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
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.35f, 0.55f, 0.3f);
            renderer.sharedMaterial = material;

            return ground;
        }

        public static GameObject CreateUnit(UnitData unitData, Vector3 position)
        {
            GameObject unitObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            unitObject.name = "Unit";
            unitObject.layer = LayerMask.NameToLayer("Unit");
            unitObject.transform.position = position;

            Unit unit = unitObject.AddComponent<Unit>();
            SerializedObject serializedUnit = new SerializedObject(unit);
            serializedUnit.FindProperty("data").objectReferenceValue = unitData;
            serializedUnit.ApplyModifiedPropertiesWithoutUndo();

            return unitObject;
        }

        public static GameObject CreateCameraRig(InputActionAsset inputActions)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            UnityEngine.Camera camera = cameraObject.AddComponent<UnityEngine.Camera>();
            cameraObject.AddComponent<AudioListener>();

            cameraObject.transform.position = new Vector3(50f, 45f, 50f);
            cameraObject.transform.rotation = Quaternion.Euler(55f, -45f, 0f);

            RTSInputReader inputReader = cameraObject.AddComponent<RTSInputReader>();
            SerializedObject serializedInput = new SerializedObject(inputReader);
            serializedInput.FindProperty("inputActions").objectReferenceValue = inputActions;
            serializedInput.ApplyModifiedPropertiesWithoutUndo();

            RTSCameraController cameraController = cameraObject.AddComponent<RTSCameraController>();
            SerializedObject serializedCamera = new SerializedObject(cameraController);
            serializedCamera.FindProperty("input").objectReferenceValue = inputReader;
            serializedCamera.ApplyModifiedPropertiesWithoutUndo();

            return cameraObject;
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
    }
}
