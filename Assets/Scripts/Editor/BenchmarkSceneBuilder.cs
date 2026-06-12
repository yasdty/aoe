using AoE.RTS.Benchmark;
using AoE.RTS.Combat;
using AoE.RTS.Core;
using AoE.RTS.Spatial;
using AoE.RTS.Units;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AoE.RTS.EditorTools
{
    public static class BenchmarkSceneBuilder
    {
        const string ScenePath = "Assets/Scenes/Benchmark.unity";

        [MenuItem("AoE/Setup Benchmark Scene", true)]
        static bool ValidateSetupBenchmarkScene() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Setup Benchmark Scene")]
        public static void SetupBenchmarkScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            Phase1SceneBuilder.EnsureLayers();
            UnitData villagerData = Phase1SceneBuilder.EnsureDefaultUnitData();
            UnitData militiaData = Phase1SceneBuilder.EnsureMilitiaData();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Phase1SceneBuilder.CreateLighting();
            Phase1SceneBuilder.CreateGround();
            GameObject cameraObject = CreateBenchmarkCamera();

            GameObject systems = new GameObject("Systems");

            GameObject simulationTickObject = new GameObject("SimulationTick");
            simulationTickObject.transform.SetParent(systems.transform);
            simulationTickObject.AddComponent<SimulationTick>();

            GameObject unitManagerObject = new GameObject("UnitManager");
            unitManagerObject.transform.SetParent(systems.transform);
            unitManagerObject.AddComponent<UnitManager>();

            GameObject unitSpatialIndexObject = new GameObject("UnitSpatialIndex");
            unitSpatialIndexObject.transform.SetParent(systems.transform);
            unitSpatialIndexObject.AddComponent<UnitSpatialIndex>();

            GameObject unitPoolObject = new GameObject("UnitPool");
            unitPoolObject.transform.SetParent(systems.transform);
            UnitPool unitPool = unitPoolObject.AddComponent<UnitPool>();
            SerializedObject serializedUnitPool = new SerializedObject(unitPool);
            serializedUnitPool.FindProperty("prewarmVillagers").intValue = 0;
            serializedUnitPool.FindProperty("prewarmMilitia").intValue = 0;
            serializedUnitPool.FindProperty("prewarmVillagerData").objectReferenceValue = villagerData;
            serializedUnitPool.FindProperty("prewarmMilitiaData").objectReferenceValue = militiaData;
            serializedUnitPool.ApplyModifiedPropertiesWithoutUndo();

            GameObject attackManagerObject = new GameObject("AttackManager");
            attackManagerObject.transform.SetParent(systems.transform);
            attackManagerObject.AddComponent<AttackManager>();

            GameObject benchmarkObject = new GameObject("Benchmark");
            BenchmarkSpawner spawner = benchmarkObject.AddComponent<BenchmarkSpawner>();
            BenchmarkMetricsView metricsView = benchmarkObject.AddComponent<BenchmarkMetricsView>();

            SerializedObject serializedSpawner = new SerializedObject(spawner);
            serializedSpawner.FindProperty("villagerData").objectReferenceValue = villagerData;
            serializedSpawner.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedMetrics = new SerializedObject(metricsView);
            serializedMetrics.FindProperty("spawner").objectReferenceValue = spawner;
            serializedMetrics.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            UnityEditor.Selection.activeGameObject = cameraObject;

            Debug.Log("Benchmark scene created at " + ScenePath);
        }

        static GameObject CreateBenchmarkCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<UnityEngine.Camera>();
            cameraObject.AddComponent<AudioListener>();
            Phase1SceneBuilder.ApplyOverviewCamera(cameraObject.transform, Vector3.zero);
            return cameraObject;
        }
    }
}
