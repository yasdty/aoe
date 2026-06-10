using AoE.RTS.Combat;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Core
{
    public static class CivilizationBonusUtility
    {
        static bool loggedPlayerGatherBonus;
        static bool loggedEnemyGatherBonus;
        static bool loggedPlayerInfantryHpBonus;
        static bool loggedEnemyInfantryHpBonus;

        public static float GetGatherRateMultiplier(UnitTeam team)
        {
            CivilizationData civilization = GameSessionManager.GetCivilization(team);
            if (civilization == null || civilization.bonusKind != CivilizationBonusKind.GatherRate)
                return 1f;

            return Mathf.Max(0.01f, civilization.gatherRateMultiplier);
        }

        public static float ScaleGatherRate(UnitTeam team, float baseRate)
        {
            return baseRate * GetGatherRateMultiplier(team);
        }

        public static void LogFirstGatherIfNeeded(UnitTeam team)
        {
            float multiplier = GetGatherRateMultiplier(team);
            if (multiplier <= 1f)
                return;

            if (team == UnitTeam.Enemy)
            {
                if (loggedEnemyGatherBonus)
                    return;

                loggedEnemyGatherBonus = true;
            }
            else
            {
                if (loggedPlayerGatherBonus)
                    return;

                loggedPlayerGatherBonus = true;
            }

            CivilizationData civilization = GameSessionManager.GetCivilization(team);
            string name = civilization != null ? civilization.displayName : team.ToString();
            Debug.Log($"[Civilization] {name} gather x{multiplier:0.00}");
        }

        public static bool QualifiesForInfantryHpBonus(UnitData data)
        {
            if (data == null || !data.CanAttack)
                return false;

            return data.armorClass == UnitArmorClass.Infantry
                && data.attackDamageType == AttackDamageType.Melee;
        }

        public static float GetScaledMaxHp(UnitData data, UnitTeam team)
        {
            float baseHp = data != null ? data.maxHp : 100f;
            CivilizationData civilization = GameSessionManager.GetCivilization(team);
            if (civilization == null || civilization.bonusKind != CivilizationBonusKind.InfantryHp)
                return baseHp;

            if (!QualifiesForInfantryHpBonus(data))
                return baseHp;

            float multiplier = Mathf.Max(0.01f, civilization.infantryHpMultiplier);
            LogFirstInfantryHpIfNeeded(team, civilization, multiplier);
            return baseHp * multiplier;
        }

        static void LogFirstInfantryHpIfNeeded(UnitTeam team, CivilizationData civilization, float multiplier)
        {
            if (team == UnitTeam.Enemy)
            {
                if (loggedEnemyInfantryHpBonus)
                    return;

                loggedEnemyInfantryHpBonus = true;
            }
            else
            {
                if (loggedPlayerInfantryHpBonus)
                    return;

                loggedPlayerInfantryHpBonus = true;
            }

            string name = civilization != null ? civilization.displayName : team.ToString();
            Debug.Log($"[Civilization] {name} infantry HP x{multiplier:0.00}");
        }

        public static string GetHudLabel(UnitTeam team)
        {
            CivilizationData civilization = GameSessionManager.GetCivilization(team);
            if (civilization == null)
                return string.Empty;

            switch (civilization.bonusKind)
            {
                case CivilizationBonusKind.GatherRate:
                {
                    float multiplier = GetGatherRateMultiplier(team);
                    if (multiplier <= 1f)
                        return $"Civ: {civilization.displayName}";
                    return $"Civ: {civilization.displayName} — Gather x{multiplier:0.00}";
                }
                case CivilizationBonusKind.InfantryHp:
                {
                    float multiplier = Mathf.Max(0.01f, civilization.infantryHpMultiplier);
                    if (multiplier <= 1f)
                        return $"Civ: {civilization.displayName}";
                    return $"Civ: {civilization.displayName} — Inf HP x{multiplier:0.00}";
                }
                default:
                    return $"Civ: {civilization.displayName}";
            }
        }
    }
}
