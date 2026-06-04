using UnityEngine;

namespace AoE.RTS.Buildings
{
    public static class RuntimeBuildingFactory
    {
        static Material sharedLitMaterial;

        public static House CreateHouse(PlacedBuildingData data, Vector3 position)
        {
            if (data == null)
                return null;

            GameObject houseObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            houseObject.name = "House";
            houseObject.layer = LayerMask.NameToLayer("Building");
            houseObject.transform.localScale = new Vector3(data.footprintWidth, data.buildingHeight, data.footprintDepth);
            houseObject.transform.position = new Vector3(
                position.x,
                data.buildingHeight * 0.5f + 0.05f,
                position.z);

            EnsureSharedMaterial();
            houseObject.GetComponent<Renderer>().sharedMaterial = sharedLitMaterial;

            House house = houseObject.AddComponent<House>();
            house.SetData(data);
            return house;
        }

        public static Barracks CreateBarracks(PlacedBuildingData data, Vector3 position)
        {
            if (data == null)
                return null;

            GameObject barracksObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barracksObject.name = "Barracks";
            barracksObject.layer = LayerMask.NameToLayer("Building");
            barracksObject.transform.localScale = new Vector3(data.footprintWidth, data.buildingHeight, data.footprintDepth);
            barracksObject.transform.position = new Vector3(
                position.x,
                data.buildingHeight * 0.5f + 0.05f,
                position.z);

            EnsureSharedMaterial();
            barracksObject.GetComponent<Renderer>().sharedMaterial = sharedLitMaterial;

            Barracks barracks = barracksObject.AddComponent<Barracks>();
            barracks.SetData(data);
            return barracks;
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
