using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public class ResourceHudView : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;
        [SerializeField] PlacedBuildingData houseData;
        [SerializeField] PlacedBuildingData barracksData;

        const float Margin = 12f;
        const float PanelWidth = 210f;
        const float WoodLineHeight = 24f;
        const float FoodLineHeight = 24f;
        const float PopLineHeight = 24f;
        const float ButtonHeight = 28f;
        const float ButtonGap = 4f;
        const float Padding = 8f;

        static ResourceHudView instance;

        void Awake()
        {
            instance = this;
            houseData = PlacedBuildingDataResolver.ResolveHouse(ref houseData);
            barracksData = PlacedBuildingDataResolver.ResolveBarracks(ref barracksData);
            if (selectionManager == null)
                selectionManager = FindAnyObjectByType<SelectionManager>();
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public static bool IsPointerOverHud(Vector2 screenPosition)
        {
            return GameUiInput.IsPointerOverHud(screenPosition);
        }

        void OnGUI()
        {
            PlacedBuildingData house = PlacedBuildingDataResolver.ResolveHouse(ref houseData);
            PlacedBuildingData barracks = PlacedBuildingDataResolver.ResolveBarracks(ref barracksData);
            float panelHeight = Padding * 2f + WoodLineHeight + ButtonGap + FoodLineHeight + ButtonGap + PopLineHeight + ButtonGap
                + ButtonHeight + ButtonGap + ButtonHeight;
            Rect panelRect = new Rect(Margin, Margin, PanelWidth, panelHeight);
            GameUiInput.SetHudPanelScreenRect(GameUiInput.GuiRectToScreenRect(panelRect));

            GUI.Box(panelRect, GUIContent.none);

            float y = Margin + Padding;

            Rect woodRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, WoodLineHeight);
            GUI.Label(woodRect, $"Wood: {Mathf.FloorToInt(ResourceManager.Wood)}");
            y += WoodLineHeight + ButtonGap;

            Rect foodRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, FoodLineHeight);
            GUI.Label(foodRect, $"Food: {Mathf.FloorToInt(ResourceManager.Food)}");
            y += FoodLineHeight + ButtonGap;

            Rect popRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, PopLineHeight);
            GUI.Label(popRect, $"Pop: {PopulationManager.CurrentPopulation}/{PopulationManager.MaxPopulation}");
            y += PopLineHeight + ButtonGap;

            bool inPlacementMode = BuildingPlacementManager.IsPlacementModeActive;
            bool gameOver = GameSessionManager.IsGameOver;

            Rect houseButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            bool canAffordHouse = ResourceManager.Wood >= house.woodCost;
            GUI.enabled = canAffordHouse && !inPlacementMode && !gameOver;
            if (GUI.Button(houseButtonRect, $"Build House ({house.woodCost} Wood)"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterHousePlacementMode(builders);
            }
            y += ButtonHeight + ButtonGap;

            Rect barracksButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            bool canAffordBarracks = ResourceManager.Wood >= barracks.woodCost;
            GUI.enabled = canAffordBarracks && !inPlacementMode && !gameOver;
            if (GUI.Button(barracksButtonRect, $"Build Barracks ({barracks.woodCost} Wood)"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterBarracksPlacementMode(builders);
            }
            GUI.enabled = true;

            if (inPlacementMode)
            {
                Rect hintRect = new Rect(Margin, panelRect.yMax + 4f, PanelWidth + 60f, 36f);
                GameUiInput.SetHudHintScreenRect(GameUiInput.GuiRectToScreenRect(hintRect));
                GUI.Label(hintRect, "Click ground to place. Esc / Right-click to cancel.");
            }
            else
            {
                GameUiInput.ClearHudHintScreenRect();
                if (!canAffordHouse && !canAffordBarracks)
                {
                    Rect hintRect = new Rect(Margin + Padding, panelRect.yMax + 4f, PanelWidth, 20f);
                    GUI.Label(hintRect, "Need more Wood.");
                }
            }
        }
    }
}
