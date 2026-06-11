using AoE.RTS.Buildings;
using AoE.RTS.Core;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Economy
{
    public static class MarketTradeUtility
    {
        public static bool CanTrade(Market market, MarketTradeAction action, MarketTradeData rates)
        {
            if (market == null || !market.IsAlive || rates == null)
                return false;

            if (market.Team != UnitTeam.Player)
                return false;

            UnitTeam team = market.Team;
            float unit = rates.tradeUnitAmount;

            switch (action)
            {
                case MarketTradeAction.SellFood:
                    return ResourceManager.GetFood(team) >= unit;
                case MarketTradeAction.BuyFood:
                    return ResourceManager.GetGold(team) >= rates.buyFoodGoldCost;
                case MarketTradeAction.SellWood:
                    return ResourceManager.GetWood(team) >= unit;
                case MarketTradeAction.BuyWood:
                    return ResourceManager.GetGold(team) >= rates.buyWoodGoldCost;
                case MarketTradeAction.SellStone:
                    return ResourceManager.GetStone(team) >= unit;
                case MarketTradeAction.BuyStone:
                    return ResourceManager.GetGold(team) >= rates.buyStoneGoldCost;
                default:
                    return false;
            }
        }

        public static bool TryTrade(Market market, MarketTradeAction action, MarketTradeData rates)
        {
            if (!CanTrade(market, action, rates))
                return false;

            UnitTeam team = market.Team;
            float unit = rates.tradeUnitAmount;

            switch (action)
            {
                case MarketTradeAction.SellFood:
                    if (!ResourceManager.TrySpendFood(team, unit))
                        return false;
                    ResourceManager.AddGold(team, rates.sellFoodGoldReceived);
                    return true;
                case MarketTradeAction.BuyFood:
                    if (!ResourceManager.TrySpendGold(team, rates.buyFoodGoldCost))
                        return false;
                    ResourceManager.AddFood(team, unit);
                    return true;
                case MarketTradeAction.SellWood:
                    if (!ResourceManager.TrySpendWood(team, unit))
                        return false;
                    ResourceManager.AddGold(team, rates.sellWoodGoldReceived);
                    return true;
                case MarketTradeAction.BuyWood:
                    if (!ResourceManager.TrySpendGold(team, rates.buyWoodGoldCost))
                        return false;
                    ResourceManager.AddWood(team, unit);
                    return true;
                case MarketTradeAction.SellStone:
                    if (!ResourceManager.TrySpendStone(team, unit))
                        return false;
                    ResourceManager.AddGold(team, rates.sellStoneGoldReceived);
                    return true;
                case MarketTradeAction.BuyStone:
                    if (!ResourceManager.TrySpendGold(team, rates.buyStoneGoldCost))
                        return false;
                    ResourceManager.AddStone(team, unit);
                    return true;
                default:
                    return false;
            }
        }

        public static string FormatTradeButtonLabel(MarketTradeAction action, MarketTradeData rates)
        {
            if (rates == null)
                return action.ToString();

            int unit = Mathf.RoundToInt(rates.tradeUnitAmount);
            switch (action)
            {
                case MarketTradeAction.SellFood:
                    return Localization.Format(
                        "trade.sell_food",
                        unit,
                        Mathf.RoundToInt(rates.sellFoodGoldReceived));
                case MarketTradeAction.BuyFood:
                    return Localization.Format(
                        "trade.buy_food",
                        Mathf.RoundToInt(rates.buyFoodGoldCost),
                        unit);
                case MarketTradeAction.SellWood:
                    return Localization.Format(
                        "trade.sell_wood",
                        unit,
                        Mathf.RoundToInt(rates.sellWoodGoldReceived));
                case MarketTradeAction.BuyWood:
                    return Localization.Format(
                        "trade.buy_wood",
                        Mathf.RoundToInt(rates.buyWoodGoldCost),
                        unit);
                case MarketTradeAction.SellStone:
                    return Localization.Format(
                        "trade.sell_stone",
                        unit,
                        Mathf.RoundToInt(rates.sellStoneGoldReceived));
                case MarketTradeAction.BuyStone:
                    return Localization.Format(
                        "trade.buy_stone",
                        Mathf.RoundToInt(rates.buyStoneGoldCost),
                        unit);
                default:
                    return action.ToString();
            }
        }
    }
}
