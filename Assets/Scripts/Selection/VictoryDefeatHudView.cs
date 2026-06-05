using AoE.RTS.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace AoE.RTS.Selection
{
    public class VictoryDefeatHudView : MonoBehaviour
    {
        void OnGUI()
        {
            if (!GameSessionManager.IsGameOver)
                return;

            MatchState state = GameSessionManager.State;
            string title = state == MatchState.Victory ? "VICTORY" : "DEFEAT";
            Color titleColor = state == MatchState.Victory
                ? new Color(0.35f, 0.95f, 0.45f)
                : new Color(0.95f, 0.35f, 0.35f);

            const float boxWidth = 420f;
            const float boxHeight = 160f;
            Rect boxRect = new Rect(
                (Screen.width - boxWidth) * 0.5f,
                (Screen.height - boxHeight) * 0.5f,
                boxWidth,
                boxHeight);

            GUI.Box(boxRect, GUIContent.none);

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 48,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = titleColor }
            };

            GUIStyle subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            Rect titleRect = new Rect(boxRect.x, boxRect.y + 24f, boxRect.width, 64f);
            GUI.Label(titleRect, title, titleStyle);

            Rect subtitleRect = new Rect(boxRect.x, boxRect.y + 96f, boxRect.width, 32f);
            GUI.Label(subtitleRect, "Press R to restart", subtitleStyle);
        }

        void Update()
        {
            if (!GameSessionManager.IsGameOver)
                return;

            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
