using System.Collections.Generic;
using AoE.RTS.Core;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Economy
{
    public class PassiveAnimalLocomotionManager : MonoBehaviour, ISimulationTickable
    {
        const float DeerWanderSpeed = 1.5f;
        const float DeerWanderRadiusMin = 2f;
        const float DeerWanderRadiusMax = 4f;
        const float DeerPauseMinSeconds = 2f;
        const float DeerPauseMaxSeconds = 4f;
        const float SheepFollowRadius = 5f;
        const float SheepFollowStopRadius = 2.5f;

        struct DeerWanderState
        {
            public DeerResource deer;
            public float pauseRemaining;
            public Vector3 wanderTarget;
            public bool hasWanderTarget;
        }

        readonly List<DeerWanderState> deerStates = new List<DeerWanderState>();
        readonly List<Unit> unitBuffer = new List<Unit>();

        void OnDestroy()
        {
            SimulationTick.Unregister(this);
        }

        void Start()
        {
            SimulationTick.Register(this);
            SyncDeerStates();
        }

        public void TickSimulation(float fixedDeltaTime)
        {
            SyncDeerStates();
            TickDeerWander(fixedDeltaTime);
            TickSheepLocomotion(fixedDeltaTime);
        }

        void SyncDeerStates()
        {
            IReadOnlyList<DeerResource> deerList = DeerRegistry.All;
            for (int i = deerStates.Count - 1; i >= 0; i--)
            {
                DeerResource deer = deerStates[i].deer;
                if (deer == null || !deer.gameObject.activeInHierarchy)
                    deerStates.RemoveAt(i);
            }

            for (int i = 0; i < deerList.Count; i++)
            {
                DeerResource deer = deerList[i];
                if (deer == null || HasDeerState(deer))
                    continue;

                deerStates.Add(new DeerWanderState
                {
                    deer = deer,
                    pauseRemaining = Random.Range(0f, 1.5f),
                    hasWanderTarget = false
                });
            }
        }

        bool HasDeerState(DeerResource deer)
        {
            for (int i = 0; i < deerStates.Count; i++)
            {
                if (deerStates[i].deer == deer)
                    return true;
            }

            return false;
        }

        void TickDeerWander(float deltaTime)
        {
            for (int i = 0; i < deerStates.Count; i++)
            {
                DeerWanderState state = deerStates[i];
                DeerResource deer = state.deer;
                if (deer == null || deer.IsDepleted || FoodGatherManager.IsAnimalBeingHunted(deer))
                    continue;

                if (state.pauseRemaining > 0f)
                {
                    state.pauseRemaining -= deltaTime;
                    deerStates[i] = state;
                    continue;
                }

                if (!state.hasWanderTarget)
                {
                    state.wanderTarget = PickDeerWanderTarget(deer.transform.position);
                    state.hasWanderTarget = true;
                }

                if (MoveTransformToward(deer.transform, state.wanderTarget, DeerWanderSpeed, deltaTime))
                {
                    state.hasWanderTarget = false;
                    state.pauseRemaining = Random.Range(DeerPauseMinSeconds, DeerPauseMaxSeconds);
                }

                deerStates[i] = state;
            }
        }

        static Vector3 PickDeerWanderTarget(Vector3 center)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(DeerWanderRadiusMin, DeerWanderRadiusMax);
            Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            Vector3 target = center + offset;
            target.y = center.y;
            return target;
        }

        void TickSheepLocomotion(float deltaTime)
        {
            IReadOnlyList<SheepResource> sheepList = SheepRegistry.All;
            UnitManager.CopyUnitsTo(unitBuffer);

            for (int i = 0; i < sheepList.Count; i++)
            {
                SheepResource sheep = sheepList[i];
                if (sheep == null || sheep.IsDepleted || sheep.IsNeutral)
                    continue;

                if (sheep.HasMoveTarget)
                {
                    sheep.TickMovement(deltaTime);
                    continue;
                }

                Unit followTarget = FindFollowTarget(sheep, unitBuffer);
                if (followTarget == null)
                    continue;

                Vector3 followPosition = followTarget.transform.position;
                followPosition.y = sheep.transform.position.y;
                if (HorizontalDistance(sheep.transform.position, followPosition) <= SheepFollowStopRadius)
                    continue;

                MoveTransformToward(sheep.transform, followPosition, sheep.MoveSpeed * 0.65f, deltaTime);
            }
        }

        static Unit FindFollowTarget(SheepResource sheep, List<Unit> units)
        {
            Unit best = null;
            float bestDistanceSq = SheepFollowRadius * SheepFollowRadius;
            Vector3 sheepPosition = sheep.transform.position;
            sheepPosition.y = 0f;

            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                if (unit == null || !unit.IsAlive || unit.CanAttack || unit.Team != sheep.OwnerTeam)
                    continue;

                Vector3 unitPosition = unit.transform.position;
                unitPosition.y = 0f;
                float distanceSq = (unitPosition - sheepPosition).sqrMagnitude;
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                best = unit;
            }

            return best;
        }

        static bool MoveTransformToward(Transform transform, Vector3 target, float speed, float deltaTime)
        {
            Vector3 position = transform.position;
            Vector3 toTarget = target - position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;
            float step = speed * deltaTime;
            if (distance <= step)
            {
                transform.position = new Vector3(target.x, position.y, target.z);
                return true;
            }

            transform.position = position + toTarget / distance * step;
            return false;
        }

        static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
