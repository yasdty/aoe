using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AoE.RTS.EditorTools
{
    [InitializeOnLoad]
    public static class Phase10SceneAutoCreate
    {
        const string ScenePath = "Assets/Scenes/Phase10.unity";

        static Phase10SceneAutoCreate()
        {
            EditorApplication.delayCall += TryPromptCreatePhase10Scene;
        }

        static void TryPromptCreatePhase10Scene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (File.Exists(ScenePath))
                return;

            bool create = EditorUtility.DisplayDialog(
                "Phase10 シーンがありません",
                "Assets/Scenes/Phase10.unity が見つかりません。\n\n"
                + "今すぐ Phase10 シーンを生成しますか？\n"
                + "（メニュー AoE → Setup Phase10 Scene と同じ処理です）",
                "生成する",
                "あとで");

            if (!create)
            {
                Debug.LogWarning(
                    "Phase10 scene is missing. Run AoE → Setup Phase10 Scene, or close Unity and regenerate from repo.");
                return;
            }

            Phase10SceneBuilder.SetupPhase10Scene();

            if (File.Exists(ScenePath))
                EditorSceneManager.OpenScene(ScenePath);
        }
    }
}
