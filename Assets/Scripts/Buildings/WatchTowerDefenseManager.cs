using System.Collections.Generic;
using AoE.RTS.Combat;
using AoE.RTS.Core;
using AoE.RTS.Spatial;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Buildings
{
    public class WatchTowerDefenseManager : MonoBehaviour, ISimulationTickable
    {
        struct TowerCombatState
        {
            public WatchTower tower;
            public float cooldownRemaining;
        }

        static WatchTowerDefenseManager instance;
        readonly List<TowerCombatState> towers = new List<TowerCombatState>();

        void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;

            SimulationTick.Unregister(this);
        }

        void Start()
        {
            SimulationTick.Register(this);
        }

        public void TickSimulation(float fixedDeltaTime)
        {
            for (int i = towers.Count - 1; i >= 0; i--)
            {
                TowerCombatState state = towers[i];
                WatchTower tower = state.tower;
                if (tower == null || !tower.IsAlive || tower.Data == null)
                {
                    towers.RemoveAt(i);
                    continue;
                }

                state.cooldownRemaining = Mathf.Max(0f, state.cooldownRemaining - fixedDeltaTime);
                if (state.cooldownRemaining > 0f)
                {
                    towers[i] = state;
                    continue;
                }

                if (TryAttackNearestEnemy(tower))
                    state.cooldownRemaining = tower.Data.towerAttackCooldown;

                towers[i] = state;
            }
        }

        static bool TryAttackNearestEnemy(WatchTower tower)
        {
            UnitTeam enemyTeam = tower.Team == UnitTeam.Enemy ? UnitTeam.Player : UnitTeam.Enemy;
            Unit target = UnitSpatialIndex.FindNearestUnit(tower.transform.position, enemyTeam);
            if (target == null || !target.IsAlive)
                return false;

            float range = tower.Data.towerAttackRange;
            Vector3 towerPosition = tower.transform.position;
            Vector3 targetPosition = target.transform.position;
            towerPosition.y = 0f;
            targetPosition.y = 0f;
            if (Vector3.SqrMagnitude(targetPosition - towerPosition) > range * range)
                return false;

            float distance = Vector3.Distance(towerPosition, targetPosition);
            CombatDamageBreakdown breakdown = CombatDamageResolver.ResolvePierceAttack(
                tower.Data.towerAttack,
                target);
            target.TakeDamage(breakdown.totalDamage);

            bool targetWasKilled = !target.IsAlive;
            CombatFeedbackBus.Raise(new CombatFeedbackEvent
            {
                sourceWorldPosition = tower.transform.position + Vector3.up * 4f,
                targetWorldPosition = target.transform.position + Vector3.up * 1f,
                kind = CombatFeedbackKind.RangedHit,
                targetWasKilled = targetWasKilled
            });

            string towerName = tower.Data.displayName ?? "Watch Tower";
            string targetName = target.Data?.displayName ?? "Unit";
            Debug.Log(
                $"[WatchTower] [{FormatTeam(tower.Team)}] {towerName} "
                + $"→ [{FormatTeam(enemyTeam)}] {targetName}: "
                + $"{breakdown.FormatLogSuffix()} (HP {target.CurrentHp:0}/{target.MaxHp:0}, "
                + $"range {distance:0.0}m/{range:0.0}m)");

            return true;
        }

        static string FormatTeam(UnitTeam team)
        {
            return team == UnitTeam.Player ? "Player" : "CPU";
        }

        public static void Register(WatchTower tower)
        {
            if (instance == null || tower == null)
                return;

            for (int i = 0; i < instance.towers.Count; i++)
            {
                if (instance.towers[i].tower == tower)
                    return;
            }

            instance.towers.Add(new TowerCombatState { tower = tower });
        }

        public static void Unregister(WatchTower tower)
        {
            if (instance == null || tower == null)
                return;

            instance.towers.RemoveAll(state => state.tower == tower);
        }
    }
}
