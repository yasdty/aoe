using AoE.RTS.Buildings;
using AoE.RTS.Camera;
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
        const string DefaultHouseDataPath = GameAssetPaths.DefaultHouseData;
        const string DefaultBarracksDataPath = GameAssetPaths.DefaultBarracksData;
        const string MilitiaDataPath = GameAssetPaths.MilitiaData;
        const string EnemyDummyDataPath = GameAssetPaths.EnemyDummyData;

        public static bool EnsureEditModeForSceneSetup()
        {
            if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
                return true;

            Debug.LogWarning("Stop Play mode before running AoE scene setup menus.");
            return false;
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
                return existing;

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

                if (existing.villagerTrainTime != 3f)
                {
                    existing.villagerTrainTime = 3f;
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
            data.villagerTrainTime = 3f;
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
                if (existing.buildTime != 3f)
                {
                    existing.buildTime = 3f;
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
            data.woodCost = 25f;
            data.buildTime = 3f;
            data.housingProvided = 5;
            data.maxHp = 150f;
            AssetDatabase.CreateAsset(data, DefaultHouseDataPath);
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

        public static PlacedBuildingData EnsureBarracksData(UnitData militiaData)
        {
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
            data.woodCost = 50f;
            data.buildTime = 5f;
            data.footprintWidth = 6f;
            data.footprintDepth = 6f;
            data.buildingHeight = 3.5f;
            data.housingProvided = 0;
            data.trainUnitData = militiaData;
            data.trainTime = 3f;
            data.trainWoodCost = 20f;
            data.spawnClearance = 4f;
            data.defaultColor = new Color(0.55f, 0.35f, 0.32f);
            data.selectedColor = new Color(0.95f, 0.55f, 0.35f);
            data.maxHp = 200f;
            AssetDatabase.CreateAsset(data, DefaultBarracksDataPath);
            AssetDatabase.SaveAssets();
            return data;
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
            UnityEngine.Camera camera = cameraObject.AddComponent<UnityEngine.Camera>();
            cameraObject.AddComponent<AudioListener>();

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
            if (data.armor != 0f) { data.armor = 0f; dirty = true; }
            if (data.attackRange != 2f) { data.attackRange = 2f; dirty = true; }
            if (data.attackCooldown != 1f) { data.attackCooldown = 1f; dirty = true; }
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
            if (data.team != UnitTeam.Enemy) { data.team = UnitTeam.Enemy; dirty = true; }
            return dirty;
        }
    }
}
