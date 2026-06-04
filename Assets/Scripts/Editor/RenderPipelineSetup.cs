using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AoE.RTS.EditorTools
{
    public static class RenderPipelineSetup
    {
        const string PipelinePath = "Assets/Settings/URP_Pipeline.asset";
        const string RendererPath = "Assets/Settings/URP_ForwardRenderer.asset";

        [MenuItem("AoE/Fix Render Pipeline", true)]
        static bool ValidateFixRenderPipeline() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Fix Render Pipeline")]
        public static void FixRenderPipelineMenu()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            EnsureRenderPipeline(force: true);
        }

        public static void EnsureRenderPipeline(bool force = false)
        {
            UniversalRenderPipelineAsset pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
                pipeline = CreatePipelineAsset();

            if (force || GraphicsSettings.defaultRenderPipeline == null)
                AssignPipeline(pipeline);
        }

        static UniversalRenderPipelineAsset CreatePipelineAsset()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Settings"))
                AssetDatabase.CreateFolder("Assets", "Settings");

            UniversalRendererData rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(rendererData, RendererPath);

            UniversalRenderPipelineAsset pipeline = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
            AssetDatabase.CreateAsset(pipeline, PipelinePath);

            SerializedObject serializedPipeline = new SerializedObject(pipeline);
            SerializedProperty rendererList = serializedPipeline.FindProperty("m_RendererDataList");
            rendererList.arraySize = 1;
            rendererList.GetArrayElementAtIndex(0).objectReferenceValue = rendererData;
            serializedPipeline.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.SaveAssets();
            return pipeline;
        }

        static void AssignPipeline(UniversalRenderPipelineAsset pipeline)
        {
            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            EditorUtility.SetDirty(GraphicsSettings.GetGraphicsSettings());
        }
    }
}
