using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AoE.RTS.Core
{
    public static class MarketTradeDataResolver
    {
        static MarketTradeData cachedDefault;

        public static MarketTradeData ResolveDefault(ref MarketTradeData cached)
        {
            if (cached != null)
                return cached;

#if UNITY_EDITOR
            cached = AssetDatabase.LoadAssetAtPath<MarketTradeData>(GameAssetPaths.DefaultMarketTradeData);
            if (cached != null)
                return cached;
#endif

            if (cachedDefault == null)
            {
                cachedDefault = ScriptableObject.CreateInstance<MarketTradeData>();
                cachedDefault.tradeUnitAmount = 100f;
                cachedDefault.sellFoodGoldReceived = 50f;
                cachedDefault.buyFoodGoldCost = 50f;
                cachedDefault.sellWoodGoldReceived = 50f;
                cachedDefault.buyWoodGoldCost = 50f;
                cachedDefault.sellStoneGoldReceived = 50f;
                cachedDefault.buyStoneGoldCost = 50f;
            }

            cached = cachedDefault;
            return cached;
        }
    }
}
