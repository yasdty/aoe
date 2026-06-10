using UnityEngine;

namespace AoE.RTS.Core
{
    [CreateAssetMenu(fileName = "CivilizationData", menuName = "AoE/Civilization Data")]
    public class CivilizationData : ScriptableObject
    {
        public string displayName = "Civilization";
        public CivilizationBonusKind bonusKind = CivilizationBonusKind.GatherRate;
        public float gatherRateMultiplier = 1.1f;
        public float infantryHpMultiplier = 1.1f;
    }
}
