using UnityEngine;

namespace AoE.RTS.Economy
{
    [CreateAssetMenu(fileName = "ResourceNodeData", menuName = "AoE/Resource Node Data")]
    public class ResourceNodeData : ScriptableObject
    {
        public string displayName = "Tree";
        public float initialWood = 100f;
        public Color defaultColor = new Color(0.2f, 0.5f, 0.22f);
        public Color depletedColor = new Color(0.35f, 0.35f, 0.35f);
    }
}
