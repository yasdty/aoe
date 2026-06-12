using UnityEngine;

namespace AoE.RTS.Economy
{
    [CreateAssetMenu(fileName = "FoodNodeData", menuName = "AoE/Food Node Data")]
    public class FoodNodeData : ScriptableObject
    {
        public string displayName = "Berry Bush";
        public float initialFood = 125f;
        public Color defaultColor = new Color(0.55f, 0.15f, 0.45f);
        public Color depletedColor = new Color(0.35f, 0.35f, 0.35f);
    }
}
