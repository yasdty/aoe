using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Core;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Economy
{
    public class FoodGatherManager : MonoBehaviour, ISimulationTickable
    {
        enum GatherState
        {
            MoveToBush,
            Gather,
            MoveToDeposit
        }

        enum FarmGatherState
        {
            MoveToFarm,
            Gather,
            MoveToDeposit
        }

        enum HuntGatherState
        {
            MoveToAnimal,
            Hunt,
            MoveToDeposit
        }

        struct FoodGatherJob
        {
            public Unit unit;
            public BerryBushResource bush;
            public GatherState state;
            public float carriedFood;
        }

        struct FarmGatherJob
        {
            public Unit unit;
            public Farm farm;
            public FarmGatherState state;
            public float carriedFood;
        }

        struct HuntGatherJob
        {
            public Unit unit;
            public MonoBehaviour animalBehaviour;
            public HuntGatherState state;
            public float carriedFood;
        }

        const float CarryCapacity = 10f;
        const float GatherRate = 2.5f;
        const float GatherReachDistance = 2.5f;
        const float DepositReachDistance = 5f;
        const float GatherStandRadius = 2f;
        const float DepositStandRadius = 3.5f;

        static FoodGatherManager instance;
        readonly List<FoodGatherJob> jobs = new List<FoodGatherJob>();
        readonly List<FarmGatherJob> farmJobs = new List<FarmGatherJob>();
        readonly List<HuntGatherJob> huntJobs = new List<HuntGatherJob>();

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
            TickBerryJobs(fixedDeltaTime);
            TickFarmJobs(fixedDeltaTime);
            TickHuntJobs(fixedDeltaTime);
        }

        void TickBerryJobs(float fixedDeltaTime)
        {
            if (jobs.Count == 0)
                return;

            for (int i = jobs.Count - 1; i >= 0; i--)
            {
                FoodGatherJob job = jobs[i];
                if (job.unit == null || !job.unit.IsAlive)
                {
                    jobs.RemoveAt(i);
                    continue;
                }

                if (job.bush == null || job.bush.IsDepleted && job.carriedFood <= 0f)
                {
                    job.unit.ClearMoveTarget();
                    jobs.RemoveAt(i);
                    continue;
                }

                switch (job.state)
                {
                    case GatherState.MoveToBush:
                        TickMoveToBush(ref job, i);
                        break;
                    case GatherState.Gather:
                        TickGather(ref job, i, fixedDeltaTime);
                        break;
                    case GatherState.MoveToDeposit:
                        TickMoveToDeposit(ref job, i);
                        break;
                }
            }
        }

        public static void IssueGatherFarmCommand(IReadOnlyList<Unit> units, Farm farm)
        {
            if (instance == null || farm == null || farm.IsDepleted)
                return;

            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                if (unit == null || unit.CanAttack || unit.Team != farm.Team)
                    continue;

                if (ProductionManager.GetTownCenterForTeam(unit.Team) == null)
                    continue;

                if (IsFarmOccupiedByOther(farm, unit))
                    continue;

                instance.RemoveJobForUnit(unit);
                instance.farmJobs.Add(new FarmGatherJob
                {
                    unit = unit,
                    farm = farm,
                    state = FarmGatherState.MoveToFarm,
                    carriedFood = 0f
                });
                unit.SetMoveTarget(GetFarmGatherPosition(farm, unit));
            }
        }

        public static bool HasAssignableFarmGatherers(IReadOnlyList<Unit> units, Farm farm)
        {
            if (instance == null || farm == null || farm.IsDepleted)
                return false;

            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                if (unit == null || unit.CanAttack || unit.Team != farm.Team)
                    continue;

                if (ProductionManager.GetTownCenterForTeam(unit.Team) == null)
                    continue;

                if (!IsFarmOccupiedByOther(farm, unit))
                    return true;
            }

            return false;
        }

        static bool IsFarmOccupiedByOther(Farm farm, Unit requestingUnit)
        {
            for (int i = 0; i < instance.farmJobs.Count; i++)
            {
                FarmGatherJob job = instance.farmJobs[i];
                if (job.farm != farm)
                    continue;

                Unit worker = job.unit;
                if (worker == null || worker == requestingUnit || !worker.IsAlive)
                    continue;

                return true;
            }

            return false;
        }

        public static void CancelJobsForFarm(Farm farm)
        {
            if (instance == null || farm == null)
                return;

            for (int i = instance.farmJobs.Count - 1; i >= 0; i--)
            {
                if (instance.farmJobs[i].farm != farm)
                    continue;

                Unit unit = instance.farmJobs[i].unit;
                if (unit != null)
                    unit.ClearMoveTarget();

                instance.farmJobs.RemoveAt(i);
            }
        }

        public static void IssueGatherCommand(IReadOnlyList<Unit> units, BerryBushResource bush)
        {
            if (instance == null || bush == null || bush.IsDepleted)
                return;

            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                if (unit == null || ProductionManager.GetTownCenterForTeam(unit.Team) == null)
                    continue;

                instance.RemoveJobForUnit(unit);
                instance.jobs.Add(new FoodGatherJob
                {
                    unit = unit,
                    bush = bush,
                    state = GatherState.MoveToBush,
                    carriedFood = 0f
                });
                unit.SetMoveTarget(GetGatherPosition(bush, unit));
            }
        }

        public static void IssueHuntCommand(IReadOnlyList<Unit> units, IHuntableFoodResource animal)
        {
            if (instance == null || animal == null || animal.IsDepleted)
                return;

            if (animal is not MonoBehaviour animalBehaviour)
                return;

            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                if (unit == null || unit.CanAttack)
                    continue;

                if (ProductionManager.GetTownCenterForTeam(unit.Team) == null)
                    continue;

                instance.RemoveJobForUnit(unit);
                instance.huntJobs.Add(new HuntGatherJob
                {
                    unit = unit,
                    animalBehaviour = animalBehaviour,
                    state = HuntGatherState.MoveToAnimal,
                    carriedFood = 0f
                });
                unit.SetMoveTarget(GetHuntGatherPosition(animal, unit));
            }
        }

        public static void CancelForUnits(IReadOnlyList<Unit> units)
        {
            if (instance == null || units == null)
                return;

            for (int i = 0; i < units.Count; i++)
                instance.RemoveJobForUnit(units[i]);
        }

        static readonly List<Unit> singleUnitCancelBuffer = new List<Unit>(1);

        public static void CancelForUnit(Unit unit)
        {
            if (unit == null)
                return;

            singleUnitCancelBuffer.Clear();
            singleUnitCancelBuffer.Add(unit);
            CancelForUnits(singleUnitCancelBuffer);
        }

        void RemoveJobForUnit(Unit unit)
        {
            for (int i = jobs.Count - 1; i >= 0; i--)
            {
                if (jobs[i].unit == unit)
                    jobs.RemoveAt(i);
            }

            for (int i = farmJobs.Count - 1; i >= 0; i--)
            {
                if (farmJobs[i].unit == unit)
                    farmJobs.RemoveAt(i);
            }

            for (int i = huntJobs.Count - 1; i >= 0; i--)
            {
                if (huntJobs[i].unit == unit)
                {
                    ClearBoarHunter(huntJobs[i]);
                    huntJobs.RemoveAt(i);
                }
            }
        }

        static void ClearBoarHunter(HuntGatherJob job)
        {
            if (job.animalBehaviour is BoarResource boar)
                boar.ClearActiveHunter(job.unit);
        }

        static void SyncBoarHunter(HuntGatherJob job)
        {
            if (job.animalBehaviour is BoarResource boar)
                boar.SetActiveHunter(job.unit);
        }

        void TickFarmJobs(float fixedDeltaTime)
        {
            for (int i = farmJobs.Count - 1; i >= 0; i--)
            {
                if (i >= farmJobs.Count)
                    continue;

                FarmGatherJob job = farmJobs[i];
                if (job.unit == null || !job.unit.IsAlive)
                {
                    farmJobs.RemoveAt(i);
                    continue;
                }

                if (!IsFarmGatherTargetValid(job.farm))
                {
                    FinishFarmJobWithoutTarget(ref job, i);
                    continue;
                }

                switch (job.state)
                {
                    case FarmGatherState.MoveToFarm:
                        TickMoveToFarm(ref job, i);
                        break;
                    case FarmGatherState.Gather:
                        TickFarmGather(ref job, i, fixedDeltaTime);
                        break;
                    case FarmGatherState.MoveToDeposit:
                        TickFarmMoveToDeposit(ref job, i);
                        break;
                }
            }
        }

        static bool IsFarmGatherTargetValid(Farm farm)
        {
            return farm != null && farm.gameObject.activeInHierarchy && !farm.IsDepleted;
        }

        void FinishFarmJobWithoutTarget(ref FarmGatherJob job, int index)
        {
            if (job.carriedFood > 0f && ProductionManager.GetTownCenterForTeam(job.unit.Team) != null)
            {
                BeginFarmMoveToDeposit(ref job, index);
                return;
            }

            job.unit.ClearMoveTarget();
            if (index < farmJobs.Count && farmJobs[index].unit == job.unit)
                farmJobs.RemoveAt(index);
        }

        void TickMoveToFarm(ref FarmGatherJob job, int index)
        {
            if (!IsFarmGatherTargetValid(job.farm))
            {
                FinishFarmJobWithoutTarget(ref job, index);
                return;
            }

            Vector3 gatherPosition = GetFarmGatherPosition(job.farm, job.unit);
            if (job.unit.IsNear(gatherPosition, GatherReachDistance))
            {
                job.unit.ClearMoveTarget();
                job.state = FarmGatherState.Gather;
                if (index < farmJobs.Count && farmJobs[index].unit == job.unit)
                    farmJobs[index] = job;
                return;
            }

            if (!job.unit.HasMoveTarget)
                job.unit.SetMoveTarget(gatherPosition);
        }

        void TickFarmGather(ref FarmGatherJob job, int index, float deltaTime)
        {
            if (!IsFarmGatherTargetValid(job.farm))
            {
                FinishFarmJobWithoutTarget(ref job, index);
                return;
            }

            float request = GatherRate * deltaTime;
            float room = CarryCapacity - job.carriedFood;
            float taken = job.farm.TakeFood(Mathf.Min(request, room));
            job.carriedFood += taken;

            if (!IsFarmGatherTargetValid(job.farm))
            {
                FinishFarmJobWithoutTarget(ref job, index);
                return;
            }

            if (job.carriedFood >= CarryCapacity)
                BeginFarmMoveToDeposit(ref job, index);
            else if (index < farmJobs.Count && farmJobs[index].unit == job.unit)
                farmJobs[index] = job;
        }

        void BeginFarmMoveToDeposit(ref FarmGatherJob job, int index)
        {
            if (job.carriedFood <= 0f || ProductionManager.GetTownCenterForTeam(job.unit.Team) == null)
            {
                job.unit.ClearMoveTarget();
                if (index < farmJobs.Count && farmJobs[index].unit == job.unit)
                    farmJobs.RemoveAt(index);
                return;
            }

            job.state = FarmGatherState.MoveToDeposit;
            job.unit.SetMoveTarget(GetDepositPosition(job.unit));
            if (index < farmJobs.Count && farmJobs[index].unit == job.unit)
                farmJobs[index] = job;
        }

        void TickFarmMoveToDeposit(ref FarmGatherJob job, int index)
        {
            Vector3 depositPosition = GetDepositPosition(job.unit);
            if (depositPosition == Vector3.zero)
            {
                if (index < farmJobs.Count && farmJobs[index].unit == job.unit)
                    farmJobs.RemoveAt(index);
                return;
            }

            if (job.unit.IsNear(depositPosition, DepositReachDistance))
            {
                ResourceManager.AddFood(job.unit.Team, job.carriedFood);
                job.carriedFood = 0f;

                if (IsFarmGatherTargetValid(job.farm))
                {
                    job.state = FarmGatherState.MoveToFarm;
                    job.unit.SetMoveTarget(GetFarmGatherPosition(job.farm, job.unit));
                    if (index < farmJobs.Count && farmJobs[index].unit == job.unit)
                        farmJobs[index] = job;
                    return;
                }

                job.unit.ClearMoveTarget();
                if (index < farmJobs.Count && farmJobs[index].unit == job.unit)
                    farmJobs.RemoveAt(index);
                return;
            }

            if (!job.unit.HasMoveTarget)
                job.unit.SetMoveTarget(depositPosition);
        }

        static Vector3 GetFarmGatherPosition(Farm farm, Unit unit)
        {
            if (farm == null)
                return Vector3.zero;

            Vector3 center = farm.GetGatherPosition();
            return UnitPositionOffsets.ApplyRingOffset(center, unit, GatherStandRadius);
        }

        void TickMoveToBush(ref FoodGatherJob job, int index)
        {
            if (job.bush.IsDepleted)
            {
                BeginMoveToDeposit(ref job, index);
                return;
            }

            Vector3 gatherPosition = GetGatherPosition(job.bush, job.unit);
            if (job.unit.IsNear(gatherPosition, GatherReachDistance))
            {
                job.unit.ClearMoveTarget();
                job.state = GatherState.Gather;
                jobs[index] = job;
                return;
            }

            if (!job.unit.HasMoveTarget)
                job.unit.SetMoveTarget(gatherPosition);
        }

        void TickGather(ref FoodGatherJob job, int index, float deltaTime)
        {
            if (job.bush.IsDepleted)
            {
                BeginMoveToDeposit(ref job, index);
                return;
            }

            float request = GatherRate * deltaTime;
            float room = CarryCapacity - job.carriedFood;
            float taken = job.bush.TakeFood(Mathf.Min(request, room));
            job.carriedFood += taken;

            if (job.carriedFood >= CarryCapacity || job.bush.IsDepleted)
                BeginMoveToDeposit(ref job, index);
            else
                jobs[index] = job;
        }

        void BeginMoveToDeposit(ref FoodGatherJob job, int index)
        {
            if (job.carriedFood <= 0f || ProductionManager.GetTownCenterForTeam(job.unit.Team) == null)
            {
                job.unit.ClearMoveTarget();
                jobs.RemoveAt(index);
                return;
            }

            job.state = GatherState.MoveToDeposit;
            job.unit.SetMoveTarget(GetDepositPosition(job.unit));
            jobs[index] = job;
        }

        void TickMoveToDeposit(ref FoodGatherJob job, int index)
        {
            Vector3 depositPosition = GetDepositPosition(job.unit);
            if (depositPosition == Vector3.zero)
            {
                jobs.RemoveAt(index);
                return;
            }

            if (job.unit.IsNear(depositPosition, DepositReachDistance))
            {
                ResourceManager.AddFood(job.unit.Team, job.carriedFood);
                job.carriedFood = 0f;

                if (job.bush != null && !job.bush.IsDepleted)
                {
                    job.state = GatherState.MoveToBush;
                    job.unit.SetMoveTarget(GetGatherPosition(job.bush, job.unit));
                    jobs[index] = job;
                    return;
                }

                job.unit.ClearMoveTarget();
                jobs.RemoveAt(index);
                return;
            }

            if (!job.unit.HasMoveTarget)
                job.unit.SetMoveTarget(depositPosition);
        }

        static Vector3 GetGatherPosition(BerryBushResource bush, Unit unit)
        {
            if (bush == null)
                return Vector3.zero;

            Vector3 center = bush.GetGatherPosition();
            return UnitPositionOffsets.ApplyRingOffset(center, unit, GatherStandRadius);
        }

        static Vector3 GetDepositPosition(Unit unit)
        {
            if (unit == null)
                return Vector3.zero;

            if (MillRegistry.TryGetNearestFoodDepositPosition(unit, DepositStandRadius, out Vector3 position))
                return position;

            return Vector3.zero;
        }

        void TickHuntJobs(float fixedDeltaTime)
        {
            if (huntJobs.Count == 0)
                return;

            for (int i = huntJobs.Count - 1; i >= 0; i--)
            {
                HuntGatherJob job = huntJobs[i];
                if (job.unit == null || !job.unit.IsAlive)
                {
                    huntJobs.RemoveAt(i);
                    continue;
                }

                IHuntableFoodResource animal = job.animalBehaviour as IHuntableFoodResource;
                if (!IsHuntTargetValid(animal) && job.carriedFood <= 0f)
                {
                    job.unit.ClearMoveTarget();
                    huntJobs.RemoveAt(i);
                    continue;
                }

                switch (job.state)
                {
                    case HuntGatherState.MoveToAnimal:
                        TickMoveToAnimal(ref job, i);
                        break;
                    case HuntGatherState.Hunt:
                        TickHunt(ref job, i, fixedDeltaTime);
                        break;
                    case HuntGatherState.MoveToDeposit:
                        TickHuntMoveToDeposit(ref job, i);
                        break;
                }
            }
        }

        static bool IsHuntTargetValid(IHuntableFoodResource animal)
        {
            return animal != null
                && animal is MonoBehaviour behaviour
                && behaviour.gameObject.activeInHierarchy
                && !animal.IsDepleted;
        }

        void FinishHuntJobWithoutTarget(ref HuntGatherJob job, int index)
        {
            if (job.carriedFood > 0f && ProductionManager.GetTownCenterForTeam(job.unit.Team) != null)
            {
                ClearBoarHunter(job);
                BeginHuntMoveToDeposit(ref job, index);
                return;
            }

            ClearBoarHunter(job);
            job.unit.ClearMoveTarget();
            if (index < huntJobs.Count && huntJobs[index].unit == job.unit)
                huntJobs.RemoveAt(index);
        }

        void TickMoveToAnimal(ref HuntGatherJob job, int index)
        {
            IHuntableFoodResource animal = job.animalBehaviour as IHuntableFoodResource;
            if (!IsHuntTargetValid(animal))
            {
                FinishHuntJobWithoutTarget(ref job, index);
                return;
            }

            Vector3 gatherPosition = GetHuntGatherPosition(animal, job.unit);
            if (job.unit.IsNear(gatherPosition, GatherReachDistance))
            {
                job.unit.ClearMoveTarget();
                job.state = HuntGatherState.Hunt;
                SyncBoarHunter(job);
                if (index < huntJobs.Count && huntJobs[index].unit == job.unit)
                    huntJobs[index] = job;
                return;
            }

            if (!job.unit.HasMoveTarget)
                job.unit.SetMoveTarget(gatherPosition);
        }

        void TickHunt(ref HuntGatherJob job, int index, float deltaTime)
        {
            IHuntableFoodResource animal = job.animalBehaviour as IHuntableFoodResource;
            if (!IsHuntTargetValid(animal))
            {
                ClearBoarHunter(job);
                FinishHuntJobWithoutTarget(ref job, index);
                return;
            }

            SyncBoarHunter(job);

            float request = GatherRate * deltaTime;
            float room = CarryCapacity - job.carriedFood;

            if (job.animalBehaviour is BoarResource boar)
            {
                if (boar.IsDead)
                {
                    float taken = boar.TakeFood(Mathf.Min(request, room));
                    job.carriedFood += taken;
                }
                else
                {
                    boar.ApplyHuntDamage(request, job.unit);
                }
            }
            else
            {
                float taken = animal.TakeFood(Mathf.Min(request, room));
                job.carriedFood += taken;
            }

            if (!IsHuntTargetValid(animal))
            {
                ClearBoarHunter(job);
                FinishHuntJobWithoutTarget(ref job, index);
                return;
            }

            bool readyToDeposit = job.carriedFood >= CarryCapacity || animal.IsDepleted;
            if (job.animalBehaviour is BoarResource huntingBoar && !huntingBoar.IsDead)
                readyToDeposit = false;

            if (readyToDeposit)
                BeginHuntMoveToDeposit(ref job, index);
            else if (index < huntJobs.Count && huntJobs[index].unit == job.unit)
                huntJobs[index] = job;
        }

        void BeginHuntMoveToDeposit(ref HuntGatherJob job, int index)
        {
            if (job.carriedFood <= 0f || ProductionManager.GetTownCenterForTeam(job.unit.Team) == null)
            {
                job.unit.ClearMoveTarget();
                if (index < huntJobs.Count && huntJobs[index].unit == job.unit)
                    huntJobs.RemoveAt(index);
                return;
            }

            job.state = HuntGatherState.MoveToDeposit;
            job.unit.SetMoveTarget(GetDepositPosition(job.unit));
            if (index < huntJobs.Count && huntJobs[index].unit == job.unit)
                huntJobs[index] = job;
        }

        void TickHuntMoveToDeposit(ref HuntGatherJob job, int index)
        {
            Vector3 depositPosition = GetDepositPosition(job.unit);
            if (depositPosition == Vector3.zero)
            {
                if (index < huntJobs.Count && huntJobs[index].unit == job.unit)
                    huntJobs.RemoveAt(index);
                return;
            }

            if (job.unit.IsNear(depositPosition, DepositReachDistance))
            {
                ResourceManager.AddFood(job.unit.Team, job.carriedFood);
                job.carriedFood = 0f;

                IHuntableFoodResource animal = job.animalBehaviour as IHuntableFoodResource;
                if (IsHuntTargetValid(animal))
                {
                    job.state = HuntGatherState.MoveToAnimal;
                    job.unit.SetMoveTarget(GetHuntGatherPosition(animal, job.unit));
                    if (index < huntJobs.Count && huntJobs[index].unit == job.unit)
                        huntJobs[index] = job;
                    return;
                }

                job.unit.ClearMoveTarget();
                if (index < huntJobs.Count && huntJobs[index].unit == job.unit)
                    huntJobs.RemoveAt(index);
                return;
            }

            if (!job.unit.HasMoveTarget)
                job.unit.SetMoveTarget(depositPosition);
        }

        static Vector3 GetHuntGatherPosition(IHuntableFoodResource animal, Unit unit)
        {
            if (animal == null)
                return Vector3.zero;

            Vector3 center = animal.GetGatherPosition();
            return UnitPositionOffsets.ApplyRingOffset(center, unit, GatherStandRadius);
        }
    }
}
