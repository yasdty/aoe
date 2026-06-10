using AoE.RTS.Core;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Buildings
{
    [CreateAssetMenu(fileName = "BuildingData", menuName = "AoE/Building Data")]
    public class BuildingData : ScriptableObject
    {
        public string displayName = "Town Center";
        public float villagerTrainTime = 3f;
        public float villagerFoodCost = 50f;
        public UnitData villagerUnitData;
        public Color defaultColor = new Color(0.75f, 0.65f, 0.45f);
        public Color selectedColor = new Color(0.95f, 0.85f, 0.35f);
        public float spawnForwardOffset = 8f;
        public float spawnClearance = 4f;
        public float maxHp = 400f;
        public float meleeArmor;
        public float pierceArmor;

        public float ScaledVillagerTrainTime => GameplayBalance.ScaleBuildTime(villagerTrainTime);
        public float ScaledVillagerFoodCost => GameplayBalance.ScaleResourceCost(villagerFoodCost);
    }
}
