using AoE.RTS.Core;
using UnityEngine;

namespace AoE.RTS.Units
{
    public static class UnitSpawner
    {
        public static Unit Spawn(UnitData unitData, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            GameObject unitObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            unitObject.name = unitData != null ? unitData.displayName : "Unit";
            unitObject.layer = LayerMask.NameToLayer(GameLayers.UnitLayerName);
            unitObject.transform.position = position;

            Unit unit = unitObject.AddComponent<Unit>();
            if (unitData != null)
                unit.SetData(unitData);
            unit.SetTeam(team);

            return unit;
        }
    }
}
