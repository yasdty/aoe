using System.Collections.Generic;
using AoE.RTS.Visuals;
using UnityEngine;

namespace AoE.RTS.Units
{
    public class UnitPool : MonoBehaviour
    {
        static UnitPool instance;

        readonly Dictionary<PlaceholderVisualKind, Stack<Unit>> available = new Dictionary<PlaceholderVisualKind, Stack<Unit>>();

        [SerializeField] int prewarmVillagers = 4;
        [SerializeField] int prewarmMilitia = 4;
        [SerializeField] UnitData prewarmVillagerData;
        [SerializeField] UnitData prewarmMilitiaData;

        int villagerSpawnCount;
        int villagerReuseCount;
        int militiaSpawnCount;
        int militiaReuseCount;

        void Awake()
        {
            instance = this;
            villagerSpawnCount = 0;
            villagerReuseCount = 0;
            militiaSpawnCount = 0;
            militiaReuseCount = 0;
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        void Start()
        {
            Prewarm(PlaceholderVisualKind.Villager, prewarmVillagerData, prewarmVillagers, UnitTeam.Player);
            Prewarm(PlaceholderVisualKind.Militia, prewarmMilitiaData, prewarmMilitia, UnitTeam.Player);
        }

        public static Unit Rent(UnitData data, Vector3 position, UnitTeam team)
        {
            EnsureInstance();
            PlaceholderVisualKind kind = EntityVisualBuilder.GetUnitVisualKind(data);
            return instance.RentInternal(kind, data, position, team);
        }

        public static void Return(Unit unit)
        {
            if (unit == null)
                return;

            if (instance == null)
            {
                Object.Destroy(unit.gameObject);
                return;
            }

            instance.ReturnInternal(unit);
        }

        static void EnsureInstance()
        {
            if (instance != null)
                return;

            GameObject poolObject = new GameObject("UnitPool");
            instance = poolObject.AddComponent<UnitPool>();
            Debug.LogWarning(
                "UnitPool was missing from the scene. Created at runtime. "
                + "Run AoE → Setup Phase10 Scene (Edit mode, not playing) for Prewarm and full wiring.");
        }

        Unit RentInternal(PlaceholderVisualKind kind, UnitData data, Vector3 position, UnitTeam team)
        {
            Stack<Unit> stack = GetStack(kind);
            Unit unit;
            if (stack.Count > 0)
            {
                unit = stack.Pop();
                IncrementReuse(kind);
            }
            else
            {
                unit = CreateFreshUnit(kind, data, transform);
                IncrementSpawn(kind);
            }

            unit.PrepareForSpawn(data, position, team);
            unit.gameObject.SetActive(true);
            return unit;
        }

        void ReturnInternal(Unit unit)
        {
            unit.transform.SetParent(transform);
            unit.gameObject.SetActive(false);

            PlaceholderVisualKind kind = EntityVisualBuilder.GetUnitVisualKind(unit.Data);
            GetStack(kind).Push(unit);
        }

        void Prewarm(PlaceholderVisualKind kind, UnitData data, int count, UnitTeam team)
        {
            if (count <= 0 || data == null)
                return;

            for (int i = 0; i < count; i++)
            {
                Unit unit = CreateFreshUnit(kind, data, transform);
                IncrementSpawn(kind);
                ReturnInternal(unit);
            }
        }

        Stack<Unit> GetStack(PlaceholderVisualKind kind)
        {
            if (!available.TryGetValue(kind, out Stack<Unit> stack))
            {
                stack = new Stack<Unit>();
                available[kind] = stack;
            }

            return stack;
        }

        static Unit CreateFreshUnit(PlaceholderVisualKind kind, UnitData data, Transform parent)
        {
            string unitName = data != null ? data.displayName : kind.ToString();
            GameObject unitObject = EntityVisualBuilder.CreateUnitShell(unitName, Vector3.zero, kind);
            if (parent != null)
                unitObject.transform.SetParent(parent, false);

            unitObject.SetActive(false);
            Unit unit = unitObject.AddComponent<Unit>();
            if (data != null)
                unit.SetData(data);

            return unit;
        }

        void IncrementSpawn(PlaceholderVisualKind kind)
        {
            if (kind == PlaceholderVisualKind.Militia)
            {
                militiaSpawnCount++;
                Debug.Log($"UnitPool: spawn={militiaSpawnCount} reuse={militiaReuseCount} (Militia)");
            }
            else
            {
                villagerSpawnCount++;
                Debug.Log($"UnitPool: spawn={villagerSpawnCount} reuse={villagerReuseCount} (Villager)");
            }
        }

        void IncrementReuse(PlaceholderVisualKind kind)
        {
            if (kind == PlaceholderVisualKind.Militia)
            {
                militiaReuseCount++;
                Debug.Log($"UnitPool: spawn={militiaSpawnCount} reuse={militiaReuseCount} (Militia)");
            }
            else
            {
                villagerReuseCount++;
                Debug.Log($"UnitPool: spawn={villagerSpawnCount} reuse={villagerReuseCount} (Villager)");
            }
        }
    }
}
