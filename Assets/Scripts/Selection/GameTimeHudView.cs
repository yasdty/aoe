using AoE.RTS.AI;
using AoE.RTS.Buildings;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Units;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AoE.RTS.Selection
{
    public class GameTimeHudView : MonoBehaviour
    {
        const float Margin = 12f;
        const float PanelWidth = 240f;
        const float LineHeight = 20f;
        const float Padding = 8f;

        void Update()
        {
            if (GameSessionManager.IsGameOver || Keyboard.current == null)
                return;

            if (Keyboard.current.pKey.wasPressedThisFrame)
                GameSessionManager.ToggleCpuAttackPace();
        }

        void OnGUI()
        {
            float lineCount = 2f;
            if (CpuMilitaryAiManager.Instance != null)
                lineCount += 3f;
            if (GameplayBalance.Mode == GameplayBalanceMode.Debug && CpuMilitaryAiManager.Instance != null)
                lineCount += 1f;

            float panelHeight = Padding * 2f + LineHeight * lineCount;
            float x = Screen.width * 0.5f - PanelWidth * 0.5f;
            Rect panelRect = new Rect(x, Margin, PanelWidth, panelHeight);
            GameUiInput.ExpandHudPanelScreenRect(panelRect);

            GUI.Box(panelRect, GUIContent.none);

            float y = Margin + Padding;
            DrawLine(x, ref y, $"Time: {FormatTime(Time.timeSinceLevelLoad)}");

            CpuMilitaryAiManager military = CpuMilitaryAiManager.Instance;
            if (military == null)
                return;

            if (military.IsAttackGraceActive)
            {
                DrawLine(x, ref y, $"CPU peace: {FormatTime(military.WaveTimerRemaining)}");
            }
            else
            {
                DrawLine(x, ref y, $"Next wave: {FormatTime(military.WaveTimerRemaining)}");
            }

            string paceLabel = GameSessionManager.CpuAttackPace == CpuAttackPace.Relaxed
                ? "Relaxed"
                : "Aggressive";
            Rect paceRect = new Rect(x + Padding, y, PanelWidth - Padding * 2f, LineHeight);
            GUI.enabled = !GameSessionManager.IsGameOver;
            if (GUI.Button(paceRect, $"CPU pace: {paceLabel} (click / P)"))
                GameSessionManager.ToggleCpuAttackPace();
            GUI.enabled = true;
            y += LineHeight;

            if (GameplayBalance.Mode == GameplayBalanceMode.Debug)
                DrawLine(x, ref y, "Debug: K=TC dmg, Shift+K=CPU wave");

            if (military.HasCpuBarracks)
            {
                DrawLine(x, ref y, "Barracks: built");
            }
            else if (military.IsBuildingCpuBarracks)
            {
                DrawLine(x, ref y, "Barracks: building");
            }
            else if (military.BarracksBuildDelayRemaining > 0f)
            {
                DrawLine(x, ref y, $"Barracks after: {FormatTime(military.BarracksBuildDelayRemaining)}");
            }
            else
            {
                int wood = Mathf.FloorToInt(ResourceManager.GetWood(UnitTeam.Enemy));
                int cost = Mathf.FloorToInt(military.BarracksWoodCost);
                if (wood < cost)
                    DrawLine(x, ref y, $"Barracks: need {cost} Wood ({wood}/{cost})");
                else
                    DrawLine(x, ref y, "Barracks: starting soon");
            }
        }

        void DrawLine(float panelX, ref float y, string text)
        {
            Rect rect = new Rect(panelX + Padding, y, PanelWidth - Padding * 2f, LineHeight);
            GUI.Label(rect, text);
            y += LineHeight;
        }

        static string FormatTime(float seconds)
        {
            seconds = Mathf.Max(0f, seconds);
            int total = Mathf.FloorToInt(seconds);
            int minutes = total / 60;
            int secs = total % 60;
            return $"{minutes:00}:{secs:00}";
        }
    }
}
