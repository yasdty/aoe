using System.Collections.Generic;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public class ControlGroupManager : MonoBehaviour
    {
        public const int SlotCount = 9;

        static ControlGroupManager instance;

        [SerializeField] SelectionManager selectionManager;

        readonly List<Unit>[] slots = new List<Unit>[SlotCount];
        readonly List<Unit> recallBuffer = new List<Unit>(32);

        void Awake()
        {
            instance = this;
            for (int i = 0; i < SlotCount; i++)
                slots[i] = new List<Unit>(8);

            if (selectionManager == null)
                selectionManager = GetComponent<SelectionManager>();
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public void SaveGroup(int slotIndex, IReadOnlyList<Unit> units)
        {
            if (!IsValidSlot(slotIndex))
                return;

            List<Unit> slot = slots[slotIndex];
            slot.Clear();
            if (units == null)
                return;

            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                if (IsValidPlayerUnit(unit))
                    slot.Add(unit);
            }
        }

        public void RecallGroup(int slotIndex, bool additive)
        {
            if (!IsValidSlot(slotIndex) || selectionManager == null)
                return;

            PruneSlot(slotIndex);
            List<Unit> slot = slots[slotIndex];
            recallBuffer.Clear();
            recallBuffer.AddRange(slot);
            if (recallBuffer.Count == 0)
                return;

            if (additive)
                selectionManager.SelectUnitsAdditive(recallBuffer);
            else
                selectionManager.SelectUnits(recallBuffer);
        }

        public int GetSlotCount(int slotIndex)
        {
            if (!IsValidSlot(slotIndex))
                return 0;

            PruneSlot(slotIndex);
            return slots[slotIndex].Count;
        }

        public static void HandleUnitDied(Unit unit)
        {
            if (instance == null || unit == null)
                return;

            for (int i = 0; i < SlotCount; i++)
                instance.slots[i].Remove(unit);
        }

        void PruneSlot(int slotIndex)
        {
            List<Unit> slot = slots[slotIndex];
            for (int i = slot.Count - 1; i >= 0; i--)
            {
                if (!IsValidPlayerUnit(slot[i]))
                    slot.RemoveAt(i);
            }
        }

        static bool IsValidSlot(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < SlotCount;
        }

        static bool IsValidPlayerUnit(Unit unit)
        {
            return unit != null && unit.IsAlive && unit.Team == UnitTeam.Player;
        }
    }
}
