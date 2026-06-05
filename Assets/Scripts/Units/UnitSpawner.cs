using AoE.RTS.Core;
using AoE.RTS.Units;
using AoE.RTS.Visuals;
using UnityEngine;

namespace AoE.RTS.Units
{
    public static class UnitSpawner
    {
        public static Unit Spawn(UnitData unitData, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            string unitName = unitData != null ? unitData.displayName : "Unit";
            PlaceholderVisualKind visualKind = EntityVisualBuilder.GetUnitVisualKind(unitData);
            GameObject unitObject = EntityVisualBuilder.CreateUnitShell(unitName, position, visualKind);

            Unit unit = unitObject.AddComponent<Unit>();
            if (unitData != null)
                unit.SetData(unitData);
            unit.SetTeam(team);

            return unit;
        }
    }
}
