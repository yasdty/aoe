using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Commands;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Input;
using UnityEngine;
using UnityEngine.UI;

namespace AoE.RTS.Selection
{
    public class ArcheryRangePanelView : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;
        [SerializeField] RTSInputReader input;

        const float PanelWidth = 220f;
        const float LineHeight = 20f;
        const float ButtonHeight = 28f;

        RectTransform panelRoot;
        Text headerText;
        Button trainButton;
        Text statusText;
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

            panelRoot = HudUiFactory.CreatePanel(stack, "ArcheryRangePanel", HudUiFactory.PanelBackgroundColor);
            HudUiFactory.AddVerticalLayout(panelRoot, 4f, reverseArrangement: false);
            panelRoot.gameObject.AddComponent<LayoutElement>().preferredWidth = PanelWidth;
            GameUiInput.RegisterHudPanel(panelRoot);

            headerText = HudUiFactory.CreateLabel(panelRoot, "Header", LineHeight, bold: true);
            trainButton = HudUiFactory.CreateButton(panelRoot, "TrainArcher", ButtonHeight);
            trainButton.onClick.AddListener(OnTrainClicked);

            GameObject queueHost = new GameObject("QueueList");
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
            ArcheryRange archeryRange = selectionManager != null ? selectionManager.SelectedArcheryRange : null;
            if (archeryRange != null)
                CommandQueue.Enqueue(new TrainArcherCommand(archeryRange));
        }

        void Update()
        {
            if (GameSessionManager.IsGameOver || selectionManager == null || input == null)
                return;

            ArcheryRange archeryRange = selectionManager.SelectedArcheryRange;
            if (archeryRange == null)
                return;

            if (input.WasTrainVillagerPressedThisFrame())
                CommandQueue.Enqueue(new TrainArcherCommand(archeryRange));
        }

        void LateUpdate()
        {
            TryBuildUi();
            if (!uiBuilt || selectionManager == null)
                return;

            ArcheryRange archeryRange = selectionManager.SelectedArcheryRange;
            bool visible = archeryRange != null && archeryRange.Data != null;
            panelRoot.gameObject.SetActive(visible);
            if (!visible)
                return;

            PlacedBuildingData data = archeryRange.Data;
            HudUiFactory.SetText(headerText, Localization.BuildingName(PlacedBuildingKind.ArcheryRange));

            int queueCount = ArcheryRangeProductionManager.GetQueueCount(archeryRange);
            bool queueFull = queueCount >= ArcheryRangeProductionManager.MaxQueueSize;
            bool populationFull = !PopulationManager.CanTrainUnit();
            bool canAffordWood = ResourceManager.Wood >= data.ScaledTrainWoodCost;
            bool canAffordFood = ResourceManager.Food >= data.ScaledTrainFoodCost;
            bool canAfford = canAffordWood && canAffordFood;

            trainButton.interactable = !queueFull && !populationFull && canAfford && !GameSessionManager.IsGameOver;
            HudUiFactory.SetButtonLabel(
                trainButton,
                Localization.Format(
                    "ui.create_unit_dual",
                    Localization.Get("unit.archer"),
                    Mathf.CeilToInt(data.ScaledTrainWoodCost),
                    Mathf.CeilToInt(data.ScaledTrainFoodCost)));

            if (queueCount > 0)
            {
                ArcheryRangeProductionManager.GetQueueEntries(archeryRange, queueEntriesBuffer);
                queueListView.Refresh(
                    queueEntriesBuffer,
                    index => ArcheryRangeProductionManager.TryCancelQueueItem(archeryRange, index));
            }
            else
                queueListView.HideAll();

            string status = string.Empty;
            if (queueFull)
                status = Localization.Get("ui.queue_full");
            else if (populationFull)
                status = Localization.Get("ui.population_full");
            else if (!canAffordWood)
                status = Localization.Get("ui.need_wood");
            else if (!canAffordFood)
                status = Localization.Get("ui.need_food");
            statusText.gameObject.SetActive(!string.IsNullOrEmpty(status));
            HudUiFactory.SetText(statusText, status);

            bool isProducing = queueCount > 0;
            trainingText.gameObject.SetActive(isProducing);
            progressSlider.gameObject.SetActive(isProducing);
            if (isProducing)
            {
                float total = ArcheryRangeProductionManager.GetTotalSeconds(archeryRange);
                float remaining = ArcheryRangeProductionManager.GetRemainingSeconds(archeryRange);
                progressSlider.value = total > 0f ? 1f - remaining / total : 0f;
                HudUiFactory.SetText(trainingText, Localization.Format("ui.training", remaining));
            }
        }
    }
}
