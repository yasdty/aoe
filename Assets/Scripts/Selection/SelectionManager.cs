using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Commands;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Input;
using AoE.RTS.Spatial;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public class SelectionManager : MonoBehaviour
    {
        [SerializeField] UnityEngine.Camera mainCamera;
        [SerializeField] RTSInputReader input;
        [SerializeField] SelectionBoxView selectionBoxView;
        [SerializeField] float dragThresholdPixels = 8f;
        [SerializeField] float groupMoveSpacing = 2f;

        static SelectionManager instance;

        readonly List<Unit> selectedUnits = new List<Unit>();
        readonly List<Unit> selectionBuffer = new List<Unit>();
        readonly List<Unit> attackCommandBuffer = new List<Unit>();
        readonly List<Unit> gatherFarmBuffer = new List<Unit>();
        readonly List<Unit> gatherMineralBuffer = new List<Unit>();

        TownCenter selectedTownCenter;
        Barracks selectedBarracks;

        Vector2 dragStartScreen;
        bool isDragging;

        public IReadOnlyList<Unit> SelectedUnits => selectedUnits;
        public TownCenter SelectedTownCenter => selectedTownCenter;
        public Barracks SelectedBarracks => selectedBarracks;

        void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public static void HandleUnitDied(Unit unit)
        {
            if (instance == null || unit == null)
                return;

            instance.selectedUnits.Remove(unit);
        }

        void Update()
        {
            if (GameSessionManager.IsGameOver)
                return;

            if (input != null && GameUiInput.IsPointerOverHud(input.PointerScreenPosition))
            {
                if (BuildingPlacementManager.IsPlacementModeActive && input.WasCommandPressedThisFrame())
                    BuildingPlacementManager.CancelPlacementMode();
                return;
            }

            if (input == null || mainCamera == null)
                return;

            if (BuildingPlacementManager.IsPlacementModeActive)
            {
                HandlePlacementModeInput();
                return;
            }

            if (input.WasSelectPressedThisFrame())
            {
                dragStartScreen = input.PointerScreenPosition;
                isDragging = false;
            }

            if (input.IsSelectPressed)
            {
                if (!isDragging)
                {
                    float thresholdSq = dragThresholdPixels * dragThresholdPixels;
                    if ((input.PointerScreenPosition - dragStartScreen).sqrMagnitude >= thresholdSq)
                        isDragging = true;
                }

                if (isDragging)
                    selectionBoxView?.Show(dragStartScreen, input.PointerScreenPosition);
            }

            if (input.WasSelectReleasedThisFrame())
            {
                if (!ResourceHudView.IsPointerOverHud(input.PointerScreenPosition))
                {
                    if (isDragging)
                    {
                        ApplyBoxSelection(dragStartScreen, input.PointerScreenPosition, input.IsShiftHeld);
                        selectionBoxView?.Hide();
                        isDragging = false;
                    }
                    else
                    {
                        HandleClickSelect(input.IsShiftHeld);
                    }
                }
            }

            if (input.WasCommandPressedThisFrame())
                HandleMoveCommand();
        }

        void HandlePlacementModeInput()
        {
            if (input.WasSelectPressedThisFrame() && !ResourceHudView.IsPointerOverHud(input.PointerScreenPosition))
            {
                dragStartScreen = input.PointerScreenPosition;
                isDragging = false;
            }

            if (input.IsSelectPressed && !ResourceHudView.IsPointerOverHud(input.PointerScreenPosition))
            {
                float thresholdSq = dragThresholdPixels * dragThresholdPixels;
                if ((input.PointerScreenPosition - dragStartScreen).sqrMagnitude >= thresholdSq)
                    isDragging = true;
            }

            if (input.WasSelectReleasedThisFrame() && !isDragging
                && !ResourceHudView.IsPointerOverHud(input.PointerScreenPosition))
                CommandQueue.Enqueue(new BuildConfirmCommand(selectedUnits));

            if (input.WasCommandPressedThisFrame())
                BuildingPlacementManager.CancelPlacementMode();
        }

        void HandleClickSelect(bool additive)
        {
            Ray ray = mainCamera.ScreenPointToRay(input.PointerScreenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameLayers.UnitMask))
            {
                Unit unit = hit.collider.GetComponentInParent<Unit>();
                if (unit != null && IsPlayerUnit(unit))
                {
                    ClearBuildingSelection();
                    if (additive)
                        ToggleUnitSelection(unit);
                    else
                        SetSelection(unit);
                    return;
                }

                if (unit != null && !additive)
                {
                    ClearAllSelection();
                    return;
                }
            }

            if (Physics.Raycast(ray, out hit, 1000f, GameLayers.BuildingMask))
            {
                TownCenter townCenter = hit.collider.GetComponentInParent<TownCenter>();
                if (townCenter != null && townCenter.Team == UnitTeam.Player)
                {
                    if (!additive)
                        SetTownCenterSelection(townCenter);
                    return;
                }

                Barracks barracks = hit.collider.GetComponentInParent<Barracks>();
                if (barracks != null && barracks.Team == UnitTeam.Player)
                {
                    if (!additive)
                        SetBarracksSelection(barracks);
                    return;
                }
            }

            if (!additive)
                ClearAllSelection();
        }

        void ApplyBoxSelection(Vector2 screenStart, Vector2 screenEnd, bool additive)
        {
            selectionBuffer.Clear();
            Rect selectionRect = ScreenRectFromPoints(screenStart, screenEnd);

            if (TryQueryUnitsInScreenBounds(selectionRect))
            {
                // Candidates already in selectionBuffer.
            }
            else
            {
                UnitManager.CopyUnitsTo(selectionBuffer);
            }

            if (!additive)
            {
                ClearSelectionVisuals();
                selectedUnits.Clear();
                ClearBuildingSelection();
            }
            else
            {
                ClearBuildingSelection();
            }

            for (int i = 0; i < selectionBuffer.Count; i++)
            {
                Unit unit = selectionBuffer[i];
                if (unit == null || !IsPlayerUnit(unit))
                    continue;

                Vector3 screenPoint = mainCamera.WorldToScreenPoint(unit.transform.position);
                if (screenPoint.z < 0f)
                    continue;

                if (!selectionRect.Contains(new Vector2(screenPoint.x, screenPoint.y)))
                    continue;

                if (additive && selectedUnits.Contains(unit))
                    continue;

                selectedUnits.Add(unit);
                unit.SetSelected(true);
            }
        }

        bool TryQueryUnitsInScreenBounds(Rect selectionRect)
        {
            if (mainCamera == null)
                return false;

            if (!TryGetGroundBoundsFromScreenRect(selectionRect, out float minX, out float maxX, out float minZ, out float maxZ))
                return false;

            UnitSpatialIndex.QueryInWorldBounds(minX, maxX, minZ, maxZ, selectionBuffer, IsPlayerUnit);
            return true;
        }

        bool TryGetGroundBoundsFromScreenRect(Rect selectionRect, out float minX, out float maxX, out float minZ, out float maxZ)
        {
            minX = float.MaxValue;
            maxX = float.MinValue;
            minZ = float.MaxValue;
            maxZ = float.MinValue;
            bool found = false;

            Vector2[] corners =
            {
                new Vector2(selectionRect.xMin, selectionRect.yMin),
                new Vector2(selectionRect.xMax, selectionRect.yMin),
                new Vector2(selectionRect.xMin, selectionRect.yMax),
                new Vector2(selectionRect.xMax, selectionRect.yMax)
            };

            for (int i = 0; i < corners.Length; i++)
            {
                Ray ray = mainCamera.ScreenPointToRay(corners[i]);
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

        void HandleMoveCommand()
        {
            if (selectedUnits.Count == 0)
                return;

            Ray ray = mainCamera.ScreenPointToRay(input.PointerScreenPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameLayers.ResourceMask))
            {
                BerryBushResource bush = hit.collider.GetComponentInParent<BerryBushResource>();
                if (bush != null && !bush.IsDepleted)
                {
                    CommandQueue.Enqueue(new GatherFoodCommand(selectedUnits, bush));
                    return;
                }

                GoldMineResource goldMine = hit.collider.GetComponentInParent<GoldMineResource>();
                if (goldMine != null && !goldMine.IsDepleted && TryIssueGatherGoldCommand(goldMine))
                    return;

                StoneMineResource stoneMine = hit.collider.GetComponentInParent<StoneMineResource>();
                if (stoneMine != null && !stoneMine.IsDepleted && TryIssueGatherStoneCommand(stoneMine))
                    return;

                TreeResource tree = hit.collider.GetComponentInParent<TreeResource>();
                if (tree != null && !tree.IsDepleted)
                {
                    CommandQueue.Enqueue(new GatherCommand(selectedUnits, tree));
                    return;
                }
            }

            if (Physics.Raycast(ray, out hit, 1000f, GameLayers.UnitMask))
            {
                Unit targetUnit = hit.collider.GetComponentInParent<Unit>();
                if (targetUnit != null && TryIssueAttackCommand(targetUnit))
                    return;
            }

            if (Physics.Raycast(ray, out hit, 1000f, GameLayers.BuildingMask))
            {
                Farm farm = hit.collider.GetComponentInParent<Farm>();
                if (farm != null && !farm.IsDepleted && TryIssueGatherFarmCommand(farm))
                    return;

                BuildingHealth targetBuilding = hit.collider.GetComponentInParent<BuildingHealth>();
                if (targetBuilding != null && TryIssueAttackBuildingCommand(targetBuilding))
                    return;
            }

            if (!Physics.Raycast(ray, out hit, 1000f, GameLayers.GroundMask))
                return;

            CommandQueue.Enqueue(new MoveCommand(selectedUnits, hit.point, groupMoveSpacing));
        }

        bool TryIssueAttackCommand(Unit targetUnit)
        {
            attackCommandBuffer.Clear();
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                Unit unit = selectedUnits[i];
                if (unit == null || !unit.CanAttack || unit.Team == targetUnit.Team)
                    continue;

                attackCommandBuffer.Add(unit);
            }

            if (attackCommandBuffer.Count == 0)
                return false;

            CommandQueue.Enqueue(new AttackUnitCommand(selectedUnits, targetUnit));
            return true;
        }

        bool TryIssueGatherFarmCommand(Farm farm)
        {
            gatherFarmBuffer.Clear();
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                Unit unit = selectedUnits[i];
                if (unit == null || unit.CanAttack || unit.Team != farm.Team)
                    continue;

                gatherFarmBuffer.Add(unit);
            }

            if (gatherFarmBuffer.Count == 0)
                return false;

            if (!FoodGatherManager.HasAssignableFarmGatherers(gatherFarmBuffer, farm))
                return false;

            CommandQueue.Enqueue(new GatherFarmFoodCommand(selectedUnits, farm));
            return true;
        }

        bool TryIssueGatherGoldCommand(GoldMineResource mine)
        {
            gatherMineralBuffer.Clear();
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                Unit unit = selectedUnits[i];
                if (unit == null || unit.CanAttack)
                    continue;

                gatherMineralBuffer.Add(unit);
            }

            if (gatherMineralBuffer.Count == 0)
                return false;

            CommandQueue.Enqueue(new GatherGoldCommand(selectedUnits, mine));
            return true;
        }

        bool TryIssueGatherStoneCommand(StoneMineResource mine)
        {
            gatherMineralBuffer.Clear();
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                Unit unit = selectedUnits[i];
                if (unit == null || unit.CanAttack)
                    continue;

                gatherMineralBuffer.Add(unit);
            }

            if (gatherMineralBuffer.Count == 0)
                return false;

            CommandQueue.Enqueue(new GatherStoneCommand(selectedUnits, mine));
            return true;
        }

        bool TryIssueAttackBuildingCommand(BuildingHealth targetBuilding)
        {
            if (targetBuilding == null || !targetBuilding.IsAlive)
                return false;

            attackCommandBuffer.Clear();
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                Unit unit = selectedUnits[i];
                if (unit == null || !unit.CanAttack || unit.Team == targetBuilding.Team)
                    continue;

                attackCommandBuffer.Add(unit);
            }

            if (attackCommandBuffer.Count == 0)
                return false;

            CommandQueue.Enqueue(new AttackBuildingCommand(selectedUnits, targetBuilding));
            return true;
        }

        void SetSelection(Unit unit)
        {
            ClearAllSelection();
            selectedUnits.Add(unit);
            unit.SetSelected(true);
        }

        void ToggleUnitSelection(Unit unit)
        {
            if (selectedUnits.Contains(unit))
            {
                selectedUnits.Remove(unit);
                unit.SetSelected(false);
                return;
            }

            ClearBuildingSelection();
            selectedUnits.Add(unit);
            unit.SetSelected(true);
        }

        void ClearSelection()
        {
            ClearSelectionVisuals();
            selectedUnits.Clear();
        }

        void SetTownCenterSelection(TownCenter townCenter)
        {
            ClearAllSelection();
            selectedTownCenter = townCenter;
            townCenter.SetSelected(true);
        }

        void SetBarracksSelection(Barracks barracks)
        {
            ClearAllSelection();
            selectedBarracks = barracks;
            barracks.SetSelected(true);
        }

        void ClearTownCenterSelection()
        {
            if (selectedTownCenter != null)
            {
                selectedTownCenter.SetSelected(false);
                selectedTownCenter = null;
            }
        }

        void ClearBarracksSelection()
        {
            if (selectedBarracks != null)
            {
                selectedBarracks.SetSelected(false);
                selectedBarracks = null;
            }
        }

        void ClearBuildingSelection()
        {
            ClearTownCenterSelection();
            ClearBarracksSelection();
        }

        void ClearAllSelection()
        {
            ClearSelection();
            ClearBuildingSelection();
        }

        void ClearSelectionVisuals()
        {
            for (int i = 0; i < selectedUnits.Count; i++)
                selectedUnits[i].SetSelected(false);
        }

        static bool IsPlayerUnit(Unit unit)
        {
            return unit != null && unit.IsAlive && unit.Team == UnitTeam.Player;
        }

        static Rect ScreenRectFromPoints(Vector2 a, Vector2 b)
        {
            float xMin = Mathf.Min(a.x, b.x);
            float xMax = Mathf.Max(a.x, b.x);
            float yMin = Mathf.Min(a.y, b.y);
            float yMax = Mathf.Max(a.y, b.y);
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }
    }
}
