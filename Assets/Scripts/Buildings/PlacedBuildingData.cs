using UnityEngine;

namespace AoE.RTS.Buildings
{
    [CreateAssetMenu(fileName = "PlacedBuildingData", menuName = "AoE/Placed Building Data")]
    public class PlacedBuildingData : ScriptableObject
    {
        public string displayName = "House";
        public float woodCost = 25f;
        public float buildTime = 3f;
        public float footprintWidth = 4f;
        public float footprintDepth = 4f;
        public float buildingHeight = 3f;
        public Color defaultColor = new Color(0.82f, 0.62f, 0.35f);
        public Color ghostValidColor = new Color(0.4f, 0.85f, 0.45f);
        public Color ghostInvalidColor = new Color(0.9f, 0.3f, 0.3f);
        public Color constructionColor = new Color(0.42f, 0.4f, 0.38f);
    }
}
