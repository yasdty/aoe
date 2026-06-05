using AoE.RTS.Core;
using UnityEngine;

namespace AoE.RTS.Visuals
{
    public static class EntityVisualBuilder
    {
        const string VisualChildName = "Visual";

        public static GameObject CreateUnitShell(string name, Vector3 position, PlaceholderVisualKind visualKind)
        {
            GameObject root = new GameObject(name);
            root.layer = LayerMask.NameToLayer(GameLayers.UnitLayerName);
            root.transform.position = position;
            EnsureUnitCollider(root);
            AttachVisualOrFallback(root, visualKind, CreateCapsuleFallback);
            return root;
        }

        public static GameObject CreateBuildingShell(
            string name,
            int layer,
            Vector3 position,
            Vector3 boxSize,
            Vector3 boxCenter,
            PlaceholderVisualKind visualKind)
        {
            GameObject root = new GameObject(name);
            root.layer = layer;
            root.transform.position = position;
            EnsureBoxCollider(root, boxSize, boxCenter);
            AttachVisualOrFallback(root, visualKind, () => CreateCubeFallback(boxSize));
            return root;
        }

        public static GameObject CreateTreeShell(
            string name,
            Vector3 position,
            float height,
            float radius,
            PlaceholderVisualKind visualKind)
        {
            GameObject root = new GameObject(name);
            root.layer = LayerMask.NameToLayer(GameLayers.ResourceLayerName);
            root.transform.position = position;
            EnsureTreeCollider(root, height, radius);
            AttachVisualOrFallback(root, visualKind, () => CreateCylinderFallback(height, radius));
            return root;
        }

        public static PlaceholderVisualKind GetUnitVisualKind(Units.UnitData unitData)
        {
            if (unitData != null && unitData.CanAttack)
                return PlaceholderVisualKind.Militia;

            return PlaceholderVisualKind.Villager;
        }

        public static PlaceholderVisualKind GetBuildingVisualKind(Buildings.PlacedBuildingData data)
        {
            if (data != null && data.kind == Buildings.PlacedBuildingKind.Barracks)
                return PlaceholderVisualKind.Barracks;

            return PlaceholderVisualKind.House;
        }
        public static bool TryAttachVisual(GameObject root, PlaceholderVisualKind kind)
        {
            return AttachVisualOrFallback(root, kind, null);
        }

        static GameObject LoadVisualPrefab(PlaceholderVisualKind kind)
        {
            return Resources.Load<GameObject>(PlaceholderVisualCatalog.GetResourcePath(kind));
        }

        static bool AttachVisualOrFallback(
            GameObject root,
            PlaceholderVisualKind kind,
            System.Func<GameObject> fallbackFactory)
        {
            if (root == null)
                return false;

            RemoveExistingVisual(root);

            GameObject prefab = LoadVisualPrefab(kind);
            if (prefab != null)
            {
                GameObject visual = Object.Instantiate(prefab, root.transform);
                visual.name = VisualChildName;
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
                visual.transform.localScale = Vector3.one;
                DisableColliders(visual);
                return true;
            }

            if (fallbackFactory == null)
                return false;

            GameObject fallback = fallbackFactory();
            fallback.name = VisualChildName;
            fallback.transform.SetParent(root.transform, false);
            RemoveCollider(fallback);
            return false;
        }

        static GameObject CreateCapsuleFallback()
        {
            GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            fallback.transform.localPosition = new Vector3(0f, 1f, 0f);
            return fallback;
        }

        static GameObject CreateCubeFallback(Vector3 size)
        {
            GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fallback.transform.localScale = size;
            return fallback;
        }

        static GameObject CreateCylinderFallback(float height, float radius)
        {
            GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fallback.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
            fallback.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            return fallback;
        }

        public static GameObject CreateGhostVisual(PlaceholderVisualKind kind, Vector3 fallbackScale)
        {
            GameObject prefab = LoadVisualPrefab(kind);
            if (prefab == null)
            {
                GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fallback.transform.localScale = fallbackScale;
                RemoveCollider(fallback);
                return fallback;
            }

            GameObject ghost = Object.Instantiate(prefab);
            ghost.name = "GhostVisual";
            ghost.transform.localScale = Vector3.one;
            DisableColliders(ghost);
            return ghost;
        }

        public static void EnsureUnitCollider(GameObject root)
        {
            CapsuleCollider collider = root.GetComponent<CapsuleCollider>();
            if (collider == null)
                collider = root.AddComponent<CapsuleCollider>();

            collider.height = 2f;
            collider.radius = 0.5f;
            collider.center = new Vector3(0f, 1f, 0f);
        }

        public static void EnsureBoxCollider(GameObject root, Vector3 size, Vector3 center)
        {
            BoxCollider collider = root.GetComponent<BoxCollider>();
            if (collider == null)
                collider = root.AddComponent<BoxCollider>();

            collider.size = size;
            collider.center = center;
        }

        public static void EnsureTreeCollider(GameObject root, float height, float radius)
        {
            CapsuleCollider collider = root.GetComponent<CapsuleCollider>();
            if (collider == null)
                collider = root.AddComponent<CapsuleCollider>();

            collider.height = height;
            collider.radius = radius;
            collider.center = new Vector3(0f, height * 0.5f, 0f);
        }

        static void RemoveExistingVisual(GameObject root)
        {
            Transform existing = root.transform.Find(VisualChildName);
            if (existing != null)
                DestroyObject(existing.gameObject);
        }

        static void DisableColliders(GameObject visualRoot)
        {
            Collider[] colliders = visualRoot.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
                colliders[i].enabled = false;
        }

        static void RemoveCollider(GameObject objectWithCollider)
        {
            Collider collider = objectWithCollider.GetComponent<Collider>();
            if (collider != null)
                DestroyObject(collider);
        }

        static void DestroyObject(Object target)
        {
            if (target == null)
                return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Object.DestroyImmediate(target);
                return;
            }
#endif
            Object.Destroy(target);
        }
    }
}
