using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Economy;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public class SelectionInfoPanelView : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;

        const float PanelWidth = 220f;
        const float LineHeight = 18f;
        const float Margin = 12f;
        const float Padding = 8f;
        const float ProductionPanelReserveHeight = 96f;

        readonly List<string> lineBuffer = new List<string>();

        void Awake()
        {
            if (selectionManager == null)
                selectionManager = GetComponent<SelectionManager>();
            if (selectionManager == null)
                selectionManager = FindAnyObjectByType<SelectionManager>();
        }

        void OnGUI()
        {
            if (selectionManager == null || !selectionManager.ShouldShowSelectionInfoPanel)
                return;

            if (!TryBuildLines(out string title, lineBuffer))
                return;

            float panelHeight = Padding * 2f + LineHeight + lineBuffer.Count * LineHeight;
            float panelX = Margin;
            float bottomOffset = Margin;
            if (selectionManager.SelectedTownCenter != null || selectionManager.SelectedBarracks != null
                || selectionManager.SelectedArcheryRange != null)
                bottomOffset += ProductionPanelReserveHeight;

            float panelY = Screen.height - panelHeight - bottomOffset;
            Rect panelRect = new Rect(panelX, panelY, PanelWidth, panelHeight);

            GUI.Box(panelRect, GUIContent.none);

            float y = panelY + Padding;
            var titleStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            GUI.Label(new Rect(panelX + Padding, y, PanelWidth - Padding * 2f, LineHeight), title, titleStyle);
            y += LineHeight;

            for (int i = 0; i < lineBuffer.Count; i++)
            {
                GUI.Label(new Rect(panelX + Padding, y, PanelWidth - Padding * 2f, LineHeight), lineBuffer[i]);
                y += LineHeight;
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
                    townCenter.Data != null ? townCenter.Data.displayName : "Town Center",
                    townCenter.GetComponent<BuildingHealth>(),
                    lines,
                    out title);
                lines.Add(townCenter.HasRally ? "Rally: Set" : "Rally: None");
                return true;
            }

            Barracks barracks = selectionManager.SelectedBarracks;
            if (barracks != null)
            {
                AppendBuildingHealthInfo(
                    barracks.Data != null ? barracks.Data.displayName : "Barracks",
                    barracks.GetComponent<BuildingHealth>(),
                    lines,
                    out title);
                lines.Add(barracks.HasRally ? "Rally: Set" : "Rally: None");
                return true;
            }

            ArcheryRange archeryRange = selectionManager.SelectedArcheryRange;
            if (archeryRange != null)
            {
                AppendBuildingHealthInfo(
                    archeryRange.Data != null ? archeryRange.Data.displayName : "Archery Range",
                    archeryRange.GetComponent<BuildingHealth>(),
                    lines,
                    out title);
                lines.Add(archeryRange.HasRally ? "Rally: Set" : "Rally: None");
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
            title = unit.Data != null ? unit.Data.displayName : "Unit";
            lines.Add($"HP: {Mathf.FloorToInt(unit.CurrentHp)} / {Mathf.FloorToInt(unit.MaxHp)}");

            if (unit.CanAttack)
                lines.Add($"Attack: {unit.AttackPower:0}");

            if (unit.Armor > 0f)
                lines.Add($"Armor: {unit.Armor:0}");
        }

        static void AppendBuildingHealthInfo(string displayName, BuildingHealth health, List<string> lines, out string title)
        {
            title = displayName;
            if (health != null)
            {
                lines.Add($"HP: {Mathf.FloorToInt(health.CurrentHp)} / {Mathf.FloorToInt(health.MaxHp)}");
                if (health.Armor > 0f)
                    lines.Add($"Armor: {health.Armor:0}");
            }
        }

        static void AppendPlacedBuildingInfo(BuildingHealth health, List<string> lines, out string title)
        {
            title = ResolvePlacedBuildingName(health);
            lines.Add($"HP: {Mathf.FloorToInt(health.CurrentHp)} / {Mathf.FloorToInt(health.MaxHp)}");
            if (health.Armor > 0f)
                lines.Add($"Armor: {health.Armor:0}");
        }

        static string ResolvePlacedBuildingName(BuildingHealth health)
        {
            Farm farm = health.GetComponent<Farm>();
            if (farm != null && farm.Data != null)
                return farm.Data.displayName;

            House house = health.GetComponent<House>();
            if (house != null && house.Data != null)
                return house.Data.displayName;

            LumberCamp lumberCamp = health.GetComponent<LumberCamp>();
            if (lumberCamp != null && lumberCamp.Data != null)
                return lumberCamp.Data.displayName;

            MiningCamp miningCamp = health.GetComponent<MiningCamp>();
            if (miningCamp != null && miningCamp.Data != null)
                return miningCamp.Data.displayName;

            Mill mill = health.GetComponent<Mill>();
            if (mill != null && mill.Data != null)
                return mill.Data.displayName;

            return "Building";
        }

        static void AppendResourceInfo(Component resource, List<string> lines, out string title)
        {
            switch (resource)
            {
                case TreeResource tree:
                    title = "Tree";
                    lines.Add($"Wood: {Mathf.FloorToInt(tree.RemainingWood)}");
                    break;
                case BerryBushResource bush:
                    title = "Berry Bush";
                    lines.Add($"Food: {Mathf.FloorToInt(bush.RemainingFood)}");
                    break;
                case DeerResource deer:
                    title = "Deer";
                    lines.Add($"Food: {Mathf.FloorToInt(deer.RemainingFood)}");
                    break;
                case SheepResource sheep:
                    title = "Sheep";
                    lines.Add(sheep.IsNeutral ? "Owner: Neutral" : $"Owner: {sheep.OwnerTeam}");
                    lines.Add($"Food: {Mathf.FloorToInt(sheep.RemainingFood)}");
                    break;
                case BoarResource boar:
                    title = "Boar";
                    if (boar.IsDead)
                        lines.Add($"Food: {Mathf.FloorToInt(boar.RemainingFood)}");
                    else
                    {
                        lines.Add($"HP: {Mathf.FloorToInt(boar.CurrentHp)}/{Mathf.FloorToInt(boar.MaxHp)}");
                        lines.Add($"Attack: {boar.AttackPower:0}");
                    }
                    break;
                case GoldMineResource goldMine:
                    title = "Gold Mine";
                    lines.Add($"Gold: {Mathf.FloorToInt(goldMine.RemainingAmount)}");
                    break;
                case StoneMineResource stoneMine:
                    title = "Stone Mine";
                    lines.Add($"Stone: {Mathf.FloorToInt(stoneMine.RemainingAmount)}");
                    break;
                default:
                    title = "Resource";
                    break;
            }
        }
    }
}
