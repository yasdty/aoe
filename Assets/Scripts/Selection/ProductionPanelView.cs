using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Commands;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Input;
using AoE.RTS.Units;
using UnityEngine;
using UnityEngine.UI;

namespace AoE.RTS.Selection
{
    public class ProductionPanelView : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;
        [SerializeField] RTSInputReader input;
        [SerializeField] AgeData feudalAgeData;

        const float PanelWidth = 220f;
        const float LineHeight = 20f;
        const float ButtonHeight = 28f;

        RectTransform panelRoot;
        Text headerText;
        Text ageText;
        Button trainButton;
        Button ageUpButton;
        Text statusText;
        Text ageUpHintText;
        Text trainingText;
        Slider progressSlider;
        ProductionQueueListView queueListView;
        readonly List<ProductionQueueEntry> queueEntriesBuffer = new List<ProductionQueueEntry>();
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

            panelRoot = HudUiFactory.CreatePanel(stack, "ProductionPanel", HudUiFactory.PanelBackgroundColor);
            panelRoot.SetAsFirstSibling();
            HudUiFactory.AddVerticalLayout(panelRoot, 4f, reverseArrangement: false);
            LayoutElement layoutElement = panelRoot.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = PanelWidth;
            GameUiInput.RegisterHudPanel(panelRoot);

            headerText = HudUiFactory.CreateLabel(panelRoot, "Header", LineHeight, bold: true);
            ageText = HudUiFactory.CreateLabel(panelRoot, "Age", LineHeight);
            trainButton = HudUiFactory.CreateButton(panelRoot, "TrainVillager", ButtonHeight);
            trainButton.onClick.AddListener(OnTrainClicked);
            ageUpButton = HudUiFactory.CreateButton(panelRoot, "AgeUp", ButtonHeight);
            ageUpButton.onClick.AddListener(OnAgeUpClicked);
            ageUpHintText = HudUiFactory.CreateLabel(panelRoot, "AgeUpHint", LineHeight);

            GameObject queueHost = new GameObject("QueueList", typeof(RectTransform));
            queueHost.transform.SetParent(panelRoot, false);
            queueListView = queueHost.AddComponent<ProductionQueueListView>();
            queueListView.Initialize(queueHost.transform, 24f);

            statusText = HudUiFactory.CreateLabel(panelRoot, "Status", LineHeight);
            trainingText = HudUiFactory.CreateLabel(panelRoot, "Training", LineHeight);
            progressSlider = HudUiFactory.CreateSlider(panelRoot, "Progress", 18f);

            panelRoot.gameObject.SetActive(false);
            uiBuilt = true;
        }

        void OnTrainClicked()
        {
            TownCenter townCenter = selectionManager != null ? selectionManager.SelectedTownCenter : null;
            if (townCenter != null)
                CommandQueue.Enqueue(new TrainVillagerCommand(townCenter));
        }

        void OnAgeUpClicked()
        {
            TownCenter townCenter = selectionManager != null ? selectionManager.SelectedTownCenter : null;
            if (townCenter != null)
                CommandQueue.Enqueue(new AgeUpCommand(townCenter));
        }

        void Update()
        {
            if (GameSessionManager.IsGameOver || selectionManager == null || input == null)
                return;

            TownCenter townCenter = selectionManager.SelectedTownCenter;
            if (townCenter == null)
                return;

            if (input.WasTrainVillagerPressedThisFrame())
            {
                int count = input.IsShiftHeld ? ProductionQueuePanelUi.ShiftBatchQueueCount : 1;
                for (int i = 0; i < count; i++)
                    CommandQueue.Enqueue(new TrainVillagerCommand(townCenter));
            }
        }

        void LateUpdate()
        {
            TryBuildUi();
            if (!uiBuilt || selectionManager == null)
                return;

            TownCenter townCenter = selectionManager.SelectedTownCenter;
            bool visible = townCenter != null;
            panelRoot.gameObject.SetActive(visible);
            if (!visible)
                return;

            HudUiFactory.SetText(headerText, Localization.Get("ui.town_center"));
            HudUiFactory.SetText(
                ageText,
                Localization.Format("ui.age_label", Localization.AgeName(GameSessionManager.GetAge(townCenter.Team))));

            int queueCount = ProductionManager.GetQueueCount(townCenter);
            bool queueFull = queueCount >= ProductionManager.MaxQueueSize;
            bool populationFull = !PopulationManager.CanTrainUnit();
            float foodCost = townCenter.Data != null ? townCenter.Data.ScaledVillagerFoodCost : 0f;
            bool canAffordFood = ResourceManager.GetFood(UnitTeam.Player) >= foodCost;
            trainButton.interactable = !queueFull && !populationFull && canAffordFood && !GameSessionManager.IsGameOver;
            HudUiFactory.SetButtonLabel(
                trainButton,
                Localization.Format("ui.create_villager", Mathf.CeilToInt(foodCost)));

            RefreshAgeUpButton(townCenter);

            if (queueCount > 0)
            {
                ProductionManager.GetQueueEntries(townCenter, queueEntriesBuffer);
                queueListView.Refresh(
                    queueEntriesBuffer,
                    index => ProductionManager.TryCancelQueueItem(townCenter, index));
            }
            else
                queueListView.HideAll();

            string status = string.Empty;
            if (queueFull)
                status = Localization.Get("ui.queue_full");
            else if (populationFull)
                status = Localization.Get("ui.population_full");
            else if (!canAffordFood)
                status = Localization.Get("ui.need_food");
            statusText.gameObject.SetActive(!string.IsNullOrEmpty(status));
            HudUiFactory.SetText(statusText, status);

            bool isProducing = queueCount > 0;
            trainingText.gameObject.SetActive(isProducing);
            progressSlider.gameObject.SetActive(isProducing);
            if (isProducing)
            {
                float total = ProductionManager.GetTotalSeconds(townCenter);
                float remaining = ProductionManager.GetRemainingSeconds(townCenter);
                float progress = total > 0f ? 1f - remaining / total : 0f;
                HudUiFactory.SetText(trainingText, Localization.Format("ui.training", remaining));
                progressSlider.value = progress;
            }
        }

        void RefreshAgeUpButton(TownCenter townCenter)
        {
            if (townCenter.Team != UnitTeam.Player || GameSessionManager.GetAge(townCenter.Team) >= GameAge.Feudal)
            {
                ageUpButton.gameObject.SetActive(false);
                ageUpHintText.gameObject.SetActive(false);
                return;
            }

            AgeData ageData = feudalAgeData;
            if (ageData == null)
            {
                ageUpButton.gameObject.SetActive(false);
                ageUpHintText.gameObject.SetActive(false);
                return;
            }

            ageUpButton.gameObject.SetActive(true);
            float foodCost = GameplayBalance.ScaleResourceCost(ageData.upgradeFoodCost);
            float goldCost = GameplayBalance.ScaleResourceCost(ageData.upgradeGoldCost);
            bool canAfford = ResourceManager.Food >= foodCost && ResourceManager.Gold >= goldCost;
            ageUpButton.interactable = canAfford && !GameSessionManager.IsGameOver;
            HudUiFactory.SetButtonLabel(
                ageUpButton,
                Localization.Format(
                    "ui.age_up_feudal",
                    Mathf.CeilToInt(foodCost),
                    Mathf.CeilToInt(goldCost)));

            ageUpHintText.gameObject.SetActive(!canAfford);
            if (!canAfford)
                HudUiFactory.SetText(ageUpHintText, Localization.Get("ui.need_food_gold_feudal"));
        }
    }
}
