using System.Collections.Generic;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Buildings
{
    public class ArcheryRangeProductionManager : MonoBehaviour, ISimulationTickable
    {
        public const int MaxQueueSize = 15;

        struct ProductionJob
        {
            public ArcheryRange archeryRange;
            public UnitData unitData;
            public float remainingSeconds;
            public float totalSeconds;
        }

        static ArcheryRangeProductionManager instance;
        readonly List<ArcheryRange> archeryRangeList = new List<ArcheryRange>();
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
                if (job.archeryRange == null)
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

                if (!PopulationManager.CanTrainUnit(job.archeryRange.Team))
                {
                    job.remainingSeconds = 0f;
                    activeJobs[i] = job;
                    continue;
                }

                Unit unit = UnitSpawner.Spawn(
                    job.unitData,
                    job.archeryRange.GetUnitSpawnPosition(),
                    job.archeryRange.Team);
                if (unit != null)
                    ProductionRallyApplier.Apply(job.archeryRange, unit);
                activeJobs.RemoveAt(i);
            }
        }

        bool IsHeadJobIndex(int index)
        {
            ArcheryRange archeryRange = activeJobs[index].archeryRange;
            for (int j = 0; j < index; j++)
            {
                if (activeJobs[j].archeryRange == archeryRange)
                    return false;
            }

            return true;
        }

        public static void Register(ArcheryRange archeryRange)
        {
            if (instance == null || archeryRange == null)
                return;

            if (!instance.archeryRangeList.Contains(archeryRange))
                instance.archeryRangeList.Add(archeryRange);
        }

        public static void Unregister(ArcheryRange archeryRange)
        {
            if (instance == null || archeryRange == null)
                return;

            instance.archeryRangeList.Remove(archeryRange);
            instance.activeJobs.RemoveAll(job => job.archeryRange == archeryRange);
        }

        public static ArcheryRange GetArcheryRangeForTeam(UnitTeam team)
        {
            if (instance == null)
                return null;

            for (int i = 0; i < instance.archeryRangeList.Count; i++)
            {
                ArcheryRange archeryRange = instance.archeryRangeList[i];
                if (archeryRange != null && archeryRange.Team == team)
                    return archeryRange;
            }

            return null;
        }

        public static bool HasArcheryRangeForTeam(UnitTeam team)
        {
            return GetArcheryRangeForTeam(team) != null;
        }

        public static bool TryQueueProduction(
            ArcheryRange archeryRange,
            UnitData unitData,
            float trainSeconds,
            float woodCost,
            float foodCost)
        {
            if (instance == null || archeryRange == null || unitData == null || trainSeconds <= 0f)
                return false;

            if (GetQueueCount(archeryRange) >= MaxQueueSize)
                return false;

            UnitTeam team = archeryRange.Team;
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
                archeryRange = archeryRange,
                unitData = unitData,
                remainingSeconds = trainSeconds,
                totalSeconds = trainSeconds
            });
            return true;
        }

        public static bool IsProducing(ArcheryRange archeryRange)
        {
            return GetQueueCount(archeryRange) > 0;
        }

        public static int GetQueueCount(ArcheryRange archeryRange)
        {
            if (instance == null || archeryRange == null)
                return 0;

            int count = 0;
            for (int i = 0; i < instance.activeJobs.Count; i++)
            {
                if (instance.activeJobs[i].archeryRange == archeryRange)
                    count++;
            }

            return count;
        }

        public static float GetRemainingSeconds(ArcheryRange archeryRange)
        {
            if (instance == null || archeryRange == null)
                return 0f;

            for (int i = 0; i < instance.activeJobs.Count; i++)
            {
                ProductionJob job = instance.activeJobs[i];
                if (job.archeryRange == archeryRange)
                    return job.remainingSeconds;
            }

            return 0f;
        }

        public static float GetTotalSeconds(ArcheryRange archeryRange)
        {
            if (instance == null || archeryRange == null)
                return 0f;

            for (int i = 0; i < instance.activeJobs.Count; i++)
            {
                ProductionJob job = instance.activeJobs[i];
                if (job.archeryRange == archeryRange)
                    return job.totalSeconds;
            }

            return 0f;
        }
    }
}
