using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AoE.RTS.EditorTools
{
    /// <summary>
    /// Play 中に Game ビューがフォーカスされているとき、Editor の Ctrl+1〜9 等のショートカットを無効化する。
    /// Game ビュー右上の「Shortcuts」ボタン（オフ）と同等。Ctrl+数字のコントロールグループ保存用。
    /// </summary>
    [InitializeOnLoad]
    static class PlayModeEditorShortcutGuard
    {
        const string ShortcutIntegrationTypeName =
            "UnityEditor.ShortcutManagement.ShortcutIntegration, UnityEditor.CoreModule";
        const string IgnoreWhenPlayModeFocusedProperty = "ignoreWhenPlayModeFocused";

        static PlayModeEditorShortcutGuard()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                EnableIgnoreWhenPlayModeFocused();
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
                EnableIgnoreWhenPlayModeFocused();
        }

        [MenuItem("AoE/Play Mode/Disable Editor Shortcuts When Game View Focused")]
        static void MenuEnableIgnoreWhenPlayModeFocused()
        {
            if (TrySetIgnoreWhenPlayModeFocused(true))
            {
                Debug.Log("[AoE] Editor shortcuts will be ignored while the Game view has focus during Play.");
                return;
            }

            Debug.LogWarning(
                "[AoE] Could not enable ignoreWhenPlayModeFocused via API. "
                + "Turn off the Shortcuts toggle in the top-right of the Game view.");
        }

        static void EnableIgnoreWhenPlayModeFocused()
        {
            if (TrySetIgnoreWhenPlayModeFocused(true))
                return;

            Debug.LogWarning(
                "[AoE] Play-mode shortcut guard failed. "
                + "Disable the Game view Shortcuts toggle (top-right) so Ctrl+1..9 control groups work.");
        }

        static bool TrySetIgnoreWhenPlayModeFocused(bool value)
        {
            Type integrationType = Type.GetType(ShortcutIntegrationTypeName);
            if (integrationType == null)
                return false;

            PropertyInfo property = integrationType.GetProperty(
                IgnoreWhenPlayModeFocusedProperty,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (property == null || !property.CanWrite)
                return false;

            property.SetValue(null, value);
            return true;
        }
    }
}
