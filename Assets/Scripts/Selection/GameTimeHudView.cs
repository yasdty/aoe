using AoE.RTS.AI;
using AoE.RTS.Buildings;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Units;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace AoE.RTS.Selection
{
    public class GameTimeHudView : MonoBehaviour
    {
        const float PanelWidth = 240f;
        const float LineHeight = 20f;
        const float Padding = 8f;

        RectTransform panelRoot;
        Text timeText;
        Button languageButton;
        Text waveText;
        Button paceButton;
        Text debugText;
        Text barracksText;
        bool uiBuilt;

        void Awake()
        {
            TryBuildUi();
        }

        void OnDestroy()
        {
            if (panelRoot != null)
                GameUiInput.UnregisterHudPanel(panelRoot);
        }

        void TryBuildUi()
        {
            if (uiBuilt)
                return;

            Transform hudRoot = HudUiFactory.GetHudRoot();
            if (hudRoot == null)
                return;

            panelRoot = HudUiFactory.SetupScreenPanel(
                hudRoot,
                "GameTimeHudPanel",
                HudUiFactory.PanelBackgroundColor,
                0f,
                HudUiFactory.Margin,
                PanelWidth,
                220f,
                topLeftAnchor: false);
            GameUiInput.RegisterHudPanel(panelRoot);
            HudUiFactory.AddVerticalLayout(panelRoot, 2f, reverseArrangement: false);

            timeText = HudUiFactory.CreateLabel(panelRoot, "Time", LineHeight);
            languageButton = HudUiFactory.CreateButton(panelRoot, "LanguageButton", LineHeight);
            languageButton.onClick.AddListener(Localization.ToggleLanguage);
            waveText = HudUiFactory.CreateLabel(panelRoot, "Wave", LineHeight);
            paceButton = HudUiFactory.CreateButton(panelRoot, "PaceButton", LineHeight);
            paceButton.onClick.AddListener(GameSessionManager.ToggleCpuDifficulty);
            debugText = HudUiFactory.CreateLabel(panelRoot, "Debug", LineHeight);
            barracksText = HudUiFactory.CreateLabel(panelRoot, "Barracks", LineHeight);

            uiBuilt = true;
        }

        void Update()
        {
            if (GameSessionManager.IsGameOver || Keyboard.current == null)
                return;

            if (Keyboard.current.pKey.wasPressedThisFrame)
                GameSessionManager.ToggleCpuDifficulty();

            if (Keyboard.current.lKey.wasPressedThisFrame)
                Localization.ToggleLanguage();
        }

        void LateUpdate()
        {
            TryBuildUi();
            if (!uiBuilt)
                return;

            HudUiFactory.SetText(timeText, $"Time: {FormatTime(Time.timeSinceLevelLoad)}");
            HudUiFactory.SetButtonLabel(
                languageButton,
                Localization.Format("ui.language_toggle", Localization.CurrentLanguageLabel()));

            CpuMilitaryAiManager military = CpuMilitaryAiManager.Instance;
            bool showMilitary = military != null;
            waveText.gameObject.SetActive(showMilitary);
            paceButton.gameObject.SetActive(showMilitary);

            if (showMilitary)
            {
                CpuDifficulty effective = CpuDifficultySettings.EffectiveDifficulty;
                HudUiFactory.SetText(
                    waveText,
                    $"CPU army: {military.CpuArmyCount} / atk≥{military.AttackThreshold}");

                string difficultyLabel = effective.ToString();
                if (GameplayBalance.Mode == GameplayBalanceMode.Debug)
                    difficultyLabel += " (Debug lock)";

                HudUiFactory.SetButtonLabel(paceButton, $"CPU: {difficultyLabel} (click / P)");
                paceButton.interactable =
                    !GameSessionManager.IsGameOver
                    && GameplayBalance.Mode != GameplayBalanceMode.Debug;
            }

            bool showDebug = showMilitary && GameplayBalance.Mode == GameplayBalanceMode.Debug;
            debugText.gameObject.SetActive(showDebug);
            if (showDebug)
                HudUiFactory.SetText(debugText, "Debug: K=TC dmg, Shift+K=CPU attack");

            barracksText.gameObject.SetActive(showMilitary);
            if (showMilitary)
                HudUiFactory.SetText(barracksText, BuildBarracksStatus(military));

            int visibleRows = 2 + (showMilitary ? 2 : 0) + (showDebug ? 1 : 0) + (showMilitary ? 1 : 0);
            panelRoot.sizeDelta = new Vector2(PanelWidth, Padding * 2f + LineHeight * visibleRows + 2f * (visibleRows - 1));
        }

        static string BuildBarracksStatus(CpuMilitaryAiManager military)
        {
            if (military.HasCpuBarracks)
                return "Barracks: built";
            if (military.IsBuildingCpuBarracks)
                return "Barracks: building";
            if (military.BarracksBuildDelayRemaining > 0f)
                return $"Barracks after: {FormatTime(military.BarracksBuildDelayRemaining)}";

            int wood = Mathf.FloorToInt(ResourceManager.GetWood(UnitTeam.Enemy));
            int cost = Mathf.FloorToInt(military.BarracksWoodCost);
            if (wood < cost)
                return $"Barracks: need {cost} Wood ({wood}/{cost})";
            return "Barracks: starting soon";
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
