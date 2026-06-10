using System.Collections.Generic;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Buildings
{
    public class ProductionManager : MonoBehaviour, ISimulationTickable
    {
        public const int MaxQueueSize = 15;

        struct ProductionJob
        {
            public TownCenter townCenter;
            public UnitData unitData;
            public float remainingSeconds;
            public float totalSeconds;
        }

        static ProductionManager instance;
        readonly List<TownCenter> townCenters = new List<TownCenter>();
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
                if (job.townCenter == null)
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

                if (!PopulationManager.CanTrainUnit(job.townCenter.Team))
                {
                    job.remainingSeconds = 0f;
                    activeJobs[i] = job;
                    continue;
                }

                Unit unit = UnitSpawner.Spawn(
                    job.unitData,
                    job.townCenter.GetVillagerSpawnPosition(),
                    job.townCenter.Team);
                if (unit != null)
                    ProductionRallyApplier.Apply(job.townCenter, unit);
                activeJobs.RemoveAt(i);
            }
        }

        bool IsHeadJobIndex(int index)
        {
            TownCenter townCenter = activeJobs[index].townCenter;
            for (int j = 0; j < index; j++)
            {
                if (activeJobs[j].townCenter == townCenter)
                    return false;
            }

            return true;
        }

        public static void Register(TownCenter townCenter)
        {
            if (instance == null || townCenter == null)
                return;

            if (!instance.townCenters.Contains(townCenter))
                instance.townCenters.Add(townCenter);
        }

        public static void Unregister(TownCenter townCenter)
        {
            if (instance == null || townCenter == null)
                return;

            instance.townCenters.Remove(townCenter);
            instance.activeJobs.RemoveAll(job => job.townCenter == townCenter);
        }

        public static TownCenter GetTownCenterForTeam(UnitTeam team)
        {
            if (instance == null)
                return null;

            for (int i = 0; i < instance.townCenters.Count; i++)
            {
                TownCenter townCenter = instance.townCenters[i];
                if (townCenter != null && townCenter.Team == team)
                    return townCenter;
            }

            return null;
        }

        public static bool TryQueueProduction(
            TownCenter townCenter,
            UnitData unitData,
            float trainSeconds,
            float foodCost = 0f)
        {
            if (instance == null || townCenter == null || unitData == null || trainSeconds <= 0f)
                return false;

            if (GetQueueCount(townCenter) >= MaxQueueSize)
                return false;

            if (!PopulationManager.CanTrainUnit(townCenter.Team))
                return false;

            if (foodCost > 0f && !ResourceManager.TrySpendFood(townCenter.Team, foodCost))
                return false;

            instance.activeJobs.Add(new ProductionJob
            {
                townCenter = townCenter,
                unitData = unitData,
                remainingSeconds = trainSeconds,
                totalSeconds = trainSeconds
            });
            return true;
        }

        public static bool IsProducing(TownCenter townCenter)
        {
            return GetQueueCount(townCenter) > 0;
        }

        public static int GetQueueCount(TownCenter townCenter)
        {
            if (instance == null || townCenter == null)
                return 0;

            int count = 0;
            for (int i = 0; i < instance.activeJobs.Count; i++)
            {
                if (instance.activeJobs[i].townCenter == townCenter)
                    count++;
            }

            return count;
        }

        public static float GetRemainingSeconds(TownCenter townCenter)
        {
            if (instance == null || townCenter == null)
                return 0f;

            for (int i = 0; i < instance.activeJobs.Count; i++)
            {
                ProductionJob job = instance.activeJobs[i];
                if (job.townCenter == townCenter)
                    return job.remainingSeconds;
            }

            return 0f;
        }

        public static float GetTotalSeconds(TownCenter townCenter)
        {
            if (instance == null || townCenter == null)
                return 0f;

            for (int i = 0; i < instance.activeJobs.Count; i++)
            {
                ProductionJob job = instance.activeJobs[i];
                if (job.townCenter == townCenter)
                    return job.totalSeconds;
            }

            return 0f;
        }
    }
}
