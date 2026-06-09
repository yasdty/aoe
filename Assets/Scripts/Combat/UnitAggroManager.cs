using System.Collections.Generic;
using AoE.RTS.Core;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Combat
{
    public class UnitAggroManager : MonoBehaviour, ISimulationTickable
    {
        const float MinAggroDetectRange = 5f;
        const float AggroDetectRangeBonus = 2f;

        static readonly List<Unit> unitBuffer = new List<Unit>(32);
        static readonly List<Unit> aggroIssueBuffer = new List<Unit>(1);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void EnsureExistsInPlayMode()
        {
            if (!Application.isPlaying || Object.FindAnyObjectByType<UnitAggroManager>() != null)
                return;

            GameObject systems = GameObject.Find("Systems");
            Transform parent = systems != null ? systems.transform : null;
            GameObject aggroObject = new GameObject("UnitAggroManager");
            if (parent != null)
                aggroObject.transform.SetParent(parent);

            aggroObject.AddComponent<UnitAggroManager>();
            Debug.Log("[AutoAggro] UnitAggroManager was missing from the scene — created at runtime.");
        }

        void OnDestroy()
        {
            SimulationTick.Unregister(this);
        }

        void Start()
        {
            SimulationTick.Register(this);
        }

        public void TickSimulation(float fixedDeltaTime)
        {
            UnitManager.CopyUnitsTo(unitBuffer);
            for (int i = 0; i < unitBuffer.Count; i++)
            {
                Unit unit = unitBuffer[i];
                if (!CanAggro(unit))
                    continue;

                float detectRange = GetAggroDetectRange(unit);
                Unit enemy = FindAggroTarget(unit, detectRange);
                if (enemy == null)
                    continue;

                aggroIssueBuffer.Clear();
                aggroIssueBuffer.Add(unit);
                AttackManager.IssueAttack(aggroIssueBuffer, enemy);

                float distance = HorizontalDistance(unit.transform.position, enemy.transform.position);
                Debug.Log(
                    $"[AutoAggro] [{FormatTeam(unit.Team)}] {unit.Data?.displayName ?? "Unit"} "
                    + $"→ [{FormatTeam(enemy.Team)}] {enemy.Data?.displayName ?? "Unit"} "
                    + $"(distance {distance:0.0}m / detect {detectRange:0.0}m, melee {unit.AttackRange:0.0}m)");
            }
        }

        static bool CanAggro(Unit unit)
        {
            if (unit == null || !unit.IsAlive || !unit.CanAttack)
                return false;

            if (unit.HasMoveTarget)
                return false;

            if (AttackManager.IsUnitAttacking(unit))
                return false;

            if (BoarAttackManager.IsUnitAttackingBoar(unit))
                return false;

            return true;
        }

        public static float GetAggroDetectRange(Unit unit)
        {
            if (unit == null)
                return MinAggroDetectRange;

            return Mathf.Max(unit.AttackRange + AggroDetectRangeBonus, MinAggroDetectRange);
        }

        static Unit FindAggroTarget(Unit unit, float detectRange)
        {
            UnitTeam enemyTeam = unit.Team == UnitTeam.Player ? UnitTeam.Enemy : UnitTeam.Player;
            float detectRangeSq = detectRange * detectRange;
            Vector3 unitPosition = unit.transform.position;
            unitPosition.y = 0f;

            Unit best = null;
            float bestDistanceSq = detectRangeSq;

            for (int i = 0; i < unitBuffer.Count; i++)
            {
                Unit candidate = unitBuffer[i];
                if (candidate == null || !candidate.IsAlive || candidate.Team != enemyTeam)
                    continue;

                Vector3 candidatePosition = candidate.transform.position;
                candidatePosition.y = 0f;
                float distanceSq = (candidatePosition - unitPosition).sqrMagnitude;
                if (distanceSq > bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                best = candidate;
            }

            return best;
        }

        static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }

        static string FormatTeam(UnitTeam team)
        {
            return team == UnitTeam.Player ? "Player" : "CPU";
        }
    }
}
