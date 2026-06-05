using System.Collections.Generic;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Buildings
{
    public class BarracksProductionManager : MonoBehaviour
    {
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
            float woodCost)
        {
            if (instance == null || barracks == null || unitData == null || trainSeconds <= 0f)
                return false;

            if (IsProducing(barracks))
                return false;

            if (!PopulationManager.CanTrainUnit(barracks.Team))
                return false;

            if (woodCost > 0f && !ResourceManager.TrySpendWood(barracks.Team, woodCost))
                return false;

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
            if (instance == null || barracks == null)
                return false;

            for (int i = 0; i < instance.activeJobs.Count; i++)
            {
                if (instance.activeJobs[i].barracks == barracks)
                    return true;
            }

            return false;
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

        void Update()
        {
            if (GameSessionManager.IsGameOver || activeJobs.Count == 0)
                return;

            float deltaTime = Time.deltaTime;
            for (int i = activeJobs.Count - 1; i >= 0; i--)
            {
                ProductionJob job = activeJobs[i];
                if (job.barracks == null)
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
                    job.barracks.GetUnitSpawnPosition(),
                    job.barracks.Team);
                activeJobs.RemoveAt(i);
            }
        }
    }
}
