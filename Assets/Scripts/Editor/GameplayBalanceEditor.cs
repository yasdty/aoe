using AoE.RTS.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AoE.RTS.EditorTools
{
    public static class GameplayBalanceEditor
    {
        [MenuItem("AoE/Balance Mode/Debug", true)]
        static bool ValidateSetDebugMode() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Balance Mode/Debug")]
        public static void SetDebugMode()
        {
            SetBalanceMode(GameplayBalanceMode.Debug);
        }

        [MenuItem("AoE/Balance Mode/AoE2", true)]
        static bool ValidateSetAoE2Mode() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Balance Mode/AoE2")]
        public static void SetAoE2Mode()
        {
            SetBalanceMode(GameplayBalanceMode.AoE2);
        }

        static void SetBalanceMode(GameplayBalanceMode mode)
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            GameSessionManager sessionManager = Object.FindAnyObjectByType<GameSessionManager>();
            if (sessionManager == null)
            {
                Debug.LogWarning("GameSessionManager not found in the open scene.");
                return;
            }

            SerializedObject serialized = new SerializedObject(sessionManager);
            serialized.FindProperty("balanceMode").enumValueIndex = (int)mode;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            sessionManager.ApplyBalanceModeFromInspector();
            EditorUtility.SetDirty(sessionManager);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"Balance mode set to {mode}. Save the scene (Ctrl+S) before Play.");
        }

        [MenuItem("AoE/CPU Attack Pace/Relaxed (2min peace)", true)]
        static bool ValidateSetRelaxedCpuPace() => !EditorApplication.isPlaying;

        [MenuItem("AoE/CPU Attack Pace/Relaxed (2min peace)")]
        public static void SetRelaxedCpuPace()
        {
            SetCpuAttackPace(CpuAttackPace.Relaxed);
        }

        [MenuItem("AoE/CPU Attack Pace/Aggressive (fast attacks)", true)]
        static bool ValidateSetAggressiveCpuPace() => !EditorApplication.isPlaying;

        [MenuItem("AoE/CPU Attack Pace/Aggressive (fast attacks)")]
        public static void SetAggressiveCpuPace()
        {
            SetCpuAttackPace(CpuAttackPace.Aggressive);
        }

        static void SetCpuAttackPace(CpuAttackPace pace)
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            GameSessionManager sessionManager = Object.FindAnyObjectByType<GameSessionManager>();
            if (sessionManager == null)
            {
                Debug.LogWarning("GameSessionManager not found in the open scene.");
                return;
            }

            SerializedObject serialized = new SerializedObject(sessionManager);
            serialized.FindProperty("cpuAttackPace").enumValueIndex = (int)pace;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(sessionManager);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"CPU attack pace set to {pace}. Save the scene (Ctrl+S) before Play.");
        }
    }
}
