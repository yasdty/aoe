using AoE.RTS.Core;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Buildings
{
    public enum PlacedBuildingKind
    {
        House = 0,
        Barracks = 1,
        Farm = 2,
        LumberCamp = 3,
        MiningCamp = 4,
        Mill = 5,
        ArcheryRange = 6,
        Stable = 7,
        Blacksmith = 8
    }

    [CreateAssetMenu(fileName = "PlacedBuildingData", menuName = "AoE/Placed Building Data")]
    public class PlacedBuildingData : ScriptableObject
    {
        public PlacedBuildingKind kind = PlacedBuildingKind.House;
        public string displayName = "House";
        public GameAge requiredAge = GameAge.Dark;
        public float woodCost = 25f;
        public float buildTime = 3f;
        public float footprintWidth = 4f;
        public float footprintDepth = 4f;
        public float buildingHeight = 3f;
        public int housingProvided = 5;
        public UnitData trainUnitData;
        public float trainTime = 3f;
        public float trainWoodCost = 20f;
        public float trainFoodCost;
        public UnitData secondaryTrainUnitData;
        public float secondaryTrainTime = 3f;
        public float secondaryTrainWoodCost = 25f;
        public float secondaryTrainFoodCost = 35f;
        public float spawnClearance = 4f;
        public Color defaultColor = new Color(0.82f, 0.62f, 0.35f);
        public Color ghostValidColor = new Color(0.4f, 0.85f, 0.45f);
        public Color ghostInvalidColor = new Color(0.9f, 0.3f, 0.3f);
        public Color constructionColor = new Color(0.42f, 0.4f, 0.38f);
        public Color selectedColor = new Color(0.95f, 0.85f, 0.35f);
        public float maxHp = 150f;
        public float meleeArmor;
        public float pierceArmor;
        public float foodCapacity;

        public float ScaledWoodCost => GameplayBalance.ScaleResourceCost(woodCost);
        public float ScaledBuildTime => GameplayBalance.ScaleBuildTime(buildTime);
        public float ScaledTrainTime => GameplayBalance.ScaleBuildTime(trainTime);
        public float ScaledTrainWoodCost => GameplayBalance.ScaleResourceCost(trainWoodCost);
        public float ScaledTrainFoodCost => GameplayBalance.ScaleResourceCost(trainFoodCost);
        public float ScaledSecondaryTrainTime => GameplayBalance.ScaleBuildTime(secondaryTrainTime);
        public float ScaledSecondaryTrainWoodCost => GameplayBalance.ScaleResourceCost(secondaryTrainWoodCost);
        public float ScaledSecondaryTrainFoodCost => GameplayBalance.ScaleResourceCost(secondaryTrainFoodCost);
    }
}
