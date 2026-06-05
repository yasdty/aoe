using System.Collections.Generic;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Buildings
{
    public class ProductionManager : MonoBehaviour
    {
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

        public static bool TryQueueProduction(TownCenter townCenter, UnitData unitData, float trainSeconds)
        {
            if (instance == null || townCenter == null || unitData == null || trainSeconds <= 0f)
                return false;

            if (IsProducing(townCenter))
                return false;

            if (!PopulationManager.CanTrainUnit(townCenter.Team))
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
            if (instance == null || townCenter == null)
                return false;

            for (int i = 0; i < instance.activeJobs.Count; i++)
            {
                if (instance.activeJobs[i].townCenter == townCenter)
                    return true;
            }

            return false;
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

        void Update()
        {
            if (GameSessionManager.IsGameOver || activeJobs.Count == 0)
                return;

            float deltaTime = Time.deltaTime;
            for (int i = activeJobs.Count - 1; i >= 0; i--)
            {
                ProductionJob job = activeJobs[i];
                if (job.townCenter == null)
                {
                    activeJobs.RemoveAt(i);
                    continue;
                }

                job.remainingSeconds -= deltaTime;
                if (job.remainingSeconds > 0f)
                {
                    activeJobs[i] = job;
                    continue;
                }

                UnitSpawner.Spawn(
                    job.unitData,
                    job.townCenter.GetVillagerSpawnPosition(),
                    job.townCenter.Team);
                activeJobs.RemoveAt(i);
            }
        }
    }
}
