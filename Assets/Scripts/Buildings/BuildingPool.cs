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

        int houseSpawnCount;
        int houseReuseCount;
        int barracksSpawnCount;
        int barracksReuseCount;

        void Awake()
        {
            instance = this;
            houseSpawnCount = 0;
            houseReuseCount = 0;
            barracksSpawnCount = 0;
            barracksReuseCount = 0;
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
    }
}
