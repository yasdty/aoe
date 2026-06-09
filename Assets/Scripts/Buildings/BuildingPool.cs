using System.Collections.Generic;
using AoE.RTS.Units;
using AoE.RTS.Visuals;
using UnityEngine;

namespace AoE.RTS.Buildings
{
    public class BuildingPool : MonoBehaviour
    {
        static BuildingPool instance;

        readonly Stack<House> availableHouses = new Stack<House>();
        readonly Stack<Barracks> availableBarracks = new Stack<Barracks>();
        readonly Stack<Farm> availableFarms = new Stack<Farm>();
        readonly Stack<LumberCamp> availableLumberCamps = new Stack<LumberCamp>();
        readonly Stack<MiningCamp> availableMiningCamps = new Stack<MiningCamp>();
        readonly Stack<Mill> availableMills = new Stack<Mill>();

        int houseSpawnCount;
        int houseReuseCount;
        int barracksSpawnCount;
        int barracksReuseCount;
        int farmSpawnCount;
        int farmReuseCount;
        int lumberCampSpawnCount;
        int lumberCampReuseCount;
        int miningCampSpawnCount;
        int miningCampReuseCount;
        int millSpawnCount;
        int millReuseCount;

        void Awake()
        {
            instance = this;
            houseSpawnCount = 0;
            houseReuseCount = 0;
            barracksSpawnCount = 0;
            barracksReuseCount = 0;
            farmSpawnCount = 0;
            farmReuseCount = 0;
            lumberCampSpawnCount = 0;
            lumberCampReuseCount = 0;
            miningCampSpawnCount = 0;
            miningCampReuseCount = 0;
            millSpawnCount = 0;
            millReuseCount = 0;
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public static House RentHouse(PlacedBuildingData data, Vector3 position, UnitTeam team)
        {
            if (instance == null)
                return RuntimeBuildingFactory.CreateFreshHouse(data, position, team);

            return instance.RentHouseInternal(data, position, team);
        }

        public static Barracks RentBarracks(PlacedBuildingData data, Vector3 position, UnitTeam team)
        {
            if (instance == null)
                return RuntimeBuildingFactory.CreateFreshBarracks(data, position, team);

            return instance.RentBarracksInternal(data, position, team);
        }

        public static void ReturnHouse(House house)
        {
            if (house == null)
                return;

            if (instance == null)
            {
                Object.Destroy(house.gameObject);
                return;
            }

            instance.ReturnHouseInternal(house);
        }

        public static void ReturnBarracks(Barracks barracks)
        {
            if (barracks == null)
                return;

            if (instance == null)
            {
                Object.Destroy(barracks.gameObject);
                return;
            }

            instance.ReturnBarracksInternal(barracks);
        }

        public static Farm RentFarm(PlacedBuildingData data, Vector3 position, UnitTeam team)
        {
            if (instance == null)
                return RuntimeBuildingFactory.CreateFreshFarm(data, position, team);

            return instance.RentFarmInternal(data, position, team);
        }

        public static void ReturnFarm(Farm farm)
        {
            if (farm == null)
                return;

            if (instance == null)
            {
                Object.Destroy(farm.gameObject);
                return;
            }

            instance.ReturnFarmInternal(farm);
        }

        public static LumberCamp RentLumberCamp(PlacedBuildingData data, Vector3 position, UnitTeam team)
        {
            if (instance == null)
                return RuntimeBuildingFactory.CreateFreshLumberCamp(data, position, team);

            return instance.RentLumberCampInternal(data, position, team);
        }

        public static void ReturnLumberCamp(LumberCamp lumberCamp)
        {
            if (lumberCamp == null)
                return;

            if (instance == null)
            {
                Object.Destroy(lumberCamp.gameObject);
                return;
            }

            instance.ReturnLumberCampInternal(lumberCamp);
        }

        public static MiningCamp RentMiningCamp(PlacedBuildingData data, Vector3 position, UnitTeam team)
        {
            if (instance == null)
                return RuntimeBuildingFactory.CreateFreshMiningCamp(data, position, team);

            return instance.RentMiningCampInternal(data, position, team);
        }

        public static void ReturnMiningCamp(MiningCamp miningCamp)
        {
            if (miningCamp == null)
                return;

            if (instance == null)
            {
                Object.Destroy(miningCamp.gameObject);
                return;
            }

            instance.ReturnMiningCampInternal(miningCamp);
        }

        public static Mill RentMill(PlacedBuildingData data, Vector3 position, UnitTeam team)
        {
            if (instance == null)
                return RuntimeBuildingFactory.CreateFreshMill(data, position, team);

            return instance.RentMillInternal(data, position, team);
        }

        public static void ReturnMill(Mill mill)
        {
            if (mill == null)
                return;

            if (instance == null)
            {
                Object.Destroy(mill.gameObject);
                return;
            }

            instance.ReturnMillInternal(mill);
        }

        House RentHouseInternal(PlacedBuildingData data, Vector3 position, UnitTeam team)
        {
            House house;
            if (availableHouses.Count > 0)
            {
                house = availableHouses.Pop();
                houseReuseCount++;
                Debug.Log($"BuildingPool: spawn={houseSpawnCount} reuse={houseReuseCount} (House)");
            }
            else
            {
                house = RuntimeBuildingFactory.CreateFreshHouse(data, position, team);
                house.gameObject.SetActive(false);
                house.transform.SetParent(transform, false);
                houseSpawnCount++;
                Debug.Log($"BuildingPool: spawn={houseSpawnCount} reuse={houseReuseCount} (House)");
            }

            house.PrepareForReuse(data, position, team);
            house.gameObject.SetActive(true);
            return house;
        }

        Barracks RentBarracksInternal(PlacedBuildingData data, Vector3 position, UnitTeam team)
        {
            Barracks barracks;
            if (availableBarracks.Count > 0)
            {
                barracks = availableBarracks.Pop();
                barracksReuseCount++;
                Debug.Log($"BuildingPool: spawn={barracksSpawnCount} reuse={barracksReuseCount} (Barracks)");
            }
            else
            {
                barracks = RuntimeBuildingFactory.CreateFreshBarracks(data, position, team);
                barracks.gameObject.SetActive(false);
                barracks.transform.SetParent(transform, false);
                barracksSpawnCount++;
                Debug.Log($"BuildingPool: spawn={barracksSpawnCount} reuse={barracksReuseCount} (Barracks)");
            }

            barracks.PrepareForReuse(data, position, team);
            barracks.gameObject.SetActive(true);
            return barracks;
        }

        void ReturnHouseInternal(House house)
        {
            house.transform.SetParent(transform, false);
            house.gameObject.SetActive(false);
            availableHouses.Push(house);
        }

        void ReturnBarracksInternal(Barracks barracks)
        {
            barracks.transform.SetParent(transform, false);
            barracks.gameObject.SetActive(false);
            availableBarracks.Push(barracks);
        }

        Farm RentFarmInternal(PlacedBuildingData data, Vector3 position, UnitTeam team)
        {
            Farm farm;
            if (availableFarms.Count > 0)
            {
                farm = availableFarms.Pop();
                farmReuseCount++;
                Debug.Log($"BuildingPool: spawn={farmSpawnCount} reuse={farmReuseCount} (Farm)");
            }
            else
            {
                farm = RuntimeBuildingFactory.CreateFreshFarm(data, position, team);
                farm.gameObject.SetActive(false);
                farm.transform.SetParent(transform, false);
                farmSpawnCount++;
                Debug.Log($"BuildingPool: spawn={farmSpawnCount} reuse={farmReuseCount} (Farm)");
            }

            farm.PrepareForReuse(data, position, team);
            farm.gameObject.SetActive(true);
            return farm;
        }

        void ReturnFarmInternal(Farm farm)
        {
            farm.transform.SetParent(transform, false);
            farm.gameObject.SetActive(false);
            availableFarms.Push(farm);
        }

        LumberCamp RentLumberCampInternal(PlacedBuildingData data, Vector3 position, UnitTeam team)
        {
            LumberCamp lumberCamp;
            if (availableLumberCamps.Count > 0)
            {
                lumberCamp = availableLumberCamps.Pop();
                lumberCampReuseCount++;
                Debug.Log($"BuildingPool: spawn={lumberCampSpawnCount} reuse={lumberCampReuseCount} (LumberCamp)");
            }
            else
            {
                lumberCamp = RuntimeBuildingFactory.CreateFreshLumberCamp(data, position, team);
                lumberCamp.gameObject.SetActive(false);
                lumberCamp.transform.SetParent(transform, false);
                lumberCampSpawnCount++;
                Debug.Log($"BuildingPool: spawn={lumberCampSpawnCount} reuse={lumberCampReuseCount} (LumberCamp)");
            }

            lumberCamp.PrepareForReuse(data, position, team);
            lumberCamp.gameObject.SetActive(true);
            return lumberCamp;
        }

        void ReturnLumberCampInternal(LumberCamp lumberCamp)
        {
            lumberCamp.transform.SetParent(transform, false);
            lumberCamp.gameObject.SetActive(false);
            availableLumberCamps.Push(lumberCamp);
        }

        MiningCamp RentMiningCampInternal(PlacedBuildingData data, Vector3 position, UnitTeam team)
        {
            MiningCamp miningCamp;
            if (availableMiningCamps.Count > 0)
            {
                miningCamp = availableMiningCamps.Pop();
                miningCampReuseCount++;
                Debug.Log($"BuildingPool: spawn={miningCampSpawnCount} reuse={miningCampReuseCount} (MiningCamp)");
            }
            else
            {
                miningCamp = RuntimeBuildingFactory.CreateFreshMiningCamp(data, position, team);
                miningCamp.gameObject.SetActive(false);
                miningCamp.transform.SetParent(transform, false);
                miningCampSpawnCount++;
                Debug.Log($"BuildingPool: spawn={miningCampSpawnCount} reuse={miningCampReuseCount} (MiningCamp)");
            }

            miningCamp.PrepareForReuse(data, position, team);
            miningCamp.gameObject.SetActive(true);
            return miningCamp;
        }

        void ReturnMiningCampInternal(MiningCamp miningCamp)
        {
            miningCamp.transform.SetParent(transform, false);
            miningCamp.gameObject.SetActive(false);
            availableMiningCamps.Push(miningCamp);
        }

        Mill RentMillInternal(PlacedBuildingData data, Vector3 position, UnitTeam team)
        {
            Mill mill;
            if (availableMills.Count > 0)
            {
                mill = availableMills.Pop();
                millReuseCount++;
                Debug.Log($"BuildingPool: spawn={millSpawnCount} reuse={millReuseCount} (Mill)");
            }
            else
            {
                mill = RuntimeBuildingFactory.CreateFreshMill(data, position, team);
                mill.gameObject.SetActive(false);
                mill.transform.SetParent(transform, false);
                millSpawnCount++;
                Debug.Log($"BuildingPool: spawn={millSpawnCount} reuse={millReuseCount} (Mill)");
            }

            mill.PrepareForReuse(data, position, team);
            mill.gameObject.SetActive(true);
            return mill;
        }

        void ReturnMillInternal(Mill mill)
        {
            mill.transform.SetParent(transform, false);
            mill.gameObject.SetActive(false);
            availableMills.Push(mill);
        }
    }
}
