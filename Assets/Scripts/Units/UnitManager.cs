using System.Collections.Generic;
using UnityEngine;

namespace AoE.RTS.Units
{
    public class UnitManager : MonoBehaviour
    {
        static UnitManager instance;
        readonly List<Unit> units = new List<Unit>();

        public static UnitManager Instance => instance;

        void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public static void Register(Unit unit)
        {
            if (instance == null || unit == null)
                return;

            if (!instance.units.Contains(unit))
                instance.units.Add(unit);
        }

        public static void Unregister(Unit unit)
        {
            instance?.units.Remove(unit);
        }

        public static void CopyUnitsTo(List<Unit> buffer)
        {
            buffer.Clear();
            if (instance == null)
                return;

            buffer.AddRange(instance.units);
        }

        public static int UnitCount => instance != null ? instance.units.Count : 0;

        void Update()
        {
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < units.Count; i++)
                units[i].TickMovement(deltaTime);
        }
    }
}
