using AoE.RTS.Economy;
using UnityEngine;

namespace AoE.RTS.Buildings
{
    public enum RallyTargetKind
    {
        None,
        Ground,
        Tree,
        BerryBush,
        Farm,
        GoldMine,
        StoneMine
    }

    public struct ProductionRallyPoint
    {
        public RallyTargetKind kind;
        public Vector3 groundPoint;
        public Component resourceTarget;

        public static ProductionRallyPoint None => default;

        public static ProductionRallyPoint FromGround(Vector3 point)
        {
            return new ProductionRallyPoint
            {
                kind = RallyTargetKind.Ground,
                groundPoint = point,
                resourceTarget = null
            };
        }

        public static ProductionRallyPoint FromTree(TreeResource tree)
        {
            return new ProductionRallyPoint
            {
                kind = RallyTargetKind.Tree,
                groundPoint = tree.transform.position,
                resourceTarget = tree
            };
        }

        public static ProductionRallyPoint FromBerryBush(BerryBushResource bush)
        {
            return new ProductionRallyPoint
            {
                kind = RallyTargetKind.BerryBush,
                groundPoint = bush.transform.position,
                resourceTarget = bush
            };
        }

        public static ProductionRallyPoint FromFarm(Farm farm)
        {
            return new ProductionRallyPoint
            {
                kind = RallyTargetKind.Farm,
                groundPoint = farm.transform.position,
                resourceTarget = farm
            };
        }

        public static ProductionRallyPoint FromGoldMine(GoldMineResource mine)
        {
            return new ProductionRallyPoint
            {
                kind = RallyTargetKind.GoldMine,
                groundPoint = mine.transform.position,
                resourceTarget = mine
            };
        }

        public static ProductionRallyPoint FromStoneMine(StoneMineResource mine)
        {
            return new ProductionRallyPoint
            {
                kind = RallyTargetKind.StoneMine,
                groundPoint = mine.transform.position,
                resourceTarget = mine
            };
        }
    }
}
