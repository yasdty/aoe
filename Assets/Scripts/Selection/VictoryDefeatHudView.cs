using AoE.RTS.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AoE.RTS.Selection
{
    public class VictoryDefeatHudView : MonoBehaviour
    {
        RectTransform panelRoot;
        Text titleText;
        Text subtitleText;

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

            panelRoot = HudUiFactory.GetOrCreateHudChild(hudRoot, "VictoryOverlay");
            HudUiFactory.ClearLegacyPanelWrapper(panelRoot);
            HudUiFactory.SetStretchFull(panelRoot);
            HudUiFactory.EnsurePanelBackground(panelRoot, HudUiFactory.OverlayBackgroundColor);
            GameUiInput.RegisterHudPanel(panelRoot);

            GameObject contentObject = new GameObject("Content", typeof(RectTransform));
            contentObject.transform.SetParent(panelRoot, false);
            RectTransform contentRect = contentObject.GetComponent<RectTransform>();
            HudUiFactory.SetStretchFull(contentRect);

            titleText = HudUiFactory.CreateLabel(contentObject.transform, "Title", 64f, bold: true);
            RectTransform titleRect = titleText.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 0.55f);
            titleRect.anchorMax = new Vector2(1f, 0.85f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.fontSize = 48;

            subtitleText = HudUiFactory.CreateLabel(contentObject.transform, "Subtitle", 32f);
            RectTransform subtitleRect = subtitleText.rectTransform;
            subtitleRect.anchorMin = new Vector2(0f, 0.25f);
            subtitleRect.anchorMax = new Vector2(1f, 0.45f);
            subtitleRect.offsetMin = Vector2.zero;
            subtitleRect.offsetMax = Vector2.zero;
            subtitleText.alignment = TextAnchor.MiddleCenter;
            subtitleText.fontSize = 18;

            panelRoot.gameObject.SetActive(false);
            uiBuilt = true;
        }

        void Update()
        {
            if (!GameSessionManager.IsGameOver)
                return;

            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        void LateUpdate()
        {
            TryBuildUi();
            if (!uiBuilt)
                return;

            bool show = GameSessionManager.IsGameOver;
            panelRoot.gameObject.SetActive(show);
            if (!show)
                return;

            MatchState state = GameSessionManager.State;
            bool victory = state == MatchState.Victory;
            HudUiFactory.SetText(titleText, victory ? Localization.Get("ui.victory") : Localization.Get("ui.defeat"));
            titleText.color = victory
                ? new Color(0.35f, 0.95f, 0.45f)
                : new Color(0.95f, 0.35f, 0.35f);
            HudUiFactory.SetText(subtitleText, Localization.Get("ui.restart_hint"));
        }
    }
}
