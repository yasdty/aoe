using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Camera;
using AoE.RTS.Spatial;
using AoE.RTS.Units;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AoE.RTS.Selection
{
    public static class MinimapIconRegistry
    {
        public struct Entry
        {
            public Transform transform;
            public Color color;
        }

        public static void Collect(List<Entry> buffer)
        {
            buffer.Clear();

            TownCenter playerTownCenter = ProductionManager.GetTownCenterForTeam(UnitTeam.Player);
            if (playerTownCenter != null)
            {
                buffer.Add(new Entry
                {
                    transform = playerTownCenter.transform,
                    color = new Color(0.25f, 0.55f, 1f, 1f)
                });
            }

            TownCenter enemyTownCenter = ProductionManager.GetTownCenterForTeam(UnitTeam.Enemy);
            if (enemyTownCenter != null)
            {
                buffer.Add(new Entry
                {
                    transform = enemyTownCenter.transform,
                    color = new Color(1f, 0.3f, 0.3f, 1f)
                });
            }
        }
    }

    public class MinimapView : MonoBehaviour
    {
        const float PanelWidth = 180f;
        const float PanelHeight = 160f;
        const float CpuPanelHeight = 8f * 2f + 22f * 5f;
        const float GapBelowCpuHud = 8f;
        const float IconSize = 10f;
        const float MapPadding = 4f;

        static readonly Color MapBackgroundColor = new Color(0.12f, 0.28f, 0.12f, 0.95f);
        static readonly Color ViewportFillColor = new Color(1f, 1f, 0.35f, 0.18f);

        [SerializeField] UnityEngine.Camera mainCamera;
        [SerializeField] RTSCameraController cameraController;

        RectTransform panelRoot;
        RectTransform mapRect;
        RectTransform iconsRoot;
        RectTransform viewportRect;
        Image playerIcon;
        Image enemyIcon;
        MinimapClickHandler clickHandler;
        bool uiBuilt;
        static Texture2D solidTexture;

        void Awake()
        {
            if (mainCamera == null)
                mainCamera = UnityEngine.Camera.main;
            if (cameraController == null)
                cameraController = FindAnyObjectByType<RTSCameraController>();

            GameObject ground = GameObject.Find("Ground");
            if (ground != null)
                MapBounds.ConfigureFromGroundTransform(ground.transform);
            else
                MapBounds.ResetToPhase10Defaults();

            TryBuildUi();
        }

        void OnDestroy()
        {
            if (panelRoot != null)
                GameUiInput.UnregisterHudPanel(panelRoot);
        }

        void LateUpdate()
        {
            if (!uiBuilt)
                TryBuildUi();

            if (!uiBuilt)
                return;

            UpdateIcons();
            UpdateCameraViewport();
        }

        void TryBuildUi()
        {
            if (uiBuilt)
                return;

            Transform hudRoot = HudUiFactory.GetHudRoot();
            if (hudRoot == null)
                return;

            float topOffset = HudUiFactory.Margin + CpuPanelHeight + GapBelowCpuHud;
            panelRoot = HudUiFactory.SetupScreenPanelTopRight(
                hudRoot,
                "MinimapPanel",
                HudUiFactory.PanelBackgroundColor,
                HudUiFactory.Margin,
                topOffset,
                PanelWidth,
                PanelHeight);
            GameUiInput.RegisterHudPanel(panelRoot);

            RectTransform mapHost = HudUiFactory.CreatePanel(panelRoot, "MapHost", Color.clear);
            HudUiFactory.SetStretchFull(mapHost);
            mapHost.offsetMin = new Vector2(MapPadding, MapPadding);
            mapHost.offsetMax = new Vector2(-MapPadding, -MapPadding);

            GameObject mapObject = new GameObject("Map", typeof(RectTransform));
            mapObject.transform.SetParent(mapHost, false);
            mapRect = mapObject.GetComponent<RectTransform>();
            HudUiFactory.SetStretchFull(mapRect);

            RawImage mapImage = mapObject.AddComponent<RawImage>();
            mapImage.texture = GetSolidTexture(MapBackgroundColor);
            mapImage.raycastTarget = true;

            GameObject iconsObject = new GameObject("Icons", typeof(RectTransform));
            iconsObject.transform.SetParent(mapRect, false);
            iconsRoot = iconsObject.GetComponent<RectTransform>();
            HudUiFactory.SetStretchFull(iconsRoot);

            playerIcon = CreateIcon(iconsRoot, "PlayerTcIcon", new Color(0.25f, 0.55f, 1f, 1f));
            enemyIcon = CreateIcon(iconsRoot, "EnemyTcIcon", new Color(1f, 0.3f, 0.3f, 1f));

            GameObject viewportObject = new GameObject("CameraViewport", typeof(RectTransform));
            viewportObject.transform.SetParent(iconsRoot, false);
            viewportRect = viewportObject.GetComponent<RectTransform>();
            Image viewportImage = viewportObject.AddComponent<Image>();
            viewportImage.color = ViewportFillColor;
            viewportImage.raycastTarget = false;

            clickHandler = mapObject.AddComponent<MinimapClickHandler>();
            clickHandler.Initialize(mapRect, cameraController);

            uiBuilt = true;
        }

        static Image CreateIcon(Transform parent, string name, Color color)
        {
            GameObject iconObject = new GameObject(name, typeof(RectTransform));
            iconObject.transform.SetParent(parent, false);
            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(IconSize, IconSize);
            Image image = iconObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        void UpdateIcons()
        {
            UpdateTeamIcon(playerIcon, UnitTeam.Player, new Color(0.25f, 0.55f, 1f, 1f));
            UpdateTeamIcon(enemyIcon, UnitTeam.Enemy, new Color(1f, 0.3f, 0.3f, 1f));
        }

        static void UpdateTeamIcon(Image icon, UnitTeam team, Color color)
        {
            if (icon == null)
                return;

            TownCenter townCenter = ProductionManager.GetTownCenterForTeam(team);
            if (townCenter == null)
            {
                icon.gameObject.SetActive(false);
                return;
            }

            icon.gameObject.SetActive(true);
            icon.color = color;
            SetIconNormalizedPosition(icon.rectTransform, MapBounds.WorldToNormalized01(townCenter.transform.position));
        }

        static void SetIconNormalizedPosition(RectTransform iconRect, Vector2 normalized01)
        {
            iconRect.anchorMin = normalized01;
            iconRect.anchorMax = normalized01;
            iconRect.anchoredPosition = Vector2.zero;
        }

        void UpdateCameraViewport()
        {
            if (viewportRect == null || mainCamera == null)
                return;

            if (!TryGetCameraGroundBounds(mainCamera, out float minX, out float maxX, out float minZ, out float maxZ))
            {
                viewportRect.gameObject.SetActive(false);
                return;
            }

            viewportRect.gameObject.SetActive(true);
            Vector2 uvMin = MapBounds.WorldToNormalized01(new Vector3(minX, 0f, minZ));
            Vector2 uvMax = MapBounds.WorldToNormalized01(new Vector3(maxX, 0f, maxZ));
            float uMin = Mathf.Min(uvMin.x, uvMax.x);
            float uMax = Mathf.Max(uvMin.x, uvMax.x);
            float vMin = Mathf.Min(uvMin.y, uvMax.y);
            float vMax = Mathf.Max(uvMin.y, uvMax.y);
            viewportRect.anchorMin = new Vector2(uMin, vMin);
            viewportRect.anchorMax = new Vector2(uMax, vMax);
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
        }

        static bool TryGetCameraGroundBounds(UnityEngine.Camera camera, out float minX, out float maxX, out float minZ, out float maxZ)
        {
            minX = float.MaxValue;
            maxX = float.MinValue;
            minZ = float.MaxValue;
            maxZ = float.MinValue;
            bool found = false;

            Vector2[] viewportCorners =
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f)
            };

            for (int i = 0; i < viewportCorners.Length; i++)
            {
                Ray ray = camera.ViewportPointToRay(viewportCorners[i]);
                if (Mathf.Abs(ray.direction.y) < 0.0001f)
                    continue;

                float t = -ray.origin.y / ray.direction.y;
                if (t < 0f)
                    continue;

                Vector3 point = ray.GetPoint(t);
                minX = Mathf.Min(minX, point.x);
                maxX = Mathf.Max(maxX, point.x);
                minZ = Mathf.Min(minZ, point.z);
                maxZ = Mathf.Max(maxZ, point.z);
                found = true;
            }

            return found;
        }

        static Texture2D GetSolidTexture(Color color)
        {
            if (solidTexture == null)
            {
                solidTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                solidTexture.hideFlags = HideFlags.HideAndDontSave;
                solidTexture.SetPixel(0, 0, color);
                solidTexture.Apply(false, false);
            }

            return solidTexture;
        }

        sealed class MinimapClickHandler : MonoBehaviour, IPointerClickHandler
        {
            RectTransform mapRectTransform;
            RTSCameraController rtsCameraController;

            public void Initialize(RectTransform mapRectTransform, RTSCameraController rtsCameraController)
            {
                this.mapRectTransform = mapRectTransform;
                this.rtsCameraController = rtsCameraController;
            }

            public void OnPointerClick(PointerEventData eventData)
            {
                if (mapRectTransform == null || rtsCameraController == null)
                    return;

                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        mapRectTransform,
                        eventData.position,
                        eventData.pressEventCamera,
                        out Vector2 local))
                    return;

                Rect rect = mapRectTransform.rect;
                if (rect.width <= 0f || rect.height <= 0f)
                    return;

                float u = Mathf.Clamp01((local.x - rect.xMin) / rect.width);
                float v = Mathf.Clamp01((local.y - rect.yMin) / rect.height);
                Vector3 world = MapBounds.Normalized01ToWorld(new Vector2(u, v));
                rtsCameraController.FocusOnGroundPoint(world);
            }
        }
    }
}
