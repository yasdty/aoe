using System.Collections.Generic;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Buildings
{
    public class StableProductionManager : MonoBehaviour, ISimulationTickable
    {
        public const int MaxQueueSize = 15;

        struct ProductionJob
        {
            public Stable stable;
            public UnitData unitData;
            public float remainingSeconds;
            public float totalSeconds;
        }

        static StableProductionManager instance;
        readonly List<Stable> stableList = new List<Stable>();
        readonly List<ProductionJob> activeJobs = new List<ProductionJob>();

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
            if (activeJobs.Count == 0)
                return;

            for (int i = activeJobs.Count - 1; i >= 0; i--)
            {
                ProductionJob job = activeJobs[i];
                if (job.stable == null)
                {
                    activeJobs.RemoveAt(i);
                    continue;
                }

                if (!IsHeadJobIndex(i))
                    continue;

                job.remainingSeconds -= fixedDeltaTime;
                if (job.remainingSeconds > 0f)
                {
                    activeJobs[i] = job;
                    continue;
                }

                Unit unit = UnitSpawner.Spawn(
                    job.unitData,
                    job.stable.GetUnitSpawnPosition(),
                    job.stable.Team);
                if (unit != null)
                    ProductionRallyApplier.Apply(job.stable, unit);
                activeJobs.RemoveAt(i);
            }
        }

        bool IsHeadJobIndex(int index)
        {
            Stable stable = activeJobs[index].stable;
            for (int j = 0; j < index; j++)
            {
                if (activeJobs[j].stable == stable)
                    return false;
            }

            return true;
        }

        public static void Register(Stable stable)
        {
            if (instance == null || stable == null)
                return;

            if (!instance.stableList.Contains(stable))
                instance.stableList.Add(stable);
        }

        public static void Unregister(Stable stable)
        {
            if (instance == null || stable == null)
                return;

            instance.stableList.Remove(stable);
            instance.activeJobs.RemoveAll(job => job.stable == stable);
        }

        public static Stable GetStableForTeam(UnitTeam team)
        {
            if (instance == null)
                return null;

            for (int i = 0; i < instance.stableList.Count; i++)
            {
                Stable stable = instance.stableList[i];
                if (stable != null && stable.Team == team)
                    return stable;
            }

            return null;
        }

        public static bool HasStableForTeam(UnitTeam team)
        {
            return GetStableForTeam(team) != null;
        }

        public static bool TryQueueProduction(
            Stable stable,
            UnitData unitData,
            float trainSeconds,
            float woodCost,
            float foodCost)
        {
            if (instance == null || stable == null || unitData == null || trainSeconds <= 0f)
                return false;

            if (GetQueueCount(stable) >= MaxQueueSize)
                return false;

            UnitTeam team = stable.Team;
            if (!PopulationManager.CanTrainUnit(team))
                return false;

            if (woodCost > 0f && ResourceManager.GetWood(team) < woodCost)
                return false;

            if (foodCost > 0f && ResourceManager.GetFood(team) < foodCost)
                return false;

            if (woodCost > 0f && !ResourceManager.TrySpendWood(team, woodCost))
                return false;

            if (foodCost > 0f && !ResourceManager.TrySpendFood(team, foodCost))
            {
                if (woodCost > 0f)
                    ResourceManager.AddWood(team, woodCost);
                return false;
            }

            instance.activeJobs.Add(new ProductionJob
            {
                stable = stable,
                unitData = unitData,
                remainingSeconds = trainSeconds,
                totalSeconds = trainSeconds
            });
            return true;
        }

        public static bool IsProducing(Stable stable)
        {
            return GetQueueCount(stable) > 0;
        }

        public static int GetQueueCount(Stable stable)
        {
            if (instance == null || stable == null)
                return 0;

            int count = 0;
            for (int i = 0; i < instance.activeJobs.Count; i++)
            {
                if (instance.activeJobs[i].stable == stable)
                    count++;
            }

            return count;
        }

        public static float GetRemainingSeconds(Stable stable)
        {
            if (instance == null || stable == null)
                return 0f;

            for (int i = 0; i < instance.activeJobs.Count; i++)
            {
                ProductionJob job = instance.activeJobs[i];
                if (job.stable == stable)
                    return job.remainingSeconds;
            }

            return 0f;
        }

        public static float GetTotalSeconds(Stable stable)
        {
            if (instance == null || stable == null)
                return 0f;

            for (int i = 0; i < instance.activeJobs.Count; i++)
            {
                ProductionJob job = instance.activeJobs[i];
                if (job.stable == stable)
                    return job.totalSeconds;
            }

            return 0f;
        }
    }
}
