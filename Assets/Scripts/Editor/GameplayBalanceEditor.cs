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

        [MenuItem("AoE/CPU Difficulty/Easy", true)]
        static bool ValidateSetCpuEasy() => !EditorApplication.isPlaying;

        [MenuItem("AoE/CPU Difficulty/Easy")]
        public static void SetCpuEasy() => SetCpuDifficulty(CpuDifficulty.Easy);

        [MenuItem("AoE/CPU Difficulty/Normal", true)]
        static bool ValidateSetCpuNormal() => !EditorApplication.isPlaying;

        [MenuItem("AoE/CPU Difficulty/Normal")]
        public static void SetCpuNormal() => SetCpuDifficulty(CpuDifficulty.Normal);

        [MenuItem("AoE/CPU Difficulty/Hard", true)]
        static bool ValidateSetCpuHard() => !EditorApplication.isPlaying;

        [MenuItem("AoE/CPU Difficulty/Hard")]
        public static void SetCpuHard() => SetCpuDifficulty(CpuDifficulty.Hard);

        [MenuItem("AoE/CPU Difficulty/Hardest", true)]
        static bool ValidateSetCpuHardest() => !EditorApplication.isPlaying;

        [MenuItem("AoE/CPU Difficulty/Hardest")]
        public static void SetCpuHardest() => SetCpuDifficulty(CpuDifficulty.Hardest);

        static void SetCpuDifficulty(CpuDifficulty difficulty)
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
            serialized.FindProperty("cpuDifficulty").enumValueIndex = (int)difficulty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(sessionManager);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"CPU difficulty set to {difficulty}. Save the scene (Ctrl+S) before Play.");
        }
    }
}
