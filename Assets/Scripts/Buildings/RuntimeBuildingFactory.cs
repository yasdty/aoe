using AoE.RTS.Units;
using AoE.RTS.Visuals;
using UnityEngine;

namespace AoE.RTS.Buildings
{
    public static class RuntimeBuildingFactory
    {
        static Material sharedLitMaterial;

        public static House CreateHouse(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            return BuildingPool.RentHouse(data, position, team);
        }

        public static Barracks CreateBarracks(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            return BuildingPool.RentBarracks(data, position, team);
        }

        public static Farm CreateFarm(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            return BuildingPool.RentFarm(data, position, team);
        }

        public static LumberCamp CreateLumberCamp(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            return BuildingPool.RentLumberCamp(data, position, team);
        }

        public static MiningCamp CreateMiningCamp(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            return BuildingPool.RentMiningCamp(data, position, team);
        }

        public static Mill CreateMill(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            return BuildingPool.RentMill(data, position, team);
        }

        public static ArcheryRange CreateArcheryRange(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            return BuildingPool.RentArcheryRange(data, position, team);
        }

        public static Stable CreateStable(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            return BuildingPool.RentStable(data, position, team);
        }

        public static Blacksmith CreateBlacksmith(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            return CreateFreshBlacksmith(data, position, team);
        }

        public static PalisadeWall CreatePalisadeWall(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            return CreateFreshPalisadeWall(data, position, team);
        }

        public static StoneWall CreateStoneWall(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            return CreateFreshStoneWall(data, position, team);
        }

        public static WatchTower CreateWatchTower(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            return CreateFreshWatchTower(data, position, team);
        }

        public static Market CreateMarket(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            return CreateFreshMarket(data, position, team);
        }

        public static House CreateFreshHouse(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            if (data == null)
                return null;

            Vector3 worldPosition = ResolveWorldPosition(data, position);
            GameObject houseObject = EntityVisualBuilder.CreateBuildingShell(
                "House",
                LayerMask.NameToLayer("Building"),
                worldPosition,
                new Vector3(data.footprintWidth, data.buildingHeight, data.footprintDepth),
                Vector3.zero,
                PlaceholderVisualKind.House);

            ApplySharedMaterialIfMissingRendererTint(houseObject);
            ConfigureBuildingHealth(houseObject, data.maxHp, data.meleeArmor, data.pierceArmor, team);

            House house = houseObject.AddComponent<House>();
            house.SetData(data);
            return house;
        }

        public static Barracks CreateFreshBarracks(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            if (data == null)
                return null;

            Vector3 worldPosition = ResolveWorldPosition(data, position);
            GameObject barracksObject = EntityVisualBuilder.CreateBuildingShell(
                "Barracks",
                LayerMask.NameToLayer("Building"),
                worldPosition,
                new Vector3(data.footprintWidth, data.buildingHeight, data.footprintDepth),
                Vector3.zero,
                PlaceholderVisualKind.Barracks);

            ApplySharedMaterialIfMissingRendererTint(barracksObject);
            ConfigureBuildingHealth(barracksObject, data.maxHp, data.meleeArmor, data.pierceArmor, team);

            Barracks barracks = barracksObject.AddComponent<Barracks>();
            barracks.SetData(data);
            barracks.SetTeam(team);
            return barracks;
        }

        public static ArcheryRange CreateFreshArcheryRange(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            if (data == null)
                return null;

            Vector3 worldPosition = ResolveWorldPosition(data, position);
            GameObject archeryRangeObject = EntityVisualBuilder.CreateBuildingShell(
                "ArcheryRange",
                LayerMask.NameToLayer("Building"),
                worldPosition,
                new Vector3(data.footprintWidth, data.buildingHeight, data.footprintDepth),
                Vector3.zero,
                PlaceholderVisualKind.Barracks);

            ApplySharedMaterialIfMissingRendererTint(archeryRangeObject);
            ConfigureBuildingHealth(archeryRangeObject, data.maxHp, data.meleeArmor, data.pierceArmor, team);

            ArcheryRange archeryRange = archeryRangeObject.AddComponent<ArcheryRange>();
            archeryRange.SetData(data);
            archeryRange.SetTeam(team);
            return archeryRange;
        }

