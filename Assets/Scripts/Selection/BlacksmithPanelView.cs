using AoE.RTS.Buildings;
using AoE.RTS.Commands;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Input;
using UnityEngine;
using UnityEngine.UI;

namespace AoE.RTS.Selection
{
    public class BlacksmithPanelView : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;
        [SerializeField] RTSInputReader input;
        [SerializeField] TechnologyData infantryUpgradeTech;

        const float PanelWidth = 240f;
        const float LineHeight = 20f;
        const float ButtonHeight = 28f;

        RectTransform panelRoot;
        Text headerText;
        Text completeText;
        Button researchButton;
        Text statusText;
        Text researchingText;
        Slider progressSlider;
        bool uiBuilt;

        void Awake()
        {
            if (selectionManager == null)
                selectionManager = FindAnyObjectByType<SelectionManager>();
            if (input == null)
                input = FindAnyObjectByType<RTSInputReader>();
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

            Transform stack = HudBottomLeftStack.GetOrCreate();
            if (stack == null)
                return;

            panelRoot = HudUiFactory.CreatePanel(stack, "BlacksmithPanel", HudUiFactory.PanelBackgroundColor);
            HudUiFactory.AddVerticalLayout(panelRoot, 4f, reverseArrangement: false);
            panelRoot.gameObject.AddComponent<LayoutElement>().preferredWidth = PanelWidth;
            GameUiInput.RegisterHudPanel(panelRoot);

            headerText = HudUiFactory.CreateLabel(panelRoot, "Header", LineHeight, bold: true);
            completeText = HudUiFactory.CreateLabel(panelRoot, "Complete", LineHeight);
            researchButton = HudUiFactory.CreateButton(panelRoot, "Research", ButtonHeight);
            researchButton.onClick.AddListener(OnResearchClicked);
            statusText = HudUiFactory.CreateLabel(panelRoot, "Status", LineHeight);
            researchingText = HudUiFactory.CreateLabel(panelRoot, "Researching", LineHeight);
            progressSlider = HudUiFactory.CreateSlider(panelRoot, "Progress", 18f);
            panelRoot.gameObject.SetActive(false);
            uiBuilt = true;
        }

        void OnResearchClicked()
        {
            Blacksmith blacksmith = selectionManager != null ? selectionManager.SelectedBlacksmith : null;
            if (blacksmith != null)
                CommandQueue.Enqueue(new ResearchInfantryUpgradeCommand(blacksmith));
        }

        void Update()
        {
            if (GameSessionManager.IsGameOver || selectionManager == null || input == null)
                return;

            Blacksmith blacksmith = selectionManager.SelectedBlacksmith;
            if (blacksmith == null || TechnologyState.HasInfantryUpgrade(blacksmith.Team))
                return;

            if (input.WasTrainVillagerPressedThisFrame())
                CommandQueue.Enqueue(new ResearchInfantryUpgradeCommand(blacksmith));
        }

        void LateUpdate()
        {
            TryBuildUi();
            if (!uiBuilt || selectionManager == null)
                return;

            Blacksmith blacksmith = selectionManager.SelectedBlacksmith;
            TechnologyData tech = TechnologyDataResolver.ResolveInfantryUpgrade(ref infantryUpgradeTech);
            bool visible = blacksmith != null && tech != null;
            panelRoot.gameObject.SetActive(visible);
            if (!visible)
                return;

            HudUiFactory.SetText(headerText, Localization.BuildingName(PlacedBuildingKind.Blacksmith));

            bool alreadyResearched = TechnologyState.HasInfantryUpgrade(blacksmith.Team);
            bool isResearching = BlacksmithResearchManager.IsResearching(blacksmith);
            bool canAffordFood = ResourceManager.Food >= tech.ScaledFoodCost;
            bool canAffordGold = ResourceManager.Gold >= tech.ScaledGoldCost;
            bool canAfford = canAffordFood && canAffordGold;

            completeText.gameObject.SetActive(alreadyResearched);
            researchButton.gameObject.SetActive(!alreadyResearched);
            statusText.gameObject.SetActive(!alreadyResearched && !canAfford && !isResearching);

            if (alreadyResearched)
            {
                HudUiFactory.SetText(
                    completeText,
                    Localization.Format("ui.tech_complete", Localization.Get("tech.infantry_upgrade")));
                researchingText.gameObject.SetActive(false);
                progressSlider.gameObject.SetActive(false);
                return;
            }

            researchButton.interactable = !isResearching && canAfford && !GameSessionManager.IsGameOver;
            HudUiFactory.SetButtonLabel(
                researchButton,
                Localization.Format(
                    "ui.research_button",
                    Localization.Get("tech.infantry_upgrade"),
                    Mathf.CeilToInt(tech.ScaledFoodCost),
                    Mathf.CeilToInt(tech.ScaledGoldCost)));

            string status = !canAffordFood
                ? Localization.Get("ui.need_food")
                : !canAffordGold
                    ? Localization.Get("ui.need_gold")
                    : string.Empty;
            HudUiFactory.SetText(statusText, status);

            researchingText.gameObject.SetActive(isResearching);
            progressSlider.gameObject.SetActive(isResearching);
            if (isResearching)
            {
                float total = BlacksmithResearchManager.GetTotalSeconds(blacksmith);
                float remaining = BlacksmithResearchManager.GetRemainingSeconds(blacksmith);
                progressSlider.value = total > 0f ? 1f - remaining / total : 0f;
                HudUiFactory.SetText(researchingText, Localization.Format("ui.researching", remaining));
            }
        }
    }
}
