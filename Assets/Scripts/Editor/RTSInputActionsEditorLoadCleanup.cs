using UnityEditor;

namespace AoE.RTS.EditorTools
{
    /// <summary>
    /// Editor 起動時に Project-wide Input Actions の壊れた参照を除去する。
    /// </summary>
    [InitializeOnLoad]
    static class RTSInputActionsEditorLoadCleanup
    {
        static RTSInputActionsEditorLoadCleanup()
        {
            EditorApplication.delayCall += RTSInputActionsProjectSettings.ClearStaleProjectWideBinding;
        }
    }
}
