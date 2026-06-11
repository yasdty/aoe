using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Economy;
using AoE.RTS.Combat;
using AoE.RTS.Core;
using AoE.RTS.Units;
using UnityEngine;
using UnityEngine.UI;

namespace AoE.RTS.Selection
{
    public class SelectionInfoPanelView : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;

        const float PanelWidth = 220f;
        const float LineHeight = 18f;
        const float Padding = 8f;

        readonly List<string> lineBuffer = new List<string>();
        readonly List<Text> lineTexts = new List<Text>();

        RectTransform panelRoot;
        Text titleText;
        bool uiBuilt;

        void Awake()
        {
            if (selectionManager == null)
                selectionManager = GetComponent<SelectionManager>();
            if (selectionManager == null)
                selectionManager = FindAnyObjectByType<SelectionManager>();
            TryBuildUi();
        }

        void OnDestroy()
        {
            if (panelRoot != null)
                GameUiInput.UnregisterHudPanel(panelRoot);
        }

        void TryBuildUi()
        {
            if (uiBuilt)
                return;

            Transform stack = HudBottomLeftStack.GetOrCreate();
            if (stack == null)
                return;

            panelRoot = HudUiFactory.CreatePanel(stack, "SelectionInfoPanel", HudUiFactory.PanelBackgroundColor);
            panelRoot.transform.SetSiblingIndex(stack.childCount - 1);
            HudUiFactory.AddVerticalLayout(panelRoot, 2f, reverseArrangement: false);
            panelRoot.gameObject.AddComponent<LayoutElement>().preferredWidth = PanelWidth;
            GameUiInput.RegisterHudPanel(panelRoot);

            titleText = HudUiFactory.CreateLabel(panelRoot, "Title", LineHeight, bold: true);
            for (int i = 0; i < 8; i++)
                lineTexts.Add(HudUiFactory.CreateLabel(panelRoot, $"Line{i}", LineHeight));

            panelRoot.gameObject.SetActive(false);
            uiBuilt = true;
        }

        void LateUpdate()
        {
            TryBuildUi();
            if (!uiBuilt || selectionManager == null)
                return;

            if (!selectionManager.ShouldShowSelectionInfoPanel || !TryBuildLines(out string title, lineBuffer))
            {
                panelRoot.gameObject.SetActive(false);
                return;
            }

            panelRoot.gameObject.SetActive(true);
            HudUiFactory.SetText(titleText, title);
            for (int i = 0; i < lineTexts.Count; i++)
            {
                if (i < lineBuffer.Count)
                {
                    lineTexts[i].gameObject.SetActive(true);
                    HudUiFactory.SetText(lineTexts[i], lineBuffer[i]);
                }
                else
                    lineTexts[i].gameObject.SetActive(false);
            }
        }

