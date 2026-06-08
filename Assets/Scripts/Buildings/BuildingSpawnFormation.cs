using UnityEngine;

namespace AoE.RTS.Buildings
{
    public static class BuildingSpawnFormation
    {
        public const int MaxSlots = 16;

        public static Vector3 GetGridSlotPosition(
            Vector3 buildingPosition,
            Vector3 exitDirection,
            float halfExtentAlongExit,
            int slotIndex,
            float spacing = 2f,
            float clearance = 1.5f,
            float groundY = 1f)
        {
            Vector3 forward = exitDirection;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.forward;
            else
                forward.Normalize();

            Vector3 right = Vector3.Cross(Vector3.up, forward);
            if (right.sqrMagnitude < 0.0001f)
                right = Vector3.right;
            else
                right.Normalize();

            Vector3 gridAnchor = buildingPosition + forward * (halfExtentAlongExit + clearance);
            gridAnchor.y = groundY;

            int index = ((slotIndex % MaxSlots) + MaxSlots) % MaxSlots;
            GetGridDimensions(MaxSlots, out int columns, out int _);

            float centerColumn = (columns - 1) * 0.5f;

            int column = index % columns;
            int row = index / columns;

            Vector3 offset =
                right * ((column - centerColumn) * spacing) +
                forward * (row * spacing);

            return gridAnchor + offset;
        }

        public static float GetHorizontalHalfExtentAlong(Transform transform, Vector3 worldDirection)
        {
            BoxCollider box = transform.GetComponent<BoxCollider>();
            if (box != null)
            {
                Vector3 localDirection = transform.InverseTransformDirection(worldDirection);
                localDirection.y = 0f;
                if (localDirection.sqrMagnitude < 0.0001f)
                    localDirection = Vector3.forward;
                localDirection.Normalize();

                Vector3 halfSize = Vector3.Scale(box.size * 0.5f, transform.lossyScale);
                return Mathf.Abs(localDirection.x) * halfSize.x + Mathf.Abs(localDirection.z) * halfSize.z;
            }

            Collider collider = transform.GetComponent<Collider>();
            if (collider != null)
            {
                Bounds bounds = collider.bounds;
                return Mathf.Max(bounds.extents.x, bounds.extents.z);
            }

            return 2f;
        }

        static void GetGridDimensions(int count, out int columns, out int rows)
        {
            columns = Mathf.CeilToInt(Mathf.Sqrt(count));
            rows = Mathf.CeilToInt(count / (float)columns);
        }
    }
}
