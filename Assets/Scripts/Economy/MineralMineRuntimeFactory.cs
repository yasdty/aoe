using AoE.RTS.Core;
using UnityEngine;

namespace AoE.RTS.Economy
{
    public static class MineralMineRuntimeFactory
    {
        const float Size = 3f;
        const float GroundClearance = 0.05f;

        public static GoldMineResource CreateGoldMine(Vector3 groundPosition, MineralNodeData data = null)
        {
            return CreateMine<GoldMineResource>("GoldMine", groundPosition, data ?? CreateDefaultGoldData());
        }

        public static StoneMineResource CreateStoneMine(Vector3 groundPosition, MineralNodeData data = null)
        {
            return CreateMine<StoneMineResource>("StoneMine", groundPosition, data ?? CreateDefaultStoneData());
        }

        static T CreateMine<T>(string objectName, Vector3 groundPosition, MineralNodeData data) where T : MonoBehaviour
        {
            Vector3 worldPosition = new Vector3(
                groundPosition.x,
                Size * 0.5f + GroundClearance,
                groundPosition.z);

            GameObject mineObject = new GameObject(objectName);
            int resourceLayer = LayerMask.NameToLayer(GameLayers.ResourceLayerName);
            if (resourceLayer >= 0)
                mineObject.layer = resourceLayer;

            mineObject.transform.position = worldPosition;

            BoxCollider collider = mineObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(Size, Size, Size);

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            visual.transform.SetParent(mineObject.transform, false);
            visual.transform.localScale = Vector3.one * Size;
            Object.Destroy(visual.GetComponent<Collider>());

            T resource = mineObject.AddComponent<T>();
            if (resource is GoldMineResource goldMine)
                goldMine.SetData(data);
            else if (resource is StoneMineResource stoneMine)
                stoneMine.SetData(data);

            return resource;
        }

        static MineralNodeData CreateDefaultGoldData()
        {
            MineralNodeData data = ScriptableObject.CreateInstance<MineralNodeData>();
            data.displayName = "Gold Mine";
            data.initialAmount = 800f;
            data.defaultColor = new Color(0.85f, 0.72f, 0.2f);
            return data;
        }

        static MineralNodeData CreateDefaultStoneData()
        {
            MineralNodeData data = ScriptableObject.CreateInstance<MineralNodeData>();
            data.displayName = "Stone Mine";
            data.initialAmount = 350f;
            data.defaultColor = new Color(0.55f, 0.55f, 0.58f);
            return data;
        }
    }
}
