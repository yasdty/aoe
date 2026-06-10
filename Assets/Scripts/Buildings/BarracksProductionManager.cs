using System.Collections.Generic;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Buildings
{
    public class BarracksProductionManager : MonoBehaviour, ISimulationTickable
    {
        public const int MaxQueueSize = 15;

        struct ProductionJob
        {
            public Barracks barracks;
            public UnitData unitData;
            public float remainingSeconds;
            public float totalSeconds;
        }

        static BarracksProductionManager instance;
        readonly List<Barracks> barracksList = new List<Barracks>();
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
                if (job.barracks == null)
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

                if (!PopulationManager.CanTrainUnit(job.barracks.Team))
                {
                    job.remainingSeconds = 0f;
                    activeJobs[i] = job;
                    continue;
                }

                Unit unit = UnitSpawner.Spawn(
                    job.unitData,
                    job.barracks.GetUnitSpawnPosition(),
                    job.barracks.Team);
                if (unit != null)
                    ProductionRallyApplier.Apply(job.barracks, unit);
                activeJobs.RemoveAt(i);
            }
        }

        bool IsHeadJobIndex(int index)
        {
            Barracks barracks = activeJobs[index].barracks;
            for (int j = 0; j < index; j++)
            {
                if (activeJobs[j].barracks == barracks)
                    return false;
            }

            return true;
        }

        public static void Register(Barracks barracks)
        {
            if (instance == null || barracks == null)
                return;

            if (!instance.barracksList.Contains(barracks))
                instance.barracksList.Add(barracks);
        }

        public static void Unregister(Barracks barracks)
        {
            if (instance == null || barracks == null)
                return;

            instance.barracksList.Remove(barracks);
            instance.activeJobs.RemoveAll(job => job.barracks == barracks);
        }

        public static Barracks GetBarracksForTeam(UnitTeam team)
        {
            if (instance == null)
                return null;

            for (int i = 0; i < instance.barracksList.Count; i++)
            {
                Barracks barracks = instance.barracksList[i];
                if (barracks != null && barracks.Team == team)
                    return barracks;
            }

            return null;
        }

        public static bool HasBarracksForTeam(UnitTeam team)
        {
            return GetBarracksForTeam(team) != null;
        }

        public static bool TryQueueProduction(
            Barracks barracks,
            UnitData unitData,
            float trainSeconds,
            float woodCost,
            float foodCost = 0f)
        {
            if (instance == null || barracks == null || unitData == null || trainSeconds <= 0f)
                return false;

            if (GetQueueCount(barracks) >= MaxQueueSize)
                return false;

            UnitTeam team = barracks.Team;
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
                barracks = barracks,
                unitData = unitData,
                remainingSeconds = trainSeconds,
                totalSeconds = trainSeconds
            });
            return true;
        }

        public static bool IsProducing(Barracks barracks)
        {
            return GetQueueCount(barracks) > 0;
        }

        public static int GetQueueCount(Barracks barracks)
        {
            if (instance == null || barracks == null)
                return 0;

            int count = 0;
            for (int i = 0; i < instance.activeJobs.Count; i++)
            {
                if (instance.activeJobs[i].barracks == barracks)
                    count++;
            }

            return count;
        }

        public static float GetRemainingSeconds(Barracks barracks)
        {
            if (instance == null || barracks == null)
                return 0f;

            for (int i = 0; i < instance.activeJobs.Count; i++)
            {
                ProductionJob job = instance.activeJobs[i];
                if (job.barracks == barracks)
                    return job.remainingSeconds;
            }

            return 0f;
        }

        public static float GetTotalSeconds(Barracks barracks)
        {
            if (instance == null || barracks == null)
                return 0f;

            for (int i = 0; i < instance.activeJobs.Count; i++)
            {
                ProductionJob job = instance.activeJobs[i];
                if (job.barracks == barracks)
                    return job.totalSeconds;
            }

            return 0f;
        }
    }
}