        public static Market CreateFreshMarket(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            if (data == null)
                return null;

            Vector3 worldPosition = ResolveWorldPosition(data, position);
            GameObject marketObject = EntityVisualBuilder.CreateBuildingShell(
                "Market",
                LayerMask.NameToLayer("Building"),
                worldPosition,
                new Vector3(data.footprintWidth, data.buildingHeight, data.footprintDepth),
                Vector3.zero,
                PlaceholderVisualKind.House);

            ApplySharedMaterialIfMissingRendererTint(marketObject);
            ConfigureBuildingHealth(marketObject, data.maxHp, data.meleeArmor, data.pierceArmor, team);

            Market market = marketObject.AddComponent<Market>();
            market.SetData(data);
            market.SetTeam(team);
            return market;
        }

        public static Blacksmith CreateFreshBlacksmith(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            if (data == null)
                return null;

            Vector3 worldPosition = ResolveWorldPosition(data, position);
            GameObject blacksmithObject = EntityVisualBuilder.CreateBuildingShell(
                "Blacksmith",
                LayerMask.NameToLayer("Building"),
                worldPosition,
                new Vector3(data.footprintWidth, data.buildingHeight, data.footprintDepth),
                Vector3.zero,
                PlaceholderVisualKind.Barracks);

            ApplySharedMaterialIfMissingRendererTint(blacksmithObject);
            ConfigureBuildingHealth(blacksmithObject, data.maxHp, data.meleeArmor, data.pierceArmor, team);

            Blacksmith blacksmith = blacksmithObject.AddComponent<Blacksmith>();
            blacksmith.SetData(data);
            blacksmith.SetTeam(team);
            return blacksmith;
        }

        public static PalisadeWall CreateFreshPalisadeWall(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            return CreateFreshDefenseWall<PalisadeWall>("PalisadeWall", data, position, team);
        }

        public static StoneWall CreateFreshStoneWall(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            return CreateFreshDefenseWall<StoneWall>("StoneWall", data, position, team);
        }

        public static WatchTower CreateFreshWatchTower(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            if (data == null)
                return null;

            Vector3 worldPosition = ResolveWorldPosition(data, position);
            GameObject towerObject = EntityVisualBuilder.CreateBuildingShell(
                "WatchTower",
                LayerMask.NameToLayer("Building"),
                worldPosition,
                new Vector3(data.footprintWidth, data.buildingHeight, data.footprintDepth),
                Vector3.zero,
                PlaceholderVisualKind.Barracks);

            ApplySharedMaterialIfMissingRendererTint(towerObject);
            ConfigureBuildingHealth(towerObject, data.maxHp, data.meleeArmor, data.pierceArmor, team);

            WatchTower watchTower = towerObject.AddComponent<WatchTower>();
            watchTower.SetData(data);
            return watchTower;
        }

        static TWall CreateFreshDefenseWall<TWall>(
            string objectName,
            PlacedBuildingData data,
            Vector3 position,
            UnitTeam team)
            where TWall : MonoBehaviour
        {
            if (data == null)
                return null;

            Vector3 worldPosition = ResolveWorldPosition(data, position);
            GameObject wallObject = EntityVisualBuilder.CreateBuildingShell(
                objectName,
                LayerMask.NameToLayer("Building"),
                worldPosition,
                new Vector3(data.footprintWidth, data.buildingHeight, data.footprintDepth),
                Vector3.zero,
                PlaceholderVisualKind.House);

            ApplySharedMaterialIfMissingRendererTint(wallObject);
            ConfigureBuildingHealth(wallObject, data.maxHp, data.meleeArmor, data.pierceArmor, team);

            TWall wall = wallObject.AddComponent<TWall>();
            if (wall is PalisadeWall palisadeWall)
                palisadeWall.SetData(data);
            else if (wall is StoneWall stoneWall)
                stoneWall.SetData(data);

            return wall;
        }

        public static Stable CreateFreshStable(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            if (data == null)
                return null;

            Vector3 worldPosition = ResolveWorldPosition(data, position);
            GameObject stableObject = EntityVisualBuilder.CreateBuildingShell(
                "Stable",
                LayerMask.NameToLayer("Building"),
                worldPosition,
                new Vector3(data.footprintWidth, data.buildingHeight, data.footprintDepth),
                Vector3.zero,
                PlaceholderVisualKind.Barracks);

            ApplySharedMaterialIfMissingRendererTint(stableObject);
            ConfigureBuildingHealth(stableObject, data.maxHp, data.meleeArmor, data.pierceArmor, team);

            Stable stable = stableObject.AddComponent<Stable>();
            stable.SetData(data);
            stable.SetTeam(team);
            return stable;
        }

