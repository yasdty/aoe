using UnityEngine;

namespace AoE.RTS.Core
{
    [CreateAssetMenu(fileName = "MarketTradeData", menuName = "AoE/Market Trade Data")]
    public class MarketTradeData : ScriptableObject
    {
        public float tradeUnitAmount = 100f;
        public float sellFoodGoldReceived = 50f;
        public float buyFoodGoldCost = 50f;
        public float sellWoodGoldReceived = 50f;
        public float buyWoodGoldCost = 50f;
        public float sellStoneGoldReceived = 50f;
        public float buyStoneGoldCost = 50f;
    }
}
