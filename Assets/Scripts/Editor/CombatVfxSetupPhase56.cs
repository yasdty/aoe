using AoE.RTS.View;
using UnityEditor;
using UnityEngine;

namespace AoE.RTS.EditorTools
{
    public static class CombatVfxSetupPhase56
    {
        [MenuItem("AoE/Setup Combat VFX (Phase56)", true)]
        static bool ValidateSetupCombatVfxPhase56() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Setup Combat VFX (Phase56)")]
        public static void SetupCombatVfxPhase56()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            EnsureResourceFolders();
            CreateProjectileMaterial();
            CreateHitBurstPrefab();
            CreateDeathPuffPrefab();
            CreateAudioClip(CombatFeedbackPaths.MeleeHitAudioResource, 220f, 0.08f, 0.35f);
            CreateAudioClip(CombatFeedbackPaths.RangedHitAudioResource, 440f, 0.06f, 0.3f);
            CreateAudioClip(CombatFeedbackPaths.UnitDeathAudioResource, 120f, 0.18f, 0.45f);
            Phase10SceneBuilder.EnsureCombatFeedbackView();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Combat VFX setup complete (Phase56). Save scene if prompted.");
        }

        static void EnsureResourceFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder(CombatFeedbackPaths.VfxResourceFolder))
                AssetDatabase.CreateFolder("Assets/Resources", "CombatVfx");
            if (!AssetDatabase.IsValidFolder(CombatFeedbackPaths.AudioResourceFolder))
                AssetDatabase.CreateFolder("Assets/Resources", "CombatAudio");
        }

        static void CreateProjectileMaterial()
        {
            string path = $"{CombatFeedbackPaths.VfxResourceFolder}/ProjectileMat.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = new Color(0.95f, 0.75f, 0.2f, 1f);
            EditorUtility.SetDirty(material);
        }

        static void CreateHitBurstPrefab()
        {
            SaveParticlePrefab(
                $"{CombatFeedbackPaths.VfxResourceFolder}/HitBurst.prefab",
                "HitBurst",
                new Color(1f, 0.55f, 0.2f, 1f),
                16,
                0.25f,
                2.5f,
                0.35f);
        }

        static void CreateDeathPuffPrefab()
        {
            SaveParticlePrefab(
                $"{CombatFeedbackPaths.VfxResourceFolder}/DeathPuff.prefab",
                "DeathPuff",
                new Color(0.65f, 0.65f, 0.65f, 1f),
                24,
                0.35f,
                1.8f,
                0.55f);
        }

        static void SaveParticlePrefab(
            string assetPath,
            string objectName,
            Color startColor,
            int maxParticles,
            float lifetime,
            float startSpeed,
            float startSize)
        {
            GameObject root = new GameObject(objectName);
            ParticleSystem particleSystem = root.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particleSystem.main;
            main.duration = lifetime;
            main.loop = false;
            main.startLifetime = lifetime;
            main.startSpeed = startSpeed;
            main.startSize = startSize;
            main.startColor = startColor;
            main.maxParticles = maxParticles;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;
            main.stopAction = ParticleSystemStopAction.Destroy;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)maxParticles) });

            ParticleSystemRenderer renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateParticleMaterial(startColor);

            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (existing == null)
                PrefabUtility.SaveAsPrefabAsset(root, assetPath);
            else
                PrefabUtility.SaveAsPrefabAssetAndConnect(root, assetPath, InteractionMode.AutomatedAction);

            Object.DestroyImmediate(root);
        }

        static Material CreateParticleMaterial(Color color)
        {
            Material material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            material.color = color;
            return material;
        }

        static void CreateAudioClip(string resourcePath, float frequency, float duration, float volume)
        {
            string assetPath = $"Assets/Resources/{resourcePath}.asset";
            const int sampleRate = 44100;
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = 1f - (t / duration);
                samples[i] = Mathf.Sin(Mathf.PI * 2f * frequency * t) * envelope * volume;
            }

            AudioClip existing = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            AudioClip clip = existing != null
                ? existing
                : AudioClip.Create(System.IO.Path.GetFileNameWithoutExtension(assetPath), sampleCount, 1, sampleRate, false);

            clip.SetData(samples, 0);
            if (existing == null)
                AssetDatabase.CreateAsset(clip, assetPath);
            else
                EditorUtility.SetDirty(clip);
        }
    }
}
