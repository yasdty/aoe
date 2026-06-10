using System.Collections.Generic;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Buildings
{
    public class BlacksmithResearchManager : MonoBehaviour, ISimulationTickable
    {
        struct ResearchJob
        {
            public Blacksmith blacksmith;
            public TechnologyData technology;
            public float remainingSeconds;
            public float totalSeconds;
        }

        static BlacksmithResearchManager instance;
        readonly List<Blacksmith> blacksmithList = new List<Blacksmith>();
        readonly List<ResearchJob> activeJobs = new List<ResearchJob>();

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
                ResearchJob job = activeJobs[i];
                if (job.blacksmith == null)
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

                CompleteResearch(job);
                activeJobs.RemoveAt(i);
            }
        }

        static void CompleteResearch(ResearchJob job)
        {
            if (job.technology == null || job.blacksmith == null)
                return;

            UnitTeam team = job.blacksmith.Team;
            if (job.technology.kind == TechnologyKind.InfantryUpgrade)
                TechnologyState.CompleteInfantryUpgrade(team);
        }

        bool IsHeadJobIndex(int index)
        {
            Blacksmith blacksmith = activeJobs[index].blacksmith;
            for (int j = 0; j < index; j++)
            {
                if (activeJobs[j].blacksmith == blacksmith)
                    return false;
            }

            return true;
        }

        public static void Register(Blacksmith blacksmith)
        {
            if (instance == null || blacksmith == null)
                return;

            if (!instance.blacksmithList.Contains(blacksmith))
                instance.blacksmithList.Add(blacksmith);
        }

        public static void Unregister(Blacksmith blacksmith)
        {
            if (instance == null || blacksmith == null)
                return;

            instance.blacksmithList.Remove(blacksmith);
            instance.activeJobs.RemoveAll(job => job.blacksmith == blacksmith);
        }

        public static bool TryQueueResearch(Blacksmith blacksmith, TechnologyData technology)
        {
            if (instance == null || blacksmith == null || technology == null)
                return false;

            if (technology.ScaledResearchTime <= 0f)
                return false;

            if (GetQueueCount(blacksmith) > 0)
                return false;

            UnitTeam team = blacksmith.Team;
            if (GameSessionManager.GetAge(team) < technology.prerequisiteAge)
                return false;

            if (technology.kind == TechnologyKind.InfantryUpgrade && TechnologyState.HasInfantryUpgrade(team))
                return false;

            float foodCost = technology.ScaledFoodCost;
            float goldCost = technology.ScaledGoldCost;

            if (foodCost > 0f && ResourceManager.GetFood(team) < foodCost)
                return false;

            if (goldCost > 0f && ResourceManager.GetGold(team) < goldCost)
                return false;

            if (foodCost > 0f && !ResourceManager.TrySpendFood(team, foodCost))
                return false;

            if (goldCost > 0f && !ResourceManager.TrySpendGold(team, goldCost))
            {
                if (foodCost > 0f)
                    ResourceManager.AddFood(team, foodCost);
                return false;
            }

            float researchTime = technology.ScaledResearchTime;
            instance.activeJobs.Add(new ResearchJob
            {
                blacksmith = blacksmith,
                technology = technology,
                remainingSeconds = researchTime,
                totalSeconds = researchTime
            });
            return true;
        }

        public static bool IsResearching(Blacksmith blacksmith)
        {
            return GetQueueCount(blacksmith) > 0;
        }

        public static int GetQueueCount(Blacksmith blacksmith)
        {
            if (instance == null || blacksmith == null)
                return 0;

            int count = 0;
            for (int i = 0; i < instance.activeJobs.Count; i++)
            {
                if (instance.activeJobs[i].blacksmith == blacksmith)
                    count++;
            }

            return count;
        }

        public static float GetRemainingSeconds(Blacksmith blacksmith)
        {
            if (instance == null || blacksmith == null)
                return 0f;

            for (int i = 0; i < instance.activeJobs.Count; i++)
            {
                ResearchJob job = instance.activeJobs[i];
                if (job.blacksmith == blacksmith)
                    return job.remainingSeconds;
            }

            return 0f;
        }

        public static float GetTotalSeconds(Blacksmith blacksmith)
        {
            if (instance == null || blacksmith == null)
                return 0f;

            for (int i = 0; i < instance.activeJobs.Count; i++)
            {
                ResearchJob job = instance.activeJobs[i];
                if (job.blacksmith == blacksmith)
                    return job.totalSeconds;
            }

            return 0f;
        }
    }
}
