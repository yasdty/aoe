using UnityEngine;

namespace AoE.RTS.Units
{
    public static class UnitSpawner
    {
        public static Unit Spawn(UnitData unitData, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            return UnitPool.Rent(unitData, position, team);
        }
    }
}
