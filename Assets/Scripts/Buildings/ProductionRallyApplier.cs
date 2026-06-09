using System.Collections.Generic;
using AoE.RTS.Economy;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Buildings
{
    public static class ProductionRallyApplier
    {
        static readonly List<Unit> rallyBuffer = new List<Unit>(1);

        public static void Apply(TownCenter building, Unit spawnedUnit)
        {
            if (building == null || spawnedUnit == null || !spawnedUnit.IsAlive)
                return;

            ApplyRally(building.Rally, spawnedUnit);
        }

        public static void Apply(Barracks building, Unit spawnedUnit)
        {
            if (building == null || spawnedUnit == null || !spawnedUnit.IsAlive)
                return;

            ApplyRally(building.Rally, spawnedUnit);
        }

        static void ApplyRally(ProductionRallyPoint rally, Unit unit)
        {
            if (rally.kind == RallyTargetKind.None)
                return;

            if (unit.CanAttack)
            {
                unit.SetMoveTarget(GetMoveTargetForRally(rally));
                return;
            }

            rallyBuffer.Clear();
            rallyBuffer.Add(unit);

            switch (rally.kind)
            {
                case RallyTargetKind.Ground:
                    unit.SetMoveTarget(rally.groundPoint);
                    break;
                case RallyTargetKind.Tree:
                    if (rally.resourceTarget is TreeResource tree && !tree.IsDepleted)
                        GatherManager.IssueGatherCommand(rallyBuffer, tree);
                    break;
                case RallyTargetKind.BerryBush:
                    if (rally.resourceTarget is BerryBushResource bush && !bush.IsDepleted)
                        FoodGatherManager.IssueGatherCommand(rallyBuffer, bush);
                    break;
                case RallyTargetKind.Farm:
                    if (rally.resourceTarget is Farm farm && !farm.IsDepleted)
                        FoodGatherManager.IssueGatherFarmCommand(rallyBuffer, farm);
                    break;
                case RallyTargetKind.GoldMine:
                    if (rally.resourceTarget is GoldMineResource goldMine && !goldMine.IsDepleted)
                        MineralGatherManager.IssueGatherGoldCommand(rallyBuffer, goldMine);
                    break;
                case RallyTargetKind.StoneMine:
                    if (rally.resourceTarget is StoneMineResource stoneMine && !stoneMine.IsDepleted)
                        MineralGatherManager.IssueGatherStoneCommand(rallyBuffer, stoneMine);
                    break;
            }
        }

        static Vector3 GetMoveTargetForRally(ProductionRallyPoint rally)
        {
            if (rally.kind == RallyTargetKind.Ground)
                return rally.groundPoint;

            if (rally.resourceTarget != null)
                return rally.resourceTarget.transform.position;

            return rally.groundPoint;
        }
    }
}
