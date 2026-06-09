using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Combat;
using AoE.RTS.Economy;
using AoE.RTS.Selection;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Commands
{
    public static class GameCommandLists
    {
        public static List<Unit> CopyUnits(IReadOnlyList<Unit> source)
        {
            List<Unit> copy = new List<Unit>();
            if (source == null)
                return copy;

            for (int i = 0; i < source.Count; i++)
            {
                Unit unit = source[i];
                if (unit != null)
                    copy.Add(unit);
            }

            return copy;
        }
    }

    public sealed class MoveCommand : IGameCommand
    {
        readonly List<Unit> units;
        readonly Vector3 destination;
        readonly float spacing;

        public string DebugName => "Move";

        public MoveCommand(IReadOnlyList<Unit> units, Vector3 destination, float spacing)
        {
            this.units = GameCommandLists.CopyUnits(units);
            this.destination = destination;
            this.spacing = spacing;
        }

        public void Execute()
        {
            if (units.Count == 0)
                return;

            BuildingPlacementManager.AbortConstructionForUnits(units);
            GatherManager.CancelForUnits(units);
            FoodGatherManager.CancelForUnits(units);
            MineralGatherManager.CancelForUnits(units);
            AttackManager.CancelForUnits(units);
            BoarAttackManager.CancelForUnits(units);
            GroupMoveFormation.AssignMoveTargets(units, destination, spacing);
        }
    }

    public sealed class GatherCommand : IGameCommand
    {
        readonly List<Unit> units;
        readonly TreeResource tree;

        public string DebugName => "Gather";

        public GatherCommand(IReadOnlyList<Unit> units, TreeResource tree)
        {
            this.units = GameCommandLists.CopyUnits(units);
            this.tree = tree;
        }

        public void Execute()
        {
            if (units.Count == 0 || tree == null || tree.IsDepleted)
                return;

            BuildingPlacementManager.AbortConstructionForUnits(units);
            AttackManager.CancelForUnits(units);
            BoarAttackManager.CancelForUnits(units);
            FoodGatherManager.CancelForUnits(units);
            MineralGatherManager.CancelForUnits(units);
            GatherManager.IssueGatherCommand(units, tree);
        }
    }

    public sealed class GatherFarmFoodCommand : IGameCommand
    {
        readonly List<Unit> units;
        readonly Farm farm;

        public string DebugName => "GatherFarmFood";

        public GatherFarmFoodCommand(IReadOnlyList<Unit> units, Farm farm)
        {
            this.units = GameCommandLists.CopyUnits(units);
            this.farm = farm;
        }

        public void Execute()
        {
            if (units.Count == 0 || farm == null || farm.IsDepleted)
                return;

            BuildingPlacementManager.AbortConstructionForUnits(units);
            AttackManager.CancelForUnits(units);
            BoarAttackManager.CancelForUnits(units);
            GatherManager.CancelForUnits(units);
            MineralGatherManager.CancelForUnits(units);
            FoodGatherManager.IssueGatherFarmCommand(units, farm);
        }
    }

    public sealed class GatherFoodCommand : IGameCommand
    {
        readonly List<Unit> units;
        readonly BerryBushResource bush;

        public string DebugName => "GatherFood";

        public GatherFoodCommand(IReadOnlyList<Unit> units, BerryBushResource bush)
        {
            this.units = GameCommandLists.CopyUnits(units);
            this.bush = bush;
        }

        public void Execute()
        {
            if (units.Count == 0 || bush == null || bush.IsDepleted)
                return;

            BuildingPlacementManager.AbortConstructionForUnits(units);
            AttackManager.CancelForUnits(units);
            BoarAttackManager.CancelForUnits(units);
            GatherManager.CancelForUnits(units);
            MineralGatherManager.CancelForUnits(units);
            FoodGatherManager.IssueGatherCommand(units, bush);
        }
    }

    public sealed class HuntFoodCommand : IGameCommand
    {
        readonly List<Unit> units;
        readonly IHuntableFoodResource animal;

        public string DebugName => "HuntFood";

        public HuntFoodCommand(IReadOnlyList<Unit> units, DeerResource deer)
        {
            this.units = GameCommandLists.CopyUnits(units);
            animal = deer;
        }

        public HuntFoodCommand(IReadOnlyList<Unit> units, SheepResource sheep)
        {
            this.units = GameCommandLists.CopyUnits(units);
            animal = sheep;
        }

        public HuntFoodCommand(IReadOnlyList<Unit> units, BoarResource boar)
        {
            this.units = GameCommandLists.CopyUnits(units);
            animal = boar;
        }

        public void Execute()
        {
            if (units.Count == 0 || animal == null || animal.IsDepleted)
                return;

            BuildingPlacementManager.AbortConstructionForUnits(units);
            AttackManager.CancelForUnits(units);
            GatherManager.CancelForUnits(units);
            MineralGatherManager.CancelForUnits(units);
            FoodGatherManager.IssueHuntCommand(units, animal);
        }
    }

    public sealed class SheepMoveCommand : IGameCommand
    {
        readonly SheepResource sheep;
        readonly Vector3 destination;

        public string DebugName => "SheepMove";

        public SheepMoveCommand(SheepResource sheep, Vector3 destination)
        {
            this.sheep = sheep;
            this.destination = destination;
        }

        public void Execute()
        {
            if (sheep == null || sheep.IsDepleted || sheep.IsNeutral)
                return;

            sheep.SetMoveTarget(destination);
        }
    }

    public sealed class GatherGoldCommand : IGameCommand
    {
        readonly List<Unit> units;
        readonly GoldMineResource mine;

        public string DebugName => "GatherGold";

        public GatherGoldCommand(IReadOnlyList<Unit> units, GoldMineResource mine)
        {
            this.units = GameCommandLists.CopyUnits(units);
            this.mine = mine;
        }

        public void Execute()
        {
            if (units.Count == 0 || mine == null || mine.IsDepleted)
                return;

            BuildingPlacementManager.AbortConstructionForUnits(units);
            AttackManager.CancelForUnits(units);
            BoarAttackManager.CancelForUnits(units);
            GatherManager.CancelForUnits(units);
            FoodGatherManager.CancelForUnits(units);
            MineralGatherManager.IssueGatherGoldCommand(units, mine);
        }
    }

    public sealed class GatherStoneCommand : IGameCommand
    {
        readonly List<Unit> units;
        readonly StoneMineResource mine;

        public string DebugName => "GatherStone";

        public GatherStoneCommand(IReadOnlyList<Unit> units, StoneMineResource mine)
        {
            this.units = GameCommandLists.CopyUnits(units);
            this.mine = mine;
        }

        public void Execute()
        {
            if (units.Count == 0 || mine == null || mine.IsDepleted)
                return;

            BuildingPlacementManager.AbortConstructionForUnits(units);
            AttackManager.CancelForUnits(units);
            BoarAttackManager.CancelForUnits(units);
            GatherManager.CancelForUnits(units);
            FoodGatherManager.CancelForUnits(units);
            MineralGatherManager.IssueGatherStoneCommand(units, mine);
        }
    }

    public sealed class AttackUnitCommand : IGameCommand
    {
        readonly List<Unit> selectedUnits;
        readonly Unit targetUnit;
        readonly List<Unit> attackers = new List<Unit>();

        public string DebugName => "AttackUnit";

        public AttackUnitCommand(IReadOnlyList<Unit> selectedUnits, Unit targetUnit)
        {
            this.selectedUnits = GameCommandLists.CopyUnits(selectedUnits);
            this.targetUnit = targetUnit;
        }

        public void Execute()
        {
            if (targetUnit == null || !targetUnit.IsAlive || selectedUnits.Count == 0)
                return;

            attackers.Clear();
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                Unit unit = selectedUnits[i];
                if (unit == null || !unit.CanAttack || unit.Team == targetUnit.Team)
                    continue;

                attackers.Add(unit);
            }

            if (attackers.Count == 0)
                return;

            BuildingPlacementManager.AbortConstructionForUnits(selectedUnits);
            GatherManager.CancelForUnits(selectedUnits);
            FoodGatherManager.CancelForUnits(selectedUnits);
            MineralGatherManager.CancelForUnits(selectedUnits);
            BoarAttackManager.CancelForUnits(selectedUnits);
            AttackManager.IssueAttack(attackers, targetUnit);
        }
    }

    public sealed class AttackBoarCommand : IGameCommand
    {
        readonly List<Unit> selectedUnits;
        readonly BoarResource boar;
        readonly List<Unit> attackers = new List<Unit>();

        public string DebugName => "AttackBoar";

        public AttackBoarCommand(IReadOnlyList<Unit> selectedUnits, BoarResource boar)
        {
            this.selectedUnits = GameCommandLists.CopyUnits(selectedUnits);
            this.boar = boar;
        }

        public void Execute()
        {
            if (boar == null || boar.IsDead || boar.IsDepleted || selectedUnits.Count == 0)
                return;

            attackers.Clear();
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                Unit unit = selectedUnits[i];
                if (unit == null || !unit.CanAttack)
                    continue;

                attackers.Add(unit);
            }

            if (attackers.Count == 0)
                return;

            BuildingPlacementManager.AbortConstructionForUnits(selectedUnits);
            GatherManager.CancelForUnits(selectedUnits);
            FoodGatherManager.CancelForUnits(selectedUnits);
            MineralGatherManager.CancelForUnits(selectedUnits);
            AttackManager.CancelForUnits(selectedUnits);
            BoarAttackManager.IssueAttack(attackers, boar);
        }
    }

    public sealed class AttackBuildingCommand : IGameCommand
    {
        readonly List<Unit> selectedUnits;
        readonly BuildingHealth targetBuilding;
        readonly List<Unit> attackers = new List<Unit>();

        public string DebugName => "AttackBuilding";

        public AttackBuildingCommand(IReadOnlyList<Unit> selectedUnits, BuildingHealth targetBuilding)
        {
            this.selectedUnits = GameCommandLists.CopyUnits(selectedUnits);
            this.targetBuilding = targetBuilding;
        }

        public void Execute()
        {
            if (targetBuilding == null || !targetBuilding.IsAlive || selectedUnits.Count == 0)
                return;

            attackers.Clear();
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                Unit unit = selectedUnits[i];
                if (unit == null || !unit.CanAttack || unit.Team == targetBuilding.Team)
                    continue;

                attackers.Add(unit);
            }

            if (attackers.Count == 0)
                return;

            BuildingPlacementManager.AbortConstructionForUnits(selectedUnits);
            GatherManager.CancelForUnits(selectedUnits);
            FoodGatherManager.CancelForUnits(selectedUnits);
            MineralGatherManager.CancelForUnits(selectedUnits);
            BoarAttackManager.CancelForUnits(selectedUnits);
            AttackManager.IssueAttack(attackers, targetBuilding);
        }
    }

    public sealed class BuildConfirmCommand : IGameCommand
    {
        readonly List<Unit> builders;

        public string DebugName => "BuildConfirm";

        public BuildConfirmCommand(IReadOnlyList<Unit> builders)
        {
            this.builders = GameCommandLists.CopyUnits(builders);
        }

        public void Execute()
        {
            BuildingPlacementManager.TryConfirmPlacement(builders);
        }
    }

    public sealed class TrainVillagerCommand : IGameCommand
    {
        readonly TownCenter townCenter;

        public string DebugName => "TrainVillager";

        public TrainVillagerCommand(TownCenter townCenter)
        {
            this.townCenter = townCenter;
        }

        public void Execute()
        {
            if (townCenter == null)
                return;

            townCenter.TryQueueVillagerProduction();
        }
    }

    public sealed class TrainMilitiaCommand : IGameCommand
    {
        readonly Barracks barracks;

        public string DebugName => "TrainMilitia";

        public TrainMilitiaCommand(Barracks barracks)
        {
            this.barracks = barracks;
        }

        public void Execute()
        {
            if (barracks == null)
                return;

            barracks.TryQueueMilitiaProduction();
        }
    }

    public sealed class TrainSpearmanCommand : IGameCommand
    {
        readonly Barracks barracks;

        public string DebugName => "TrainSpearman";

        public TrainSpearmanCommand(Barracks barracks)
        {
            this.barracks = barracks;
        }

        public void Execute()
        {
            if (barracks == null)
                return;

            barracks.TryQueueSpearmanProduction();
        }
    }

    public sealed class TrainArcherCommand : IGameCommand
    {
        readonly ArcheryRange archeryRange;

        public string DebugName => "TrainArcher";

        public TrainArcherCommand(ArcheryRange archeryRange)
        {
            this.archeryRange = archeryRange;
        }

        public void Execute()
        {
            if (archeryRange == null)
                return;

            archeryRange.TryQueueArcherProduction();
        }
    }

    public sealed class TrainCavalryCommand : IGameCommand
    {
        readonly Stable stable;

        public string DebugName => "TrainCavalry";

        public TrainCavalryCommand(Stable stable)
        {
            this.stable = stable;
        }

        public void Execute()
        {
            if (stable == null)
                return;

            stable.TryQueueCavalryProduction();
        }
    }

    public sealed class TrainScoutCommand : IGameCommand
    {
        readonly Stable stable;

        public string DebugName => "TrainScout";

        public TrainScoutCommand(Stable stable)
        {
            this.stable = stable;
        }

        public void Execute()
        {
            if (stable == null)
                return;

            stable.TryQueueScoutProduction();
        }
    }

    public sealed class SetRallyPointCommand : IGameCommand
    {
        readonly TownCenter townCenter;
        readonly Barracks barracks;
        readonly ArcheryRange archeryRange;
        readonly Stable stable;
        readonly ProductionRallyPoint rally;

        public string DebugName => "SetRallyPoint";

        public SetRallyPointCommand(TownCenter townCenter, ProductionRallyPoint rally)
        {
            this.townCenter = townCenter;
            this.barracks = null;
            this.archeryRange = null;
            this.stable = null;
            this.rally = rally;
        }

        public SetRallyPointCommand(Barracks barracks, ProductionRallyPoint rally)
        {
            this.townCenter = null;
            this.barracks = barracks;
            this.archeryRange = null;
            this.stable = null;
            this.rally = rally;
        }

        public SetRallyPointCommand(ArcheryRange archeryRange, ProductionRallyPoint rally)
        {
            this.townCenter = null;
            this.barracks = null;
            this.archeryRange = archeryRange;
            this.stable = null;
            this.rally = rally;
        }

        public SetRallyPointCommand(Stable stable, ProductionRallyPoint rally)
        {
            this.townCenter = null;
            this.barracks = null;
            this.archeryRange = null;
            this.stable = stable;
            this.rally = rally;
        }

        public void Execute()
        {
            if (townCenter != null)
            {
                townCenter.SetRally(rally);
                return;
            }

            if (barracks != null)
            {
                barracks.SetRally(rally);
                return;
            }

            if (archeryRange != null)
            {
                archeryRange.SetRally(rally);
                return;
            }

            if (stable != null)
                stable.SetRally(rally);
        }
    }
}
