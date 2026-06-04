using UnityEngine;

namespace AoE.RTS.EditorTools
{
    public static class SceneMaterialFactory
    {
        public static Material CreateLitMaterial(Color color)
        {
            RenderPipelineSetup.EnsureRenderPipeline();

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                Debug.LogError("URP Lit shader not found. Run AoE → Fix Render Pipeline.");
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader);
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);
            else
                material.color = color;

            return material;
        }
    }
}
