using System.Collections.Generic;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Input;
using AoE.RTS.Units;
using AoE.RTS.Visuals;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AoE.RTS.Buildings
{
    public class BuildingPlacementManager : MonoBehaviour, ISimulationTickable
    {
        struct ConstructionSite
        {
            public PlacedBuildingData data;
            public Unit builder;
            public Vector3 position;
            public float remainingTime;
            public bool builderArrived;
            public GameObject siteVisual;
        }

        const float BuilderReachPadding = 1.5f;
        const float ApproachReachDistance = 2.5f;
        const float GroundRayDistance = 1000f;
        const float MinSiteSeparation = 5f;

        static BuildingPlacementManager instance;

        [SerializeField] UnityEngine.Camera mainCamera;
        [SerializeField] RTSInputReader input;
        [SerializeField] PlacedBuildingData houseData;
        [SerializeField] PlacedBuildingData barracksData;
        [SerializeField] PlacedBuildingData farmData;

        PlacedBuildingData activePlacementData;

        readonly List<ConstructionSite> sites = new List<ConstructionSite>();
        readonly List<Unit> stashedBuilders = new List<Unit>();
        readonly List<Unit> builderLookupBuffer = new List<Unit>();

        GameObject ghostObject;
        Renderer ghostRenderer;
        MaterialPropertyBlock ghostPropertyBlock;
        Vector3 ghostPosition;
        bool ghostValid;
        bool isPlacementModeActive;
        int placementOpenedFrame = -1;

        public static bool IsPlacementModeActive => instance != null && instance.isPlacementModeActive;

        public static bool IsUnitBuilding(Unit unit)
        {
            if (instance == null || unit == null)
                return false;

            for (int i = 0; i < instance.sites.Count; i++)
            {
                if (instance.sites[i].builder == unit)
                    return true;
            }

            return false;
        }

        public static bool HasActiveConstructionForTeam(UnitTeam team)
        {
            if (instance == null)
                return false;

            for (int i = 0; i < instance.sites.Count; i++)
            {
                Unit builder = instance.sites[i].builder;
                if (builder != null && builder.Team == team)
                    return true;
            }

            return false;
        }

        public static bool HasActiveBarracksConstructionForTeam(UnitTeam team)
        {
            if (instance == null)
                return false;

            for (int i = 0; i < instance.sites.Count; i++)
            {
                ConstructionSite site = instance.sites[i];
                if (site.builder == null || site.builder.Team != team)
                    continue;

                if (site.data != null && site.data.kind == PlacedBuildingKind.Barracks)
                    return true;
            }

            return false;
        }

        void Awake()
        {
            instance = this;
            houseData = PlacedBuildingDataResolver.ResolveHouse(ref houseData);
            barracksData = PlacedBuildingDataResolver.ResolveBarracks(ref barracksData);
            farmData = PlacedBuildingDataResolver.ResolveFarm(ref farmData);
            if (mainCamera == null)
                mainCamera = UnityEngine.Camera.main;
            if (input == null)
                input = FindAnyObjectByType<RTSInputReader>();
            CreateGhost();
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;

            SimulationTick.Unregister(this);

            if (ghostObject != null)
                Destroy(ghostObject);
        }

        void Update()
        {
            if (GameSessionManager.IsGameOver)
                return;

            if (isPlacementModeActive)
                UpdatePlacementMode();
        }

        void Start()
        {
            SimulationTick.Register(this);
        }

        public void TickSimulation(float fixedDeltaTime)
        {
            TickConstructionSites(fixedDeltaTime);
        }

        public static void EnterHousePlacementMode(IReadOnlyList<Unit> builders)
        {
            if (instance == null || GameSessionManager.IsGameOver)
                return;

            instance.houseData = PlacedBuildingDataResolver.ResolveHouse(ref instance.houseData);
            if (instance.houseData == null)
                return;

            instance.activePlacementData = instance.houseData;
            instance.stashedBuilders.Clear();
            if (builders != null)
            {
                for (int i = 0; i < builders.Count; i++)
                {
                    Unit unit = builders[i];
                    if (unit != null && unit.IsAlive && unit.Team == UnitTeam.Player)
                        instance.stashedBuilders.Add(unit);
                }
            }

            instance.isPlacementModeActive = true;
            instance.placementOpenedFrame = Time.frameCount;
            instance.ghostObject.SetActive(true);
            instance.RefreshGhostFromPointer();
        }

        public static void EnterHousePlacementMode()
        {
            EnterHousePlacementMode(null);
        }

        public static void EnterBarracksPlacementMode(IReadOnlyList<Unit> builders)
        {
            if (instance == null || GameSessionManager.IsGameOver)
                return;

            instance.barracksData = PlacedBuildingDataResolver.ResolveBarracks(ref instance.barracksData);
            if (instance.barracksData == null)
                return;

            instance.stashedBuilders.Clear();
            if (builders != null)
            {
                for (int i = 0; i < builders.Count; i++)
                {
                    Unit unit = builders[i];
                    if (unit != null && unit.IsAlive && unit.Team == UnitTeam.Player)
                        instance.stashedBuilders.Add(unit);
                }
            }

            instance.activePlacementData = instance.barracksData;
            instance.isPlacementModeActive = true;
            instance.placementOpenedFrame = Time.frameCount;
            instance.ghostObject.SetActive(true);
            instance.RefreshGhostFromPointer();
        }

        public static void EnterFarmPlacementMode(IReadOnlyList<Unit> builders)
        {
            if (instance == null || GameSessionManager.IsGameOver)
                return;

            instance.farmData = PlacedBuildingDataResolver.ResolveFarm(ref instance.farmData);
            if (instance.farmData == null)
                return;

            instance.stashedBuilders.Clear();
            if (builders != null)
            {
                for (int i = 0; i < builders.Count; i++)
                {
                    Unit unit = builders[i];
                    if (unit != null && unit.IsAlive && unit.Team == UnitTeam.Player)
                        instance.stashedBuilders.Add(unit);
                }
            }

            instance.activePlacementData = instance.farmData;
            instance.isPlacementModeActive = true;
            instance.placementOpenedFrame = Time.frameCount;
            instance.ghostObject.SetActive(true);
            instance.RefreshGhostFromPointer();
        }

        public static void CancelPlacementMode()
        {
            if (instance == null)
                return;

            instance.isPlacementModeActive = false;
            instance.activePlacementData = null;
            instance.ghostObject.SetActive(false);
            instance.stashedBuilders.Clear();
            instance.placementOpenedFrame = -1;
        }

        public static bool TryConfirmPlacement(IReadOnlyList<Unit> builders)
        {
            if (instance == null || !instance.isPlacementModeActive || instance.activePlacementData == null)
                return false;

            if (Time.frameCount <= instance.placementOpenedFrame)
                return false;

            Unit builder = ResolveBuilder(builders);
            if (builder == null)
                return false;

            if (!instance.TryGetPointerPlacementPosition(out Vector3 placementPosition))
                return false;

            PlacedBuildingData placementData = instance.activePlacementData;
            instance.ghostPosition = placementPosition;
            instance.ghostValid = instance.CanPlaceAt(placementPosition, placementData);
            if (!instance.ghostValid)
                return false;

            if (!ResourceManager.TrySpendWood(placementData.woodCost))
                return false;

            instance.builderLookupBuffer.Clear();
            instance.builderLookupBuffer.Add(builder);
            GatherManager.CancelForUnits(instance.builderLookupBuffer);
            FoodGatherManager.CancelForUnits(instance.builderLookupBuffer);
            instance.RemoveIncompleteSitesForBuilder(builder);

            Vector3 approach = instance.GetBuildApproachPosition(instance.ghostPosition, placementData, builder);
            builder.SetMoveTarget(approach);

            instance.sites.Add(new ConstructionSite
            {
                data = placementData,
                builder = builder,
                position = instance.ghostPosition,
                remainingTime = placementData.buildTime,
                builderArrived = false,
                siteVisual = instance.CreateConstructionVisual(placementData, instance.ghostPosition)
            });

            CancelPlacementMode();
            return true;
        }

        public static bool TryStartTeamConstruction(PlacedBuildingData data, Vector3 position, Unit builder)
        {
            if (instance == null || data == null || builder == null || !builder.IsAlive)
                return false;

            position = instance.SnapToFootprint(position);
            if (!instance.CanPlaceAt(position, data))
                return false;

            if (!ResourceManager.TrySpendWood(builder.Team, data.woodCost))
                return false;

            instance.builderLookupBuffer.Clear();
            instance.builderLookupBuffer.Add(builder);
            GatherManager.CancelForUnits(instance.builderLookupBuffer);
            FoodGatherManager.CancelForUnits(instance.builderLookupBuffer);
            instance.RemoveIncompleteSitesForBuilder(builder);

            Vector3 approach = instance.GetBuildApproachPosition(position, data, builder);
            builder.SetMoveTarget(approach);

            instance.sites.Add(new ConstructionSite
            {
                data = data,
                builder = builder,
                position = position,
                remainingTime = data.buildTime,
                builderArrived = false,
                siteVisual = instance.CreateConstructionVisual(data, position)
            });

            return true;
        }

        public static bool TryFindPlacementNear(
            Vector3 center,
            float minRadius,
            float maxRadius,
            PlacedBuildingData data,
            out Vector3 placementPosition)
        {
            placementPosition = center;
            if (instance == null || data == null)
                return false;

            const int angleSteps = 16;
            for (float radius = minRadius; radius <= maxRadius; radius += 2f)
            {
                for (int step = 0; step < angleSteps; step++)
                {
                    float angle = step * (Mathf.PI * 2f / angleSteps);
                    Vector3 candidate = new Vector3(
                        center.x + Mathf.Cos(angle) * radius,
                        0f,
                        center.z + Mathf.Sin(angle) * radius);

                    candidate = instance.SnapToFootprint(candidate);
                    if (instance.CanPlaceAt(candidate, data))
                    {
                        placementPosition = candidate;
                        return true;
                    }
                }
            }

            return false;
        }

        static Unit ResolveBuilder(IReadOnlyList<Unit> builders)
        {
            Unit builder = PickPlayerBuilder(instance.stashedBuilders);
            if (builder != null)
                return builder;

            builder = PickPlayerBuilder(builders);
            if (builder != null)
                return builder;

            return null;
        }

        static Unit PickPlayerBuilder(IReadOnlyList<Unit> units)
        {
            if (units == null)
                return null;

            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                if (unit != null && unit.IsAlive && unit.Team == UnitTeam.Player)
                    return unit;
            }

            return null;
        }

        void UpdatePlacementMode()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CancelPlacementMode();
                return;
            }

            RefreshGhostFromPointer();
        }

        void RefreshGhostFromPointer()
        {
            if (mainCamera == null || input == null || activePlacementData == null)
                return;

            if (!TryGetPointerPlacementPosition(out Vector3 placementPosition))
                return;

            ghostPosition = placementPosition;
            ghostValid = CanPlaceAt(ghostPosition, activePlacementData);
            UpdateGhostVisual(activePlacementData, ghostPosition, ghostValid);
        }

        bool TryGetPointerPlacementPosition(out Vector3 placementPosition)
        {
            placementPosition = ghostPosition;
            if (mainCamera == null || input == null)
                return false;

            Ray ray = mainCamera.ScreenPointToRay(input.PointerScreenPosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, GroundRayDistance, GameLayers.GroundMask))
                return false;

            placementPosition = SnapToFootprint(hit.point);
            return true;
        }

        void TickConstructionSites(float deltaTime)
        {
            for (int i = sites.Count - 1; i >= 0; i--)
            {
                ConstructionSite site = sites[i];
                if (site.builder == null)
                {
                    DestroySiteVisual(site.siteVisual);
                    sites.RemoveAt(i);
                    continue;
                }

                if (!site.builderArrived)
                {
                    PlacedBuildingData siteData = site.data ?? activePlacementData ?? houseData;
                    if (HasBuilderArrived(site.builder, site.position, siteData))
                    {
                        site.builder.ClearMoveTarget();
                        site.builderArrived = true;
                        sites[i] = site;
                    }
                    else if (!site.builder.HasMoveTarget)
                    {
                        site.builder.SetMoveTarget(GetBuildApproachPosition(site.position, siteData, site.builder));
                    }

                    continue;
                }

                PlacedBuildingData buildData = site.data ?? activePlacementData ?? houseData;
                if (!HasBuilderArrived(site.builder, site.position, buildData))
                    continue;

                site.remainingTime -= deltaTime;
                if (site.remainingTime > 0f)
                {
                    sites[i] = site;
                    continue;
                }

                CompleteConstruction(ref site);
                sites.RemoveAt(i);
            }
        }

        public static void AbortConstructionForUnits(IReadOnlyList<Unit> units)
        {
            if (instance == null || units == null || units.Count == 0)
                return;

            for (int i = instance.sites.Count - 1; i >= 0; i--)
            {
                Unit siteBuilder = instance.sites[i].builder;
                if (siteBuilder == null)
                    continue;

                for (int u = 0; u < units.Count; u++)
                {
                    if (units[u] != siteBuilder)
                        continue;

                    DestroySiteVisual(instance.sites[i].siteVisual);
                    instance.sites.RemoveAt(i);
                    break;
                }
            }
        }

        static readonly List<Unit> singleUnitAbortBuffer = new List<Unit>(1);

        public static void AbortConstructionForUnit(Unit unit)
        {
            if (unit == null)
                return;

            singleUnitAbortBuffer.Clear();
            singleUnitAbortBuffer.Add(unit);
            AbortConstructionForUnits(singleUnitAbortBuffer);
        }

        void CompleteConstruction(ref ConstructionSite site)
        {
            DestroySiteVisual(site.siteVisual);
            site.builder.ClearMoveTarget();

            if (site.data == null)
                return;

            if (site.data.kind == PlacedBuildingKind.Barracks)
            {
                UnitTeam team = site.builder != null ? site.builder.Team : UnitTeam.Player;
                Barracks barracks = RuntimeBuildingFactory.CreateBarracks(site.data, site.position, team);
                if (barracks != null && site.builder != null)
                    barracks.SetTeam(site.builder.Team);
                return;
            }

            if (site.data.kind == PlacedBuildingKind.Farm)
            {
                UnitTeam team = site.builder != null ? site.builder.Team : UnitTeam.Player;
                RuntimeBuildingFactory.CreateFarm(site.data, site.position, team);
                return;
            }

            UnitTeam houseTeam = site.builder != null ? site.builder.Team : UnitTeam.Player;
            RuntimeBuildingFactory.CreateHouse(site.data, site.position, houseTeam);
            if (site.data.housingProvided > 0 && site.builder != null)
                PopulationManager.AddHousing(site.builder.Team, site.data.housingProvided);
        }

        Vector3 SnapToFootprint(Vector3 worldPoint)
        {
            return new Vector3(worldPoint.x, 0f, worldPoint.z);
        }

        Vector3 GetBuildApproachPosition(Vector3 sitePosition, PlacedBuildingData data, Unit builder)
        {
            const float buildStandRadius = 2.5f;
            float offset = data != null ? data.footprintDepth * 0.5f + 2f : 4f;
            Vector3 approach = new Vector3(sitePosition.x, 1f, sitePosition.z + offset);
            return UnitPositionOffsets.ApplyRingOffset(approach, builder, buildStandRadius);
        }

        bool HasBuilderArrived(Unit builder, Vector3 sitePosition, PlacedBuildingData data)
        {
            if (builder == null || data == null)
                return false;

            Vector3 approach = GetBuildApproachPosition(sitePosition, data, builder);
            if (builder.IsNear(approach, ApproachReachDistance))
                return true;

            float reach = Mathf.Max(data.footprintWidth, data.footprintDepth) * 0.5f + BuilderReachPadding;
            Vector3 siteCenter = new Vector3(sitePosition.x, 1f, sitePosition.z);
            return builder.IsNear(siteCenter, reach);
        }

        void RemoveIncompleteSitesForBuilder(Unit builder)
        {
            for (int i = sites.Count - 1; i >= 0; i--)
            {
                if (sites[i].builder != builder)
                    continue;

                DestroySiteVisual(sites[i].siteVisual);
                sites.RemoveAt(i);
            }
        }

        bool IsSitePositionOccupied(Vector3 position)
        {
            for (int i = 0; i < sites.Count; i++)
            {
                Vector3 delta = sites[i].position - position;
                delta.y = 0f;
                if (delta.sqrMagnitude < MinSiteSeparation * MinSiteSeparation)
                    return true;
            }

            return false;
        }

        bool CanPlaceAt(Vector3 position, PlacedBuildingData data)
        {
            if (data == null)
                return false;

            Vector3 halfExtents = new Vector3(
                data.footprintWidth * 0.5f,
                data.buildingHeight * 0.5f,
                data.footprintDepth * 0.5f);

            Vector3 center = new Vector3(
                position.x,
                data.buildingHeight * 0.5f + 0.05f,
                position.z);

            int mask = GameLayers.BuildingMask | GameLayers.ResourceMask;
            Collider[] overlaps = Physics.OverlapBox(center, halfExtents, Quaternion.identity, mask);
            if (overlaps.Length > 0)
                return false;

            return !IsSitePositionOccupied(position);
        }

        void CreateGhost()
        {
            ghostObject = new GameObject("PlacementGhost");
            ghostObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            ghostObject.SetActive(false);
            ghostRenderer = null;
        }

        void UpdateGhostVisual(PlacedBuildingData data, Vector3 position, bool valid)
        {
            if (ghostObject == null || data == null)
                return;

            for (int i = ghostObject.transform.childCount - 1; i >= 0; i--)
                Destroy(ghostObject.transform.GetChild(i).gameObject);

            Vector3 fallbackScale = new Vector3(data.footprintWidth, data.buildingHeight, data.footprintDepth);
            PlaceholderVisualKind kind = EntityVisualBuilder.GetBuildingVisualKind(data);
            GameObject visual = EntityVisualBuilder.CreateGhostVisual(kind, fallbackScale);
            visual.transform.SetParent(ghostObject.transform, false);
            visual.transform.localPosition = Vector3.zero;

            ghostObject.transform.position = new Vector3(
                position.x,
                data.buildingHeight * 0.5f + 0.05f,
                position.z);

            ghostRenderer = visual.GetComponentInChildren<Renderer>();
            if (ghostRenderer == null)
                return;

            if (ghostPropertyBlock == null)
                ghostPropertyBlock = new MaterialPropertyBlock();

            Color color = valid ? data.ghostValidColor : data.ghostInvalidColor;
            ghostRenderer.GetPropertyBlock(ghostPropertyBlock);
            ghostPropertyBlock.SetColor("_BaseColor", color);
            ghostPropertyBlock.SetColor("_Color", color);
            ghostRenderer.SetPropertyBlock(ghostPropertyBlock);
        }

        GameObject CreateConstructionVisual(PlacedBuildingData data, Vector3 position)
        {
            GameObject siteRoot = new GameObject("ConstructionSite");
            siteRoot.layer = LayerMask.NameToLayer("Building");
            siteRoot.transform.position = new Vector3(
                position.x,
                data.buildingHeight * 0.5f + 0.05f,
                position.z);

            Vector3 fallbackScale = new Vector3(data.footprintWidth, data.buildingHeight, data.footprintDepth);
            PlaceholderVisualKind kind = EntityVisualBuilder.GetBuildingVisualKind(data);
            GameObject visual = EntityVisualBuilder.CreateGhostVisual(kind, fallbackScale);
            visual.transform.SetParent(siteRoot.transform, false);
            visual.transform.localScale = Vector3.one;
            visual.transform.localPosition = Vector3.zero;

            Renderer renderer = visual.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(block);
                block.SetColor("_BaseColor", data.constructionColor);
                block.SetColor("_Color", data.constructionColor);
                renderer.SetPropertyBlock(block);
            }

            return siteRoot;
        }

        static void DestroySiteVisual(GameObject visual)
        {
            if (visual != null)
                Destroy(visual);
        }
    }
}
