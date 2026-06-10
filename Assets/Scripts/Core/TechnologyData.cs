using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Core
{
    [CreateAssetMenu(fileName = "TechnologyData", menuName = "AoE/Technology Data")]
    public class TechnologyData : ScriptableObject
    {
        public TechnologyKind kind = TechnologyKind.InfantryUpgrade;
        public string displayName = "Technology";
        public GameAge prerequisiteAge = GameAge.Feudal;
        public float foodCost = 100f;
        public float goldCost = 50f;
        public float researchTimeSeconds = 75f;
        public UnitData outputUnitData;
        public UnitData replacesUnitData;

        public float ScaledFoodCost => GameplayBalance.ScaleResourceCost(foodCost);
        public float ScaledGoldCost => GameplayBalance.ScaleResourceCost(goldCost);
        public float ScaledResearchTime => GameplayBalance.ScaleBuildTime(researchTimeSeconds);
    }
}