        bool TryBuildLines(out string title, List<string> lines)
        {
            title = string.Empty;
            lines.Clear();

            IReadOnlyList<Unit> units = selectionManager.SelectedUnits;
            if (units.Count == 1)
            {
                Unit unit = units[0];
                if (unit == null || !unit.IsAlive)
                    return false;

                AppendUnitInfo(unit, lines, out title);
                return true;
            }

            TownCenter townCenter = selectionManager.SelectedTownCenter;
            if (townCenter != null)
            {
                AppendBuildingHealthInfo(
                    Localization.BuildingName(PlacedBuildingKind.TownCenter),
                    townCenter.GetComponent<BuildingHealth>(),
                    lines,
                    out title);
                lines.Add(townCenter.HasRally ? Localization.Get("ui.rally_set") : Localization.Get("ui.rally_none"));
                return true;
            }

            Barracks barracks = selectionManager.SelectedBarracks;
            if (barracks != null)
            {
                AppendBuildingHealthInfo(
                    Localization.BuildingName(barracks.Data),
                    barracks.GetComponent<BuildingHealth>(),
                    lines,
                    out title);
                lines.Add(barracks.HasRally ? Localization.Get("ui.rally_set") : Localization.Get("ui.rally_none"));
                return true;
            }

            ArcheryRange archeryRange = selectionManager.SelectedArcheryRange;
            if (archeryRange != null)
            {
                AppendBuildingHealthInfo(
                    Localization.BuildingName(archeryRange.Data),
                    archeryRange.GetComponent<BuildingHealth>(),
                    lines,
                    out title);
                lines.Add(archeryRange.HasRally ? Localization.Get("ui.rally_set") : Localization.Get("ui.rally_none"));
                return true;
            }

            Stable stable = selectionManager.SelectedStable;
            if (stable != null)
            {
                AppendBuildingHealthInfo(
                    Localization.BuildingName(stable.Data),
                    stable.GetComponent<BuildingHealth>(),
                    lines,
                    out title);
                lines.Add(stable.HasRally ? Localization.Get("ui.rally_set") : Localization.Get("ui.rally_none"));
                return true;
            }

            Blacksmith blacksmith = selectionManager.SelectedBlacksmith;
            if (blacksmith != null)
            {
                AppendBuildingHealthInfo(
                    Localization.BuildingName(blacksmith.Data),
                    blacksmith.GetComponent<BuildingHealth>(),
                    lines,
                    out title);
                return true;
            }

            Market market = selectionManager.SelectedMarket;
            if (market != null)
            {
                AppendBuildingHealthInfo(
                    Localization.BuildingName(market.Data),
                    market.GetComponent<BuildingHealth>(),
                    lines,
                    out title);
                return true;
            }

            BuildingHealth placedBuilding = selectionManager.SelectedPlacedBuilding;
            if (placedBuilding != null)
            {
                AppendPlacedBuildingInfo(placedBuilding, lines, out title);
                return true;
            }

            Component resource = selectionManager.SelectedResource;
            if (resource != null)
            {
                AppendResourceInfo(resource, lines, out title);
                return true;
            }

            return false;
        }

        static void AppendUnitInfo(Unit unit, List<string> lines, out string title)
        {
            title = UnitDisplayNameUtility.GetDisplayName(unit);
            lines.Add(Localization.Format(
                "ui.hp",
                Mathf.FloorToInt(unit.CurrentHp),
                Mathf.FloorToInt(unit.MaxHp)));

            if (unit.CanAttack)
            {
                string damageTypeLabel = unit.AttackDamageType == AttackDamageType.Pierce
                    ? Localization.Get("damage.pierce")
                    : Localization.Get("damage.melee");
                lines.Add(Localization.Format("ui.attack", unit.AttackPower, damageTypeLabel));
                lines.Add(Localization.Format("ui.stance", FormatStance(unit.CombatStance)));
            }

            lines.Add(Localization.Format("ui.melee_armor", unit.MeleeArmor));
            lines.Add(Localization.Format("ui.pierce_armor", unit.PierceArmor));
        }

        static string FormatStance(UnitCombatStance stance)
        {
            switch (stance)
            {
                case UnitCombatStance.Defensive:
                    return Localization.Get("stance.defensive");
                case UnitCombatStance.StandGround:
                    return Localization.Get("stance.stand_ground");
                default:
                    return Localization.Get("stance.aggressive");
            }
        }

        static void AppendBuildingHealthInfo(string displayName, BuildingHealth health, List<string> lines, out string title)
        {
            title = displayName;
            if (health != null)
            {
                lines.Add(Localization.Format(
                    "ui.hp",
                    Mathf.FloorToInt(health.CurrentHp),
                    Mathf.FloorToInt(health.MaxHp)));
                lines.Add(Localization.Format("ui.melee_armor", health.MeleeArmor));
                lines.Add(Localization.Format("ui.pierce_armor", health.PierceArmor));
            }
        }

