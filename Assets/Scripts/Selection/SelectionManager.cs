using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Input;
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

        readonly List<Unit> selectedUnits = new List<Unit>();
        readonly List<Unit> selectionBuffer = new List<Unit>();

        TownCenter selectedTownCenter;

        Vector2 dragStartScreen;
        bool isDragging;

        public IReadOnlyList<Unit> SelectedUnits => selectedUnits;
        public TownCenter SelectedTownCenter => selectedTownCenter;

        void Update()
        {
            if (input == null || mainCamera == null)
                return;

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

            if (input.WasCommandPressedThisFrame())
                HandleMoveCommand();
        }

        void HandleClickSelect(bool additive)
        {
            Ray ray = mainCamera.ScreenPointToRay(input.PointerScreenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameLayers.UnitMask))
            {
                Unit unit = hit.collider.GetComponentInParent<Unit>();
                if (unit != null)
                {
                    ClearTownCenterSelection();
                    if (additive)
                        ToggleUnitSelection(unit);
                    else
                        SetSelection(unit);
                    return;
                }
            }

            if (Physics.Raycast(ray, out hit, 1000f, GameLayers.BuildingMask))
            {
                TownCenter townCenter = hit.collider.GetComponentInParent<TownCenter>();
                if (townCenter != null)
                {
                    if (!additive)
                        SetTownCenterSelection(townCenter);
                    return;
                }
            }

            if (!additive)
                ClearAllSelection();
        }

        void ApplyBoxSelection(Vector2 screenStart, Vector2 screenEnd, bool additive)
        {
            selectionBuffer.Clear();
            UnitManager.CopyUnitsTo(selectionBuffer);

            Rect selectionRect = ScreenRectFromPoints(screenStart, screenEnd);

            if (!additive)
            {
                ClearSelectionVisuals();
                selectedUnits.Clear();
                ClearTownCenterSelection();
            }
            else
            {
                ClearTownCenterSelection();
            }

            for (int i = 0; i < selectionBuffer.Count; i++)
            {
                Unit unit = selectionBuffer[i];
                if (unit == null)
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

        void HandleMoveCommand()
        {
            if (selectedUnits.Count == 0)
                return;

            Ray ray = mainCamera.ScreenPointToRay(input.PointerScreenPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameLayers.ResourceMask))
            {
                TreeResource tree = hit.collider.GetComponentInParent<TreeResource>();
                if (tree != null && !tree.IsDepleted)
                {
                    GatherManager.IssueGatherCommand(selectedUnits, tree);
                    return;
                }
            }

            if (!Physics.Raycast(ray, out hit, 1000f, GameLayers.GroundMask))
                return;

            GatherManager.CancelForUnits(selectedUnits);
            GroupMoveFormation.AssignMoveTargets(selectedUnits, hit.point, groupMoveSpacing);
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

            ClearTownCenterSelection();
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

        void ClearTownCenterSelection()
        {
            if (selectedTownCenter != null)
            {
                selectedTownCenter.SetSelected(false);
                selectedTownCenter = null;
            }
        }

        void ClearAllSelection()
        {
            ClearSelection();
            ClearTownCenterSelection();
        }

        void ClearSelectionVisuals()
        {
            for (int i = 0; i < selectedUnits.Count; i++)
                selectedUnits[i].SetSelected(false);
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
