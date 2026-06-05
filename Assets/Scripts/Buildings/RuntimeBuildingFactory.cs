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
            if (data == null)
                return null;

            Vector3 worldPosition = new Vector3(
                position.x,
                data.buildingHeight * 0.5f + 0.05f,
                position.z);

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

        public static Barracks CreateBarracks(PlacedBuildingData data, Vector3 position, UnitTeam team = UnitTeam.Player)
        {
            if (data == null)
                return null;

            Vector3 worldPosition = new Vector3(
                position.x,
                data.buildingHeight * 0.5f + 0.05f,
                position.z);

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
            return barracks;
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
