using AoE.RTS.Buildings;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Combat
{
    public struct CombatDamageBreakdown
    {
        public AttackDamageType damageType;
        public float attackPower;
        public float armorApplied;
        public float damageAfterArmor;
        public float bonusDamage;
        public float totalDamage;

        public string FormatLogSuffix()
        {
            string typeLabel = damageType == AttackDamageType.Pierce ? "Pierce" : "Melee";
            if (bonusDamage > 0f)
                return $"{totalDamage:0} ({damageAfterArmor:0}+{bonusDamage:0}) ({typeLabel})";

            return $"{totalDamage:0} ({typeLabel})";
        }
    }

    public static class CombatDamageResolver
    {
        struct BonusEntry
        {
            public string attackerName;
            public UnitArmorClass targetClass;
            public float bonus;
        }

        static readonly BonusEntry[] BonusTable =
        {
            new BonusEntry { attackerName = "Spearman", targetClass = UnitArmorClass.Cavalry, bonus = 12f }
        };

        public static CombatDamageBreakdown Resolve(Unit attacker, Unit target)
        {
            if (attacker == null || target == null)
                return default;

            AttackDamageType damageType = attacker.AttackDamageType;
            float attackPower = attacker.AttackPower;
            float armor = GetArmorForDamageType(target.MeleeArmor, target.PierceArmor, damageType);
            float bonus = GetBonusDamage(attacker.Data?.displayName, target.ArmorClass);
            return BuildBreakdown(damageType, attackPower, armor, bonus);
        }

        public static CombatDamageBreakdown Resolve(Unit attacker, BuildingHealth target)
        {
            if (attacker == null || target == null)
                return default;

            AttackDamageType damageType = attacker.AttackDamageType;
            float attackPower = attacker.AttackPower;
            float armor = GetArmorForDamageType(target.MeleeArmor, target.PierceArmor, damageType);
            return BuildBreakdown(damageType, attackPower, armor, bonus: 0f);
        }

        public static CombatDamageBreakdown ResolveMeleeAttack(float attackPower, Unit target)
        {
            if (target == null)
                return default;

            float armor = target.MeleeArmor;
            return BuildBreakdown(AttackDamageType.Melee, attackPower, armor, bonus: 0f);
        }

        public static CombatDamageBreakdown ResolvePierceAttack(float attackPower, Unit target)
        {
            if (target == null)
                return default;

            float armor = target.PierceArmor;
            return BuildBreakdown(AttackDamageType.Pierce, attackPower, armor, bonus: 0f);
        }

        static CombatDamageBreakdown BuildBreakdown(
            AttackDamageType damageType,
            float attackPower,
            float armor,
            float bonus)
        {
            float afterArmor = Mathf.Max(1f, attackPower - armor);
            float total = afterArmor + bonus;
            return new CombatDamageBreakdown
            {
                damageType = damageType,
                attackPower = attackPower,
                armorApplied = armor,
                damageAfterArmor = afterArmor,
                bonusDamage = bonus,
                totalDamage = total
            };
        }

        static float GetArmorForDamageType(float meleeArmor, float pierceArmor, AttackDamageType damageType)
        {
            return damageType == AttackDamageType.Pierce ? pierceArmor : meleeArmor;
        }

        static float GetBonusDamage(string attackerDisplayName, UnitArmorClass targetClass)
        {
            if (string.IsNullOrEmpty(attackerDisplayName) || targetClass == UnitArmorClass.None)
                return 0f;

            for (int i = 0; i < BonusTable.Length; i++)
            {
                BonusEntry entry = BonusTable[i];
                if (entry.attackerName == attackerDisplayName && entry.targetClass == targetClass)
                    return entry.bonus;
            }

            return 0f;
        }
    }
}
