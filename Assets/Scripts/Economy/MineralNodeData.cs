using UnityEngine;

namespace AoE.RTS.Economy
{
    [CreateAssetMenu(fileName = "MineralNodeData", menuName = "AoE/Mineral Node Data")]
    public class MineralNodeData : ScriptableObject
    {
        public string displayName = "Gold Mine";
        public float initialAmount = 800f;
        public Color defaultColor = new Color(0.85f, 0.72f, 0.2f);
        public Color depletedColor = new Color(0.35f, 0.35f, 0.35f);
    }
}
