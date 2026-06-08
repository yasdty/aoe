using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Buildings
{
    public enum PlacedBuildingKind
    {
        House = 0,
        Barracks = 1,
        Farm = 2,
        LumberCamp = 3
    }

    [CreateAssetMenu(fileName = "PlacedBuildingData", menuName = "AoE/Placed Building Data")]
    public class PlacedBuildingData : ScriptableObject
    {
        public PlacedBuildingKind kind = PlacedBuildingKind.House;
        public string displayName = "House";
        public float woodCost = 25f;
        public float buildTime = 3f;
        public float footprintWidth = 4f;
        public float footprintDepth = 4f;
        public float buildingHeight = 3f;
        public int housingProvided = 5;
        public UnitData trainUnitData;
        public float trainTime = 3f;
        public float trainWoodCost = 20f;
        public float spawnClearance = 4f;
        public Color defaultColor = new Color(0.82f, 0.62f, 0.35f);
        public Color ghostValidColor = new Color(0.4f, 0.85f, 0.45f);
        public Color ghostInvalidColor = new Color(0.9f, 0.3f, 0.3f);
        public Color constructionColor = new Color(0.42f, 0.4f, 0.38f);
        public Color selectedColor = new Color(0.95f, 0.85f, 0.35f);
        public float maxHp = 150f;
        public float armor;
        public float foodCapacity;
    }
}
