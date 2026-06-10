using UnityEngine;

namespace AoE.RTS.Core
{
    [CreateAssetMenu(fileName = "AgeData", menuName = "AoE/Age Data")]
    public class AgeData : ScriptableObject
    {
        public GameAge targetAge = GameAge.Feudal;
        public string displayName = "Feudal Age";
        public float upgradeFoodCost = 500f;
        public float upgradeGoldCost = 300f;
    }
}
