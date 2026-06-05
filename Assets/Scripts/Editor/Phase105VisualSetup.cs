using System.Linq;
using AoE.RTS.Visuals;
using UnityEditor;
using UnityEngine;

namespace AoE.RTS.EditorTools
{
    public static class Phase105VisualSetup
    {
        [MenuItem("AoE/Setup Phase10.5 Visual Prefabs", true)]
        static bool ValidateSetup() => Phase1SceneBuilder.EnsureEditModeForSceneSetup();

        [MenuItem("AoE/Setup Phase10.5 Visual Prefabs")]
        public static void SetupVisualPrefabs()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            EnsureFolders();

            if (!TryCreatePrefabsFromGlb(out string glbFailureReason))
            {
                if (IsGltfImporterAvailable())
                    Debug.LogWarning("Phase10.5: GLB import failed — " + glbFailureReason);
                else
                    Debug.Log("Phase10.5: glTFast not loaded yet — creating prefabs from Editor meshes.");

                PlaceholderVisualMeshFactory.CreateAllPrefabs();
            }

            if (!ValidatePrefabsExist())
            {
                Debug.LogError("Phase10.5 visual prefab setup failed.");
                return;
            }

            CreateCatalogAsset();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Phase10.5 visual prefabs ready.");
        }

        [MenuItem("AoE/Setup Phase10.5 Scene", true)]
        static bool ValidateSetupScene() => Phase1SceneBuilder.EnsureEditModeForSceneSetup();

        [MenuItem("AoE/Setup Phase10.5 Scene")]
        public static void SetupPhase105Scene()
        {
            SetupVisualPrefabs();
            if (!ValidatePrefabsExist())
                return;

            Phase10SceneBuilder.SetupPhase10Scene();
        }

        static void EnsureFolders()
        {
            EnsureFolder("Assets/Models");
            EnsureFolder("Assets/Models/Placeholder");
            EnsureFolder("Assets/Resources");
            EnsureFolder(PlaceholderVisualPaths.GlbFolder);
            EnsureFolder(PlaceholderVisualPaths.ResourcesFolder);
        }

        static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
                string name = System.IO.Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        static bool IsGltfImporterAvailable()
        {
            return System.AppDomain.CurrentDomain.GetAssemblies().Any(
                assembly =>
                {
                    string name = assembly.GetName().Name;
                    return name == "glTFast" || name == "glTFast.Editor";
                });
        }

        static bool TryCreatePrefabsFromGlb(out string failureReason)
        {
            failureReason = null;

            if (!IsGltfImporterAvailable())
            {
                failureReason = "install com.unity.cloud.gltfast and wait for Package Manager to finish.";
                return false;
            }

            (string glbPath, string prefabPath)[] assets =
            {
                (PlaceholderVisualPaths.VillagerGlb, PlaceholderVisualPaths.VillagerPrefabAsset),
                (PlaceholderVisualPaths.MilitiaGlb, PlaceholderVisualPaths.MilitiaPrefabAsset),
                (PlaceholderVisualPaths.TownCenterGlb, PlaceholderVisualPaths.TownCenterPrefabAsset),
                (PlaceholderVisualPaths.HouseGlb, PlaceholderVisualPaths.HousePrefabAsset),
                (PlaceholderVisualPaths.BarracksGlb, PlaceholderVisualPaths.BarracksPrefabAsset),
                (PlaceholderVisualPaths.TreeGlb, PlaceholderVisualPaths.TreePrefabAsset)
            };

            for (int i = 0; i < assets.Length; i++)
            {
                if (!System.IO.File.Exists(assets[i].glbPath))
                {
                    failureReason = "missing " + assets[i].glbPath + " (run: py Tools/generate_placeholder_glbs.py)";
                    return false;
                }
            }

            for (int i = 0; i < assets.Length; i++)
                AssetDatabase.ImportAsset(assets[i].glbPath, ImportAssetOptions.ForceUpdate);

            AssetDatabase.Refresh();

            for (int i = 0; i < assets.Length; i++)
            {
                if (!SaveVisualPrefab(assets[i].glbPath, assets[i].prefabPath, out failureReason))
                    return false;
            }

            return true;
        }

        static bool SaveVisualPrefab(string glbPath, string prefabPath, out string failureReason)
        {
            failureReason = null;
            GameObject source = LoadImportedGlbRoot(glbPath);
            if (source == null)
            {
                failureReason = "could not load " + glbPath + " as GameObject after import.";
                return false;
            }

            GameObject instance = Object.Instantiate(source);
            instance.name = System.IO.Path.GetFileNameWithoutExtension(prefabPath);
            StripColliders(instance);
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);
            return true;
        }

        static GameObject LoadImportedGlbRoot(string glbPath)
        {
            GameObject direct = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);
            if (direct != null)
                return direct;

            Object main = AssetDatabase.LoadMainAssetAtPath(glbPath);
            if (main is GameObject mainObject)
                return mainObject;

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(glbPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is GameObject gameObject)
                    return gameObject;
            }

            return null;
        }

        static void StripColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
                Object.DestroyImmediate(colliders[i]);
        }

        static bool ValidatePrefabsExist()
        {
            string[] prefabPaths =
            {
                PlaceholderVisualPaths.VillagerPrefabAsset,
                PlaceholderVisualPaths.MilitiaPrefabAsset,
                PlaceholderVisualPaths.TownCenterPrefabAsset,
                PlaceholderVisualPaths.HousePrefabAsset,
                PlaceholderVisualPaths.BarracksPrefabAsset,
                PlaceholderVisualPaths.TreePrefabAsset
            };

            for (int i = 0; i < prefabPaths.Length; i++)
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPaths[i]) == null)
                {
                    Debug.LogError("Missing visual prefab: " + prefabPaths[i]);
                    return false;
                }
            }

            return true;
        }

        static void CreateCatalogAsset()
        {
            PlaceholderVisualCatalog catalog = AssetDatabase.LoadAssetAtPath<PlaceholderVisualCatalog>(
                PlaceholderVisualPaths.CatalogAsset);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<PlaceholderVisualCatalog>();
                AssetDatabase.CreateAsset(catalog, PlaceholderVisualPaths.CatalogAsset);
            }

            SerializedObject serialized = new SerializedObject(catalog);
            serialized.FindProperty("villagerPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>(PlaceholderVisualPaths.VillagerPrefabAsset);
            serialized.FindProperty("militiaPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>(PlaceholderVisualPaths.MilitiaPrefabAsset);
            serialized.FindProperty("townCenterPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>(PlaceholderVisualPaths.TownCenterPrefabAsset);
            serialized.FindProperty("housePrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>(PlaceholderVisualPaths.HousePrefabAsset);
            serialized.FindProperty("barracksPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>(PlaceholderVisualPaths.BarracksPrefabAsset);
            serialized.FindProperty("treePrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>(PlaceholderVisualPaths.TreePrefabAsset);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
        }
    }
}
