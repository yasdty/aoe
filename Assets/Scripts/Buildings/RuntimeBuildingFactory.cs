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
            ConfigureBuildingHealth(houseObject, data.maxHp, data.armor, team);

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
            ConfigureBuildingHealth(barracksObject, data.maxHp, data.armor, team);

            Barracks barracks = barracksObject.AddComponent<Barracks>();
            barracks.SetData(data);
            barracks.SetTeam(team);
            return barracks;
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
            ConfigureBuildingHealth(farmObject, data.maxHp, data.armor, team);

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
            ConfigureBuildingHealth(lumberCampObject, data.maxHp, data.armor, team);

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
            ConfigureBuildingHealth(miningCampObject, data.maxHp, data.armor, team);

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
            ConfigureBuildingHealth(millObject, data.maxHp, data.armor, team);

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

        static void ConfigureBuildingHealth(GameObject buildingObject, float hp, float buildingArmor, UnitTeam team)
        {
            BuildingHealth health = buildingObject.AddComponent<BuildingHealth>();
            health.Configure(hp, buildingArmor, team, townCenter: false);
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
