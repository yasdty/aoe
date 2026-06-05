using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Economy;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Combat
{
    public class AttackManager : MonoBehaviour
    {
        struct AttackJob
        {
            public Unit attacker;
            public Unit targetUnit;
            public BuildingHealth targetBuilding;
            public float cooldownRemaining;
        }

        static AttackManager instance;
        static readonly List<Unit> gatherCancelBuffer = new List<Unit>(1);
        readonly List<AttackJob> activeJobs = new List<AttackJob>();

        void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        void Update()
        {
            if (activeJobs.Count == 0)
                return;

            float deltaTime = Time.deltaTime;
            for (int i = activeJobs.Count - 1; i >= 0; i--)
            {
                if (i >= activeJobs.Count)
                    continue;

                AttackJob job = activeJobs[i];
                if (!IsValidCombatUnit(job.attacker))
                {
                    Unit staleAttacker = job.attacker;
                    activeJobs.RemoveAt(i);
                    if (staleAttacker != null)
                        staleAttacker.NotifyStateChanged();
                    continue;
                }

                if (job.targetUnit != null)
                {
                    if (!ProcessUnitTargetJob(ref job, i, deltaTime))
                        continue;
                }
                else if (job.targetBuilding != null)
                {
                    if (!ProcessBuildingTargetJob(ref job, i, deltaTime))
                        continue;
                }
                else
                {
                    activeJobs.RemoveAt(i);
                    job.attacker.NotifyStateChanged();
                }
            }

            RefreshAttackerVisuals();
        }

        bool ProcessUnitTargetJob(ref AttackJob job, int index, float deltaTime)
        {
            if (!IsValidCombatUnit(job.targetUnit))
            {
                Unit staleAttacker = job.attacker;
                activeJobs.RemoveAt(index);
                if (staleAttacker != null)
                    staleAttacker.NotifyStateChanged();
                return false;
            }

            if (job.attacker.Team == job.targetUnit.Team)
            {
                Unit staleAttacker = job.attacker;
                activeJobs.RemoveAt(index);
                staleAttacker.NotifyStateChanged();
                return false;
            }

            return ProcessAttackCycle(
                job.attacker,
                job.targetUnit.transform.position,
                job.targetUnit.Armor,
                job.targetUnit.Team,
                job.targetUnit.Data?.displayName ?? "Unit",
                job.cooldownRemaining,
                deltaTime,
                ref job,
                index,
                unitTarget: job.targetUnit);
        }

        bool ProcessBuildingTargetJob(ref AttackJob job, int index, float deltaTime)
        {
            BuildingHealth building = job.targetBuilding;
            if (building == null || !building.IsAlive)
            {
                Unit staleAttacker = job.attacker;
                activeJobs.RemoveAt(index);
                if (staleAttacker != null)
                    staleAttacker.NotifyStateChanged();
                return false;
            }

            if (job.attacker.Team == building.Team)
            {
                Unit staleAttacker = job.attacker;
                activeJobs.RemoveAt(index);
                staleAttacker.NotifyStateChanged();
                return false;
            }

            string buildingName = building.IsTownCenter ? "Town Center" : "Building";
            Vector3 standPosition = building.GetMeleeStandPosition(job.attacker.transform.position);
            return ProcessAttackCycle(
                job.attacker,
                standPosition,
                building.Armor,
                building.Team,
                buildingName,
                job.cooldownRemaining,
                deltaTime,
                ref job,
                index,
                unitTarget: null,
                buildingTarget: building);
        }

        bool ProcessAttackCycle(
            Unit attacker,
            Vector3 targetPosition,
            float targetArmor,
            UnitTeam targetTeam,
            string targetName,
            float cooldownRemaining,
            float deltaTime,
            ref AttackJob job,
            int index,
            Unit unitTarget,
            BuildingHealth buildingTarget = null)
        {
            float attackRange = attacker.AttackRange;
            if (!attacker.IsNear(targetPosition, attackRange))
            {
                if (!JobStillActive(index, attacker))
                    return false;

                float standRadius = Mathf.Max(0.5f, attackRange * 0.85f);
                Vector3 approachPosition = UnitPositionOffsets.ApplyRingOffset(
                    targetPosition,
                    attacker,
                    standRadius);
                attacker.SetMoveTarget(approachPosition);
                job.cooldownRemaining = cooldownRemaining;
                job.attacker = attacker;
                job.targetUnit = unitTarget;
                job.targetBuilding = buildingTarget;
                activeJobs[index] = job;
                return true;
            }

            attacker.ClearMoveTarget();
            cooldownRemaining -= deltaTime;
            if (cooldownRemaining > 0f)
            {
                if (!JobStillActive(index, attacker))
                    return false;

                job.cooldownRemaining = cooldownRemaining;
                job.attacker = attacker;
                job.targetUnit = unitTarget;
                job.targetBuilding = buildingTarget;
                activeJobs[index] = job;
                return true;
            }

            float damage = Mathf.Max(1f, attacker.AttackPower - targetArmor);
            if (unitTarget != null)
                unitTarget.TakeDamage(damage);
            else if (buildingTarget != null)
                buildingTarget.TakeDamage(damage);

            // TakeDamage/Die may remove this job via CancelJobsForUnit while we are in Update.
            if (!JobStillActive(index, attacker))
                return false;

            if (!IsValidCombatUnit(attacker))
            {
                activeJobs.RemoveAt(index);
                return false;
            }

            bool targetStillAlive = unitTarget != null
                ? IsValidCombatUnit(unitTarget)
                : buildingTarget != null && buildingTarget.IsAlive;

            if (!targetStillAlive)
            {
                activeJobs.RemoveAt(index);
                attacker.NotifyStateChanged();
                return false;
            }

            job.cooldownRemaining = attacker.AttackCooldownSeconds;
            job.attacker = attacker;
            job.targetUnit = unitTarget;
            job.targetBuilding = buildingTarget;
            activeJobs[index] = job;

            if (unitTarget != null)
            {
                Debug.Log(
                    $"[{FormatTeam(attacker.Team)}] {attacker.Data?.displayName ?? "Unit"} "
                    + $"→ [{FormatTeam(targetTeam)}] {targetName}: "
                    + $"{damage:0} dmg (HP {unitTarget.CurrentHp:0}/{unitTarget.MaxHp:0})");
            }
            else if (buildingTarget != null)
            {
                Debug.Log(
                    $"[{FormatTeam(attacker.Team)}] {attacker.Data?.displayName ?? "Unit"} "
                    + $"→ [{FormatTeam(targetTeam)}] {targetName}: "
                    + $"{damage:0} dmg (HP {buildingTarget.CurrentHp:0}/{buildingTarget.MaxHp:0})");
            }

            return true;
        }

        void RefreshAttackerVisuals()
        {
            for (int i = 0; i < activeJobs.Count; i++)
            {
                Unit attacker = activeJobs[i].attacker;
                if (attacker != null)
                    attacker.NotifyStateChanged();
            }
        }

        static bool IsValidCombatUnit(Unit unit)
        {
            return unit != null && unit.IsAlive;
        }

        bool JobStillActive(int index, Unit attacker)
        {
            return index < activeJobs.Count && activeJobs[index].attacker == attacker;
        }

        public static bool IsUnitAttacking(Unit unit)
        {
            if (instance == null || unit == null)
                return false;

            for (int i = 0; i < instance.activeJobs.Count; i++)
            {
                if (instance.activeJobs[i].attacker == unit)
                    return true;
            }

            return false;
        }

        public static void IssueAttack(IReadOnlyList<Unit> attackers, Unit target)
        {
            if (instance == null || target == null || attackers == null || !target.IsAlive)
                return;

            for (int i = 0; i < attackers.Count; i++)
            {
                Unit attacker = attackers[i];
                if (!IsValidCombatUnit(attacker) || !attacker.CanAttack)
                    continue;

                if (attacker.Team == target.Team)
                    continue;

                gatherCancelBuffer.Clear();
                gatherCancelBuffer.Add(attacker);
                GatherManager.CancelForUnits(gatherCancelBuffer);
                RemoveJobsForAttacker(attacker);
                instance.activeJobs.Add(new AttackJob
                {
                    attacker = attacker,
                    targetUnit = target,
                    targetBuilding = null,
                    cooldownRemaining = 0f
                });
            }
        }

        public static void IssueAttack(IReadOnlyList<Unit> attackers, BuildingHealth targetBuilding)
        {
            if (instance == null || targetBuilding == null || attackers == null || !targetBuilding.IsAlive)
                return;

            for (int i = 0; i < attackers.Count; i++)
            {
                Unit attacker = attackers[i];
                if (!IsValidCombatUnit(attacker) || !attacker.CanAttack)
                    continue;

                if (attacker.Team == targetBuilding.Team)
                    continue;

                gatherCancelBuffer.Clear();
                gatherCancelBuffer.Add(attacker);
                GatherManager.CancelForUnits(gatherCancelBuffer);
                RemoveJobsForAttacker(attacker);
                instance.activeJobs.Add(new AttackJob
                {
                    attacker = attacker,
                    targetUnit = null,
                    targetBuilding = targetBuilding,
                    cooldownRemaining = 0f
                });
            }
        }

        public static void CancelJobsForUnit(Unit unit)
        {
            if (instance == null || unit == null)
                return;

            for (int i = instance.activeJobs.Count - 1; i >= 0; i--)
            {
                AttackJob job = instance.activeJobs[i];
                if (job.attacker == unit || job.targetUnit == unit)
                {
                    Unit staleAttacker = job.attacker;
                    instance.activeJobs.RemoveAt(i);
                    if (staleAttacker != null)
                        staleAttacker.NotifyStateChanged();
                }
            }
        }

        public static void CancelForUnits(IReadOnlyList<Unit> units)
        {
            if (instance == null || units == null || units.Count == 0)
                return;

            for (int i = instance.activeJobs.Count - 1; i >= 0; i--)
            {
                Unit attacker = instance.activeJobs[i].attacker;
                if (attacker == null)
                {
                    instance.activeJobs.RemoveAt(i);
                    continue;
                }

                for (int u = 0; u < units.Count; u++)
                {
                    if (units[u] != attacker)
                        continue;

                    instance.activeJobs.RemoveAt(i);
                    attacker.NotifyStateChanged();
                    break;
                }
            }
        }

        static void RemoveJobsForAttacker(Unit attacker)
        {
            for (int i = instance.activeJobs.Count - 1; i >= 0; i--)
            {
                if (instance.activeJobs[i].attacker == attacker)
                    instance.activeJobs.RemoveAt(i);
            }
        }

        static string FormatTeam(UnitTeam team)
        {
            return team == UnitTeam.Player ? "Player" : "CPU";
        }
    }
}
