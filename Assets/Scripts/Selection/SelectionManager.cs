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
        ArcheryRange selectedArcheryRange;
        Stable selectedStable;
        Blacksmith selectedBlacksmith;
        BuildingHealth selectedPlacedBuilding;
        Component selectedResource;

        Vector2 dragStartScreen;
        bool isDragging;

        public IReadOnlyList<Unit> SelectedUnits => selectedUnits;
        public TownCenter SelectedTownCenter => selectedTownCenter;
        public Barracks SelectedBarracks => selectedBarracks;
        public ArcheryRange SelectedArcheryRange => selectedArcheryRange;
        public Stable SelectedStable => selectedStable;
        public Blacksmith SelectedBlacksmith => selectedBlacksmith;
        public BuildingHealth SelectedPlacedBuilding => selectedPlacedBuilding;
        public Component SelectedResource => selectedResource;

        public bool ShouldShowSelectionInfoPanel
        {
            get
            {
                if (selectedUnits.Count > 1)
                    return false;

                if (selectedUnits.Count == 1)
                    return true;

                return selectedTownCenter != null
                    || selectedBarracks != null
                    || selectedArcheryRange != null
                    || selectedStable != null
                    || selectedBlacksmith != null
                    || selectedPlacedBuilding != null
                    || selectedResource != null;
            }
        }

        void Awake()
        {
            instance = this;
            EnsureSelectionInfoPanelView();
        }

        void EnsureSelectionInfoPanelView()
        {
            if (GetComponent<SelectionInfoPanelView>() != null)
                return;

            gameObject.AddComponent<SelectionInfoPanelView>();
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
            ControlGroupManager.HandleUnitDied(unit);
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
                    ClearInfoSelection();
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
                if (TrySelectPlayerBuilding(hit, additive))
                    return;
            }

            if (Physics.Raycast(ray, out hit, 1000f, GameLayers.ResourceMask))
            {
                if (!additive && TrySelectResource(hit))
                    return;
            }

            if (!additive)
                ClearAllSelection();
        }

        bool TrySelectPlayerBuilding(RaycastHit hit, bool additive)
        {
            if (additive)
                return false;

            TownCenter townCenter = hit.collider.GetComponentInParent<TownCenter>();
            if (townCenter != null && townCenter.Team == UnitTeam.Player)
            {
                SetTownCenterSelection(townCenter);
                return true;
            }

            Barracks barracks = hit.collider.GetComponentInParent<Barracks>();
            if (barracks != null && barracks.Team == UnitTeam.Player)
            {
                SetBarracksSelection(barracks);
                return true;
            }

            ArcheryRange archeryRange = hit.collider.GetComponentInParent<ArcheryRange>();
            if (archeryRange != null && archeryRange.Team == UnitTeam.Player)
            {
                SetArcheryRangeSelection(archeryRange);
                return true;
            }

            Stable stable = hit.collider.GetComponentInParent<Stable>();
            if (stable != null && stable.Team == UnitTeam.Player)
            {
                SetStableSelection(stable);
                return true;
            }

            Blacksmith blacksmith = hit.collider.GetComponentInParent<Blacksmith>();
            if (blacksmith != null && blacksmith.Team == UnitTeam.Player)
            {
                SetBlacksmithSelection(blacksmith);
                return true;
            }

            PalisadeWall palisadeWall = hit.collider.GetComponentInParent<PalisadeWall>();
            if (palisadeWall != null && palisadeWall.Team == UnitTeam.Player)
            {
                SetPlacedBuildingSelection(palisadeWall.GetComponent<BuildingHealth>());
                return true;
            }

            StoneWall stoneWall = hit.collider.GetComponentInParent<StoneWall>();
            if (stoneWall != null && stoneWall.Team == UnitTeam.Player)
            {
                SetPlacedBuildingSelection(stoneWall.GetComponent<BuildingHealth>());
                return true;
            }

            WatchTower watchTower = hit.collider.GetComponentInParent<WatchTower>();
            if (watchTower != null && watchTower.Team == UnitTeam.Player)
            {
                SetPlacedBuildingSelection(watchTower.GetComponent<BuildingHealth>());
                return true;
            }

            Farm farm = hit.collider.GetComponentInParent<Farm>();
            if (farm != null && farm.Team == UnitTeam.Player)
            {
                SetPlacedBuildingSelection(farm.GetComponent<BuildingHealth>());
                return true;
            }

            House house = hit.collider.GetComponentInParent<House>();
            if (house != null)
            {
                BuildingHealth houseHealth = house.GetComponent<BuildingHealth>();
                if (houseHealth != null && houseHealth.Team == UnitTeam.Player)
                {
                    SetPlacedBuildingSelection(houseHealth);
                    return true;
                }
            }

            LumberCamp lumberCamp = hit.collider.GetComponentInParent<LumberCamp>();
            if (lumberCamp != null && lumberCamp.Team == UnitTeam.Player)
            {
                SetPlacedBuildingSelection(lumberCamp.GetComponent<BuildingHealth>());
                return true;
            }

            MiningCamp miningCamp = hit.collider.GetComponentInParent<MiningCamp>();
            if (miningCamp != null && miningCamp.Team == UnitTeam.Player)
            {
                SetPlacedBuildingSelection(miningCamp.GetComponent<BuildingHealth>());
                return true;
            }

            Mill mill = hit.collider.GetComponentInParent<Mill>();
            if (mill != null && mill.Team == UnitTeam.Player)
            {
                SetPlacedBuildingSelection(mill.GetComponent<BuildingHealth>());
                return true;
            }

            return false;
        }

        bool TrySelectResource(RaycastHit hit)
        {
            TreeResource tree = hit.collider.GetComponentInParent<TreeResource>();
            if (tree != null)
            {
                SetResourceSelection(tree);
                return true;
            }

            BerryBushResource bush = hit.collider.GetComponentInParent<BerryBushResource>();
            if (bush != null)
            {
                SetResourceSelection(bush);
                return true;
            }

            DeerResource deer = hit.collider.GetComponentInParent<DeerResource>();
            if (deer != null)
            {
                SetResourceSelection(deer);
                return true;
            }

            SheepResource sheep = hit.collider.GetComponentInParent<SheepResource>();
            if (sheep != null)
            {
                SetResourceSelection(sheep);
                return true;
            }

            BoarResource boar = hit.collider.GetComponentInParent<BoarResource>();
            if (boar != null)
            {
                SetResourceSelection(boar);
                return true;
            }

            GoldMineResource goldMine = hit.collider.GetComponentInParent<GoldMineResource>();
            if (goldMine != null)
            {
                SetResourceSelection(goldMine);
                return true;
            }

            StoneMineResource stoneMine = hit.collider.GetComponentInParent<StoneMineResource>();
            if (stoneMine != null)
            {
                SetResourceSelection(stoneMine);
                return true;
            }

            return false;
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
                ClearInfoSelection();
            }
            else
            {
                ClearBuildingSelection();
                ClearInfoSelection();
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
            {
                if (TrySetProductionRallyFromClick())
                    return;

                TryIssueSheepMoveCommand();
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(input.PointerScreenPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameLayers.ResourceMask))
            {
                DeerResource deer = hit.collider.GetComponentInParent<DeerResource>();
                if (deer != null && !deer.IsDepleted)
                {
                    CommandQueue.Enqueue(new HuntFoodCommand(selectedUnits, deer));
                    return;
                }

                SheepResource sheep = hit.collider.GetComponentInParent<SheepResource>();
                if (sheep != null && !sheep.IsDepleted && CanHuntSheepWithSelection(sheep))
                {
                    CommandQueue.Enqueue(new HuntFoodCommand(selectedUnits, sheep));
                    return;
                }

                BoarResource boar = hit.collider.GetComponentInParent<BoarResource>();
                if (boar != null && !boar.IsDepleted)
                {
                    if (!boar.IsDead)
                        TryIssueAttackBoarCommand(boar);
                    if (HasNonCombatSelectedUnits())
                        CommandQueue.Enqueue(new HuntFoodCommand(selectedUnits, boar));
                    return;
                }

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

            if (input.WasAttackMoveModifierHeld())
                CommandQueue.Enqueue(new AttackMoveCommand(selectedUnits, hit.point, groupMoveSpacing));
            else
                CommandQueue.Enqueue(new MoveCommand(selectedUnits, hit.point, groupMoveSpacing));
        }

        bool TrySetProductionRallyFromClick()
        {
            if (mainCamera == null || input == null)
                return false;

            Ray ray = mainCamera.ScreenPointToRay(input.PointerScreenPosition);

            if (selectedTownCenter != null && selectedTownCenter.Team == UnitTeam.Player)
            {
                if (TryBuildTownCenterRallyFromRay(ray, out ProductionRallyPoint rally))
                {
                    CommandQueue.Enqueue(new SetRallyPointCommand(selectedTownCenter, rally));
                    return true;
                }

                return false;
            }

            if (selectedBarracks != null && selectedBarracks.Team == UnitTeam.Player)
            {
                if (TryBuildBarracksRallyFromRay(ray, out ProductionRallyPoint rally))
                {
                    CommandQueue.Enqueue(new SetRallyPointCommand(selectedBarracks, rally));
                    return true;
                }

                return false;
            }

            if (selectedArcheryRange != null && selectedArcheryRange.Team == UnitTeam.Player)
            {
                if (TryBuildBarracksRallyFromRay(ray, out ProductionRallyPoint rally))
                {
                    CommandQueue.Enqueue(new SetRallyPointCommand(selectedArcheryRange, rally));
                    return true;
                }

                return false;
            }

            if (selectedStable != null && selectedStable.Team == UnitTeam.Player)
            {
                if (TryBuildBarracksRallyFromRay(ray, out ProductionRallyPoint rally))
                {
                    CommandQueue.Enqueue(new SetRallyPointCommand(selectedStable, rally));
                    return true;
                }

                return false;
            }

            return false;
        }

        bool TryBuildTownCenterRallyFromRay(Ray ray, out ProductionRallyPoint rally)
        {
            rally = ProductionRallyPoint.None;

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameLayers.ResourceMask))
            {
                TreeResource tree = hit.collider.GetComponentInParent<TreeResource>();
                if (tree != null && !tree.IsDepleted)
                {
                    rally = ProductionRallyPoint.FromTree(tree);
                    return true;
                }

                BerryBushResource bush = hit.collider.GetComponentInParent<BerryBushResource>();
                if (bush != null && !bush.IsDepleted)
                {
                    rally = ProductionRallyPoint.FromBerryBush(bush);
                    return true;
                }

                GoldMineResource goldMine = hit.collider.GetComponentInParent<GoldMineResource>();
                if (goldMine != null && !goldMine.IsDepleted)
                {
                    rally = ProductionRallyPoint.FromGoldMine(goldMine);
                    return true;
                }

                StoneMineResource stoneMine = hit.collider.GetComponentInParent<StoneMineResource>();
                if (stoneMine != null && !stoneMine.IsDepleted)
                {
                    rally = ProductionRallyPoint.FromStoneMine(stoneMine);
                    return true;
                }
            }

            if (Physics.Raycast(ray, out hit, 1000f, GameLayers.BuildingMask))
            {
                Farm farm = hit.collider.GetComponentInParent<Farm>();
                if (farm != null && !farm.IsDepleted && farm.Team == UnitTeam.Player)
                {
                    rally = ProductionRallyPoint.FromFarm(farm);
                    return true;
                }
            }

            if (Physics.Raycast(ray, out hit, 1000f, GameLayers.GroundMask))
            {
                rally = ProductionRallyPoint.FromGround(hit.point);
                return true;
            }

            return false;
        }

        bool TryBuildBarracksRallyFromRay(Ray ray, out ProductionRallyPoint rally)
        {
            rally = ProductionRallyPoint.None;

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameLayers.GroundMask))
            {
                rally = ProductionRallyPoint.FromGround(hit.point);
                return true;
            }

            if (Physics.Raycast(ray, out hit, 1000f, GameLayers.ResourceMask | GameLayers.BuildingMask))
            {
                rally = ProductionRallyPoint.FromGround(hit.point);
                return true;
            }

            return false;
        }

        bool TryIssueSheepMoveCommand()
        {
            if (selectedResource is not SheepResource sheep || sheep.IsDepleted || sheep.IsNeutral)
                return false;

            if (sheep.OwnerTeam != UnitTeam.Player)
                return false;

            Ray ray = mainCamera.ScreenPointToRay(input.PointerScreenPosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, GameLayers.GroundMask))
                return false;

            CommandQueue.Enqueue(new SheepMoveCommand(sheep, hit.point));
            return true;
        }

        bool CanHuntSheepWithSelection(SheepResource sheep)
        {
            if (sheep == null || sheep.IsNeutral)
                return false;

            for (int i = 0; i < selectedUnits.Count; i++)
            {
                Unit unit = selectedUnits[i];
                if (unit == null || unit.CanAttack)
                    continue;

                if (sheep.CanBeHuntedBy(unit.Team))
                    return true;
            }

            return false;
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

        bool TryIssueAttackBoarCommand(BoarResource boar)
        {
            if (boar == null || boar.IsDead)
                return false;

            attackCommandBuffer.Clear();
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                Unit unit = selectedUnits[i];
                if (unit == null || !unit.CanAttack)
                    continue;

                attackCommandBuffer.Add(unit);
            }

            if (attackCommandBuffer.Count == 0)
                return false;

            CommandQueue.Enqueue(new AttackBoarCommand(selectedUnits, boar));
            return true;
        }

        bool HasNonCombatSelectedUnits()
        {
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                Unit unit = selectedUnits[i];
                if (unit != null && !unit.CanAttack)
                    return true;
            }

            return false;
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

        public void SelectSingleUnit(Unit unit)
        {
            if (!IsPlayerUnit(unit))
                return;

            SetSelection(unit);
        }

        public void SelectUnits(IReadOnlyList<Unit> units)
        {
            ClearAllSelection();
            if (units == null)
                return;

            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                if (!IsPlayerUnit(unit))
                    continue;

                selectedUnits.Add(unit);
                unit.SetSelected(true);
            }
        }

        public void SelectUnitsAdditive(IReadOnlyList<Unit> units)
        {
            if (units == null || units.Count == 0)
                return;

            ClearBuildingSelection();
            ClearInfoSelection();

            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                if (!IsPlayerUnit(unit) || selectedUnits.Contains(unit))
                    continue;

                selectedUnits.Add(unit);
                unit.SetSelected(true);
            }
        }

        void SetPlacedBuildingSelection(BuildingHealth building)
        {
            ClearAllSelection();
            selectedPlacedBuilding = building;
        }

        void SetResourceSelection(Component resource)
        {
            ClearAllSelection();
            selectedResource = resource;
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
            ClearInfoSelection();
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

        void SetArcheryRangeSelection(ArcheryRange archeryRange)
        {
            ClearAllSelection();
            selectedArcheryRange = archeryRange;
            archeryRange.SetSelected(true);
        }

        void SetStableSelection(Stable stable)
        {
            ClearAllSelection();
            selectedStable = stable;
            stable.SetSelected(true);
        }

        void SetBlacksmithSelection(Blacksmith blacksmith)
        {
            ClearAllSelection();
            selectedBlacksmith = blacksmith;
            blacksmith.SetSelected(true);
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

        void ClearArcheryRangeSelection()
        {
            if (selectedArcheryRange != null)
            {
                selectedArcheryRange.SetSelected(false);
                selectedArcheryRange = null;
            }
        }

        void ClearStableSelection()
        {
            if (selectedStable != null)
            {
                selectedStable.SetSelected(false);
                selectedStable = null;
            }
        }

        void ClearBlacksmithSelection()
        {
            if (selectedBlacksmith != null)
            {
                selectedBlacksmith.SetSelected(false);
                selectedBlacksmith = null;
            }
        }

        void ClearBuildingSelection()
        {
            ClearTownCenterSelection();
            ClearBarracksSelection();
            ClearArcheryRangeSelection();
            ClearStableSelection();
            ClearBlacksmithSelection();
            selectedPlacedBuilding = null;
        }

        void ClearInfoSelection()
        {
            selectedResource = null;
        }

        void ClearAllSelection()
        {
            ClearSelection();
            ClearBuildingSelection();
            ClearInfoSelection();
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