        static void AppendPlacedBuildingInfo(BuildingHealth health, List<string> lines, out string title)
        {
            title = ResolvePlacedBuildingName(health);
            lines.Add(Localization.Format(
                "ui.hp",
                Mathf.FloorToInt(health.CurrentHp),
                Mathf.FloorToInt(health.MaxHp)));
            lines.Add(Localization.Format("ui.melee_armor", health.MeleeArmor));
            lines.Add(Localization.Format("ui.pierce_armor", health.PierceArmor));
        }

        static string ResolvePlacedBuildingName(BuildingHealth health)
        {
            Farm farm = health.GetComponent<Farm>();
            if (farm != null && farm.Data != null)
                return Localization.BuildingName(farm.Data);

            House house = health.GetComponent<House>();
            if (house != null && house.Data != null)
                return Localization.BuildingName(house.Data);

            LumberCamp lumberCamp = health.GetComponent<LumberCamp>();
            if (lumberCamp != null && lumberCamp.Data != null)
                return Localization.BuildingName(lumberCamp.Data);

            MiningCamp miningCamp = health.GetComponent<MiningCamp>();
            if (miningCamp != null && miningCamp.Data != null)
                return Localization.BuildingName(miningCamp.Data);

            Mill mill = health.GetComponent<Mill>();
            if (mill != null && mill.Data != null)
                return Localization.BuildingName(mill.Data);

            return Localization.Get("building.generic");
        }

        static void AppendResourceInfo(Component resource, List<string> lines, out string title)
        {
            switch (resource)
            {
                case TreeResource tree:
                    title = Localization.Get("resource.node.tree");
                    lines.Add(Localization.Format("ui.resource_amount", Localization.Get("resource.wood"), Mathf.FloorToInt(tree.RemainingWood)));
                    break;
                case BerryBushResource bush:
                    title = Localization.Get("resource.node.berry_bush");
                    lines.Add(Localization.Format("ui.resource_amount", Localization.Get("resource.food"), Mathf.FloorToInt(bush.RemainingFood)));
                    break;
                case DeerResource deer:
                    title = Localization.Get("resource.node.deer");
                    lines.Add(Localization.Format("ui.resource_amount", Localization.Get("resource.food"), Mathf.FloorToInt(deer.RemainingFood)));
                    break;
                case SheepResource sheep:
                    title = Localization.Get("resource.node.sheep");
                    lines.Add(sheep.IsNeutral
                        ? Localization.Get("ui.owner_neutral")
                        : Localization.Format("ui.owner_team", sheep.OwnerTeam));
                    lines.Add(Localization.Format("ui.resource_amount", Localization.Get("resource.food"), Mathf.FloorToInt(sheep.RemainingFood)));
                    break;
                case BoarResource boar:
                    title = Localization.Get("resource.node.boar");
                    if (boar.IsDead)
                        lines.Add(Localization.Format("ui.resource_amount", Localization.Get("resource.food"), Mathf.FloorToInt(boar.RemainingFood)));
                    else
                    {
                        lines.Add(Localization.Format("ui.hp", Mathf.FloorToInt(boar.CurrentHp), Mathf.FloorToInt(boar.MaxHp)));
                        lines.Add(Localization.Format("ui.attack", boar.AttackPower, Localization.Get("damage.melee")));
                    }
                    break;
                case GoldMineResource goldMine:
                    title = Localization.Get("resource.node.gold_mine");
                    lines.Add(Localization.Format("ui.resource_amount", Localization.Get("resource.gold"), Mathf.FloorToInt(goldMine.RemainingAmount)));
                    break;
                case StoneMineResource stoneMine:
                    title = Localization.Get("resource.node.stone_mine");
                    lines.Add(Localization.Format("ui.resource_amount", Localization.Get("resource.stone"), Mathf.FloorToInt(stoneMine.RemainingAmount)));
                    break;
                default:
                    title = Localization.Get("resource.node.generic");
                    break;
            }
        }
    }
}
