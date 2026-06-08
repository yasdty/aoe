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

        int houseSpawnCount;
        int houseReuseCount;
        int barracksSpawnCount;
        int barracksReuseCount;
        int farmSpawnCount;
        int farmReuseCount;
        int lumberCampSpawnCount;
        int lumberCampReuseCount;

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
    }
}
