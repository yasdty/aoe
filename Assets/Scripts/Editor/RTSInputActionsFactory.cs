using System;
using System.IO;
using AoE.RTS.Core;
using AoE.RTS.Input;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AoE.RTS.EditorTools
{
    /// <summary>
    /// RTSInputActions.inputactions の唯一の生成元。CONSTITUTION の Unity アセット生成ルールに従い、
    /// Input System API で構築したうえで ToJson + ImportAsset する（リポジトリへ JSON 手書きしない）。
    /// </summary>
    static class RTSInputActionsFactory
    {
        public const string AssetPath = GameAssetPaths.RTSInputActions;

        public static InputActionAsset EnsureAsset()
        {
            RTSInputActionsProjectSettings.ClearStaleProjectWideBinding();

            if (TryLoadValid(out InputActionAsset valid))
                return valid;

            DeleteAssetCompletely();
            return CreateAndImport();
        }

        static bool TryLoadValid(out InputActionAsset asset)
        {
            asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(AssetPath);
            if (asset == null)
                return false;

            InputActionMap map = asset.FindActionMap("Gameplay", false);
            if (map == null)
                return false;

            if (map.FindAction("Select", false) == null
                || map.FindAction("Command", false) == null
                || map.FindAction("MoveCamera", false) == null
                || map.FindAction("Zoom", false) == null
                || map.FindAction("PointerPosition", false) == null
                || map.FindAction("TrainVillager", false) == null
                || map.FindAction("SelectNextIdleVillager", false) == null
                || map.FindAction("SelectNextIdleMilitary", false) == null)
                return false;

            return true;
        }

        static void DeleteAssetCompletely()
        {
            if (AssetDatabase.LoadAssetAtPath<InputActionAsset>(AssetPath) != null)
            {
                AssetDatabase.DeleteAsset(AssetPath);
            }
            else
            {
                if (File.Exists(AssetPath))
                    File.Delete(AssetPath);

                string metaPath = AssetPath + ".meta";
                if (File.Exists(metaPath))
                    File.Delete(metaPath);
            }

            AssetDatabase.Refresh();
        }

        static InputActionAsset CreateAndImport()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Input"))
                AssetDatabase.CreateFolder("Assets", "Input");

            InputActionAsset built = RTSInputActionsBuilder.Build();
            string json = built.ToJson();
            UnityEngine.Object.DestroyImmediate(built);

            File.WriteAllText(AssetPath, json);
            AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceUpdate);

            InputActionAsset imported = AssetDatabase.LoadAssetAtPath<InputActionAsset>(AssetPath);
            if (imported == null || !TryLoadValid(out imported))
                throw new InvalidOperationException("Failed to import RTSInputActions at " + AssetPath);

            return imported;
        }
    }
}
