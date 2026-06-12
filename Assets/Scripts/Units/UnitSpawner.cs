using AoE.RTS.Core;
using UnityEngine;

namespace AoE.RTS.Units
{
    public static class UnitSpawner
    {
        public static Unit Spawn(UnitData unitData, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            return Spawn(unitData, position, PlayerIdMapping.FromLegacyTeam(team));
        }

        public static Unit Spawn(UnitData unitData, Vector3 position, PlayerId ownerId)
        {
            Unit unit = UnitPool.Rent(unitData, position, PlayerIdMapping.ToLegacyTeam(ownerId));
            if (unit != null)
                unit.SetOwner(ownerId);
            return unit;
        }
    }
}