        public static Farm CreateFreshFarm(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            if (data == null)
                return null;

            Vector3 worldPosition = ResolveWorldPosition(data, position);
            GameObject farmObject = EntityVisualBuilder.CreateBuildingShell(
                "Farm",
                LayerMask.NameToLayer("Building"),
                worldPosition,
                new Vector3(data.footprintWidth, data.buildingHeight, data.footprintDepth),
                Vector3.zero,
                PlaceholderVisualKind.House);

            ApplySharedMaterialIfMissingRendererTint(farmObject);
            ConfigureBuildingHealth(farmObject, data.maxHp, data.meleeArmor, data.pierceArmor, team);

            Farm farm = farmObject.AddComponent<Farm>();
            farm.SetData(data);
            return farm;
        }

        public static LumberCamp CreateFreshLumberCamp(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            if (data == null)
                return null;

            Vector3 worldPosition = ResolveWorldPosition(data, position);
            GameObject lumberCampObject = EntityVisualBuilder.CreateBuildingShell(
                "LumberCamp",
                LayerMask.NameToLayer("Building"),
                worldPosition,
                new Vector3(data.footprintWidth, data.buildingHeight, data.footprintDepth),
                Vector3.zero,
                PlaceholderVisualKind.House);

            ApplySharedMaterialIfMissingRendererTint(lumberCampObject);
            ConfigureBuildingHealth(lumberCampObject, data.maxHp, data.meleeArmor, data.pierceArmor, team);

            LumberCamp lumberCamp = lumberCampObject.AddComponent<LumberCamp>();
            lumberCamp.SetData(data);
            return lumberCamp;
        }

        public static MiningCamp CreateFreshMiningCamp(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            if (data == null)
                return null;

            Vector3 worldPosition = ResolveWorldPosition(data, position);
            GameObject miningCampObject = EntityVisualBuilder.CreateBuildingShell(
                "MiningCamp",
                LayerMask.NameToLayer("Building"),
                worldPosition,
                new Vector3(data.footprintWidth, data.buildingHeight, data.footprintDepth),
                Vector3.zero,
                PlaceholderVisualKind.House);

            ApplySharedMaterialIfMissingRendererTint(miningCampObject);
            ConfigureBuildingHealth(miningCampObject, data.maxHp, data.meleeArmor, data.pierceArmor, team);

            MiningCamp miningCamp = miningCampObject.AddComponent<MiningCamp>();
            miningCamp.SetData(data);
            return miningCamp;
        }

        public static Mill CreateFreshMill(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            if (data == null)
                return null;

            Vector3 worldPosition = ResolveWorldPosition(data, position);
            GameObject millObject = EntityVisualBuilder.CreateBuildingShell(
                "Mill",
                LayerMask.NameToLayer("Building"),
                worldPosition,
                new Vector3(data.footprintWidth, data.buildingHeight, data.footprintDepth),
                Vector3.zero,
                PlaceholderVisualKind.House);

            ApplySharedMaterialIfMissingRendererTint(millObject);
            ConfigureBuildingHealth(millObject, data.maxHp, data.meleeArmor, data.pierceArmor, team);

            Mill mill = millObject.AddComponent<Mill>();
            mill.SetData(data);
            return mill;
        }

        public static Vector3 ResolveWorldPosition(PlacedBuildingData data, Vector3 position)
        {
            return new Vector3(
                position.x,
                data.buildingHeight * 0.5f + 0.05f,
                position.z);
        }

        static void ConfigureBuildingHealth(
            GameObject buildingObject,
            float hp,
            float buildingMeleeArmor,
            float buildingPierceArmor,
            UnitTeam team)
        {
            BuildingHealth health = buildingObject.AddComponent<BuildingHealth>();
            health.Configure(hp, buildingMeleeArmor, buildingPierceArmor, team, townCenter: false);
        }

        static void ApplySharedMaterialIfMissingRendererTint(GameObject buildingObject)
        {
            Renderer renderer = buildingObject.GetComponentInChildren<Renderer>();
            if (renderer == null)
                return;

            EnsureSharedMaterial();
            renderer.sharedMaterial = sharedLitMaterial;
        }

        static void EnsureSharedMaterial()
        {
            if (sharedLitMaterial != null)
                return;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            sharedLitMaterial = new Material(shader);
        }

        public static Material GetSharedLitMaterial()
        {
            EnsureSharedMaterial();
            return sharedLitMaterial;
        }
    }
}
