using AoE.RTS.Buildings;
using AoE.RTS.Commands;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public class MarketPanelView : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;
        [SerializeField] MarketTradeData tradeRates;

        const float PanelWidth = 260f;
        const float PanelHeight = 220f;
        const float Margin = 12f;

        static readonly MarketTradeAction[] TradeActions =
        {
            MarketTradeAction.SellFood,
            MarketTradeAction.BuyFood,
            MarketTradeAction.SellWood,
            MarketTradeAction.BuyWood,
            MarketTradeAction.SellStone,
            MarketTradeAction.BuyStone
        };

        void Awake()
        {
            if (selectionManager == null)
                selectionManager = GetComponent<SelectionManager>();
            if (selectionManager == null)
                selectionManager = FindAnyObjectByType<SelectionManager>();

            MarketTradeDataResolver.ResolveDefault(ref tradeRates);
        }

        void OnGUI()
        {
            GameUiInput.BeginHudLayoutFrame();

            if (selectionManager == null)
                return;

            Market market = selectionManager.SelectedMarket;
            if (market == null)
                return;

            MarketTradeData rates = MarketTradeDataResolver.ResolveDefault(ref tradeRates);
            if (rates == null)
                return;

            Rect panelRect = new Rect(Margin, Screen.height - PanelHeight - Margin, PanelWidth, PanelHeight);
            GameUiInput.ExpandHudPanelScreenRect(panelRect);
            GUI.Box(panelRect, GUIContent.none);

            GUILayout.BeginArea(panelRect);
            GUILayout.Label("Market");

            bool gameOver = GameSessionManager.IsGameOver;
            for (int i = 0; i < TradeActions.Length; i++)
            {
                MarketTradeAction action = TradeActions[i];
                bool canTrade = MarketTradeUtility.CanTrade(market, action, rates);
                GUI.enabled = canTrade && !gameOver;
                if (GUILayout.Button(MarketTradeUtility.FormatTradeButtonLabel(action, rates)))
                    CommandQueue.Enqueue(new MarketTradeCommand(market, action, rates));
            }

            GUI.enabled = true;
            GUILayout.EndArea();
        }
    }
}
