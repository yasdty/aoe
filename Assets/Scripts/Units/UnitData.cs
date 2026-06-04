using UnityEngine;

namespace AoE.RTS.Units
{
    [CreateAssetMenu(fileName = "UnitData", menuName = "AoE/Unit Data")]
    public class UnitData : ScriptableObject
    {
        public string displayName = "Unit";
        public float maxHp = 100f;
        public float moveSpeed = 5f;
        public Color defaultColor = new Color(0.2f, 0.45f, 0.85f);
        public Color selectedColor = new Color(0.2f, 0.85f, 0.35f);
    }
}
