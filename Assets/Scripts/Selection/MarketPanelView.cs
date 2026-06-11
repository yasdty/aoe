using AoE.RTS.Buildings;
using AoE.RTS.Commands;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace AoE.RTS.Selection
{
    public class MarketPanelView : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;
        [SerializeField] MarketTradeData tradeRates;

        const float PanelWidth = 260f;
        const float LineHeight = 20f;
        const float ButtonHeight = 26f;

        static readonly MarketTradeAction[] TradeActions =
        {
            MarketTradeAction.SellFood,
            MarketTradeAction.BuyFood,
            MarketTradeAction.SellWood,
            MarketTradeAction.BuyWood,
            MarketTradeAction.SellStone,
            MarketTradeAction.BuyStone
        };

        RectTransform panelRoot;
        Text headerText;
        readonly Button[] tradeButtons = new Button[6];
        bool uiBuilt;

        void Awake()
        {
            if (selectionManager == null)
                selectionManager = GetComponent<SelectionManager>();
            if (selectionManager == null)
                selectionManager = FindAnyObjectByType<SelectionManager>();
            MarketTradeDataResolver.ResolveDefault(ref tradeRates);
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

            panelRoot = HudUiFactory.CreatePanel(stack, "MarketPanel", HudUiFactory.PanelBackgroundColor);
            HudUiFactory.AddVerticalLayout(panelRoot, 4f, reverseArrangement: false);
            panelRoot.gameObject.AddComponent<LayoutElement>().preferredWidth = PanelWidth;
            GameUiInput.RegisterHudPanel(panelRoot);

            headerText = HudUiFactory.CreateLabel(panelRoot, "Header", LineHeight, bold: true);
            for (int i = 0; i < TradeActions.Length; i++)
            {
                int actionIndex = i;
                tradeButtons[i] = HudUiFactory.CreateButton(panelRoot, $"Trade{i}", ButtonHeight);
                tradeButtons[i].onClick.AddListener(() => OnTradeClicked(actionIndex));
            }

            panelRoot.gameObject.SetActive(false);
            uiBuilt = true;
        }

        void OnTradeClicked(int actionIndex)
        {
            Market market = selectionManager != null ? selectionManager.SelectedMarket : null;
            MarketTradeData rates = MarketTradeDataResolver.ResolveDefault(ref tradeRates);
            if (market == null || rates == null || actionIndex < 0 || actionIndex >= TradeActions.Length)
                return;

            CommandQueue.Enqueue(new MarketTradeCommand(market, TradeActions[actionIndex], rates));
        }

        void LateUpdate()
        {
            TryBuildUi();
            if (!uiBuilt || selectionManager == null)
                return;

            Market market = selectionManager.SelectedMarket;
            MarketTradeData rates = MarketTradeDataResolver.ResolveDefault(ref tradeRates);
            bool visible = market != null && rates != null;
            panelRoot.gameObject.SetActive(visible);
            if (!visible)
                return;

            HudUiFactory.SetText(headerText, Localization.BuildingName(PlacedBuildingKind.Market));
            bool gameOver = GameSessionManager.IsGameOver;
            for (int i = 0; i < TradeActions.Length; i++)
            {
                MarketTradeAction action = TradeActions[i];
                bool canTrade = MarketTradeUtility.CanTrade(market, action, rates);
                tradeButtons[i].interactable = canTrade && !gameOver;
                HudUiFactory.SetButtonLabel(tradeButtons[i], MarketTradeUtility.FormatTradeButtonLabel(action, rates));
            }
        }
    }
}
