using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Visuals;
using UnityEngine;

namespace AoE.RTS.View
{
    public class PlacementPreviewView : MonoBehaviour, IPlacementPreviewView
    {
        sealed class PreviewSegment
        {
            public GameObject root;
            public Renderer renderer;
            public MaterialPropertyBlock propertyBlock;
        }

        readonly List<PreviewSegment> segmentPool = new List<PreviewSegment>();
        Transform poolRoot;

        void Awake()
        {
            poolRoot = new GameObject("PlacementPreviewPool").transform;
            poolRoot.SetParent(transform, false);
            PlacementPreviewViewRegistry.Register(this);
        }

        void OnDestroy()
        {
            PlacementPreviewViewRegistry.Unregister(this);
        }

        public void ShowSinglePreview(PlacementPreviewState state)
        {
            if (state.data == null)
            {
                HidePreview();
                return;
            }

            EnsurePoolSize(1);
            ApplySegment(segmentPool[0], state);
            for (int i = 1; i < segmentPool.Count; i++)
                segmentPool[i].root.SetActive(false);
        }

        public void ShowWallLinePreview(IReadOnlyList<PlacementPreviewState> segments)
        {
            if (segments == null || segments.Count == 0)
            {
                HidePreview();
                return;
            }

            EnsurePoolSize(segments.Count);
            for (int i = 0; i < segments.Count; i++)
                ApplySegment(segmentPool[i], segments[i]);

            for (int i = segments.Count; i < segmentPool.Count; i++)
                segmentPool[i].root.SetActive(false);
        }

        public void HidePreview()
        {
            for (int i = 0; i < segmentPool.Count; i++)
                segmentPool[i].root.SetActive(false);
        }

        void EnsurePoolSize(int count)
        {
            while (segmentPool.Count < count)
                segmentPool.Add(CreateSegment());
        }

        PreviewSegment CreateSegment()
        {
            GameObject root = new GameObject($"PlacementPreviewSegment_{segmentPool.Count}");
            root.layer = LayerMask.NameToLayer("Ignore Raycast");
            root.transform.SetParent(poolRoot, false);

            return new PreviewSegment
            {
                root = root,
                propertyBlock = new MaterialPropertyBlock()
            };
        }

        void ApplySegment(PreviewSegment segment, PlacementPreviewState state)
        {
            PlacedBuildingData data = state.data;
            if (data == null)
            {
                segment.root.SetActive(false);
                return;
            }

            for (int i = segment.root.transform.childCount - 1; i >= 0; i--)
                Destroy(segment.root.transform.GetChild(i).gameObject);

            Vector3 fallbackScale = new Vector3(data.footprintWidth, data.buildingHeight, data.footprintDepth);
            PlaceholderVisualKind kind = EntityVisualBuilder.GetBuildingVisualKind(data);
            GameObject visual = EntityVisualBuilder.CreateGhostVisual(kind, fallbackScale);
            visual.transform.SetParent(segment.root.transform, false);
            visual.transform.localPosition = Vector3.zero;

            segment.root.transform.position = new Vector3(
                state.groundPosition.x,
                data.buildingHeight * 0.5f + 0.05f,
                state.groundPosition.z);
            segment.root.transform.rotation = Quaternion.Euler(0f, state.wallOrientationY, 0f);
            segment.root.SetActive(true);

            segment.renderer = visual.GetComponentInChildren<Renderer>();
            if (segment.renderer == null)
                return;

            Color color = state.valid ? data.ghostValidColor : data.ghostInvalidColor;
            segment.renderer.GetPropertyBlock(segment.propertyBlock);
            segment.propertyBlock.SetColor("_BaseColor", color);
            segment.propertyBlock.SetColor("_Color", color);
            segment.renderer.SetPropertyBlock(segment.propertyBlock);
        }
    }
}
