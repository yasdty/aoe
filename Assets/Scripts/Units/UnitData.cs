using AoE.RTS.Combat;
using UnityEngine;

namespace AoE.RTS.Units
{
    [CreateAssetMenu(fileName = "UnitData", menuName = "AoE/Unit Data")]
    public class UnitData : ScriptableObject
    {
        public string displayName = "Unit";
        public float maxHp = 100f;
        public float moveSpeed = 5f;
        public float attack;
        public AttackDamageType attackDamageType = AttackDamageType.Melee;
        public float meleeArmor;
        public float pierceArmor;
        public UnitArmorClass armorClass = UnitArmorClass.None;
        public float attackRange = 1.5f;
        public float attackCooldown = 1f;
        public UnitTeam team = UnitTeam.Player;
        public Color defaultColor = new Color(0.2f, 0.45f, 0.85f);
        public Color selectedColor = new Color(0.2f, 0.85f, 0.35f);

        public bool CanAttack => attack > 0f;
    }
}
