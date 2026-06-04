using AoE.RTS.Core;
using UnityEditor;
using UnityEngine.InputSystem;

namespace AoE.RTS.EditorTools
{
    /// <summary>
    /// Project-wide Input Actions の不正参照を除去する（手書き fileID 付き config 対策）。
    /// </summary>
    static class RTSInputActionsProjectSettings
    {
        internal const string ProjectWideActionsConfigKey = "com.unity.input.settings.actions";

        public static void ClearStaleProjectWideBinding()
        {
            if (EditorBuildSettings.TryGetConfigObject(ProjectWideActionsConfigKey, out InputActionAsset configured))
            {
                string path = configured != null ? AssetDatabase.GetAssetPath(configured) : null;
                if (configured == null
                    || path == GameAssetPaths.RTSInputActions
                    || !IsAssetLoadable(GameAssetPaths.RTSInputActions))
                {
                    EditorBuildSettings.RemoveConfigObject(ProjectWideActionsConfigKey);
                }
            }

            if (InputSystem.actions != null)
            {
                string actionsPath = AssetDatabase.GetAssetPath(InputSystem.actions);
                if (actionsPath == GameAssetPaths.RTSInputActions && !IsAssetLoadable(actionsPath))
                    InputSystem.actions = null;
            }
        }

        static bool IsAssetLoadable(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return false;

            InputActionAsset asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(assetPath);
            if (asset == null)
                return false;

            return asset.FindActionMap("Gameplay", false) != null;
        }
    }
}
