using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Economy;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public class ResourceHudView : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;
        [SerializeField] PlacedBuildingData houseData;

        const float Margin = 12f;
        const float PanelWidth = 210f;
        const float WoodLineHeight = 24f;
        const float PopLineHeight = 24f;
        const float ButtonHeight = 28f;
        const float Padding = 8f;

        static ResourceHudView instance;

        void Awake()
        {
            instance = this;
            houseData = PlacedBuildingDataResolver.Resolve(ref houseData);
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
            PlacedBuildingData data = PlacedBuildingDataResolver.Resolve(ref houseData);
            float panelHeight = Padding * 2f + WoodLineHeight + 4f + PopLineHeight + 4f + ButtonHeight;
            Rect panelRect = new Rect(Margin, Margin, PanelWidth, panelHeight);
            GameUiInput.SetHudPanelScreenRect(GameUiInput.GuiRectToScreenRect(panelRect));

            GUI.Box(panelRect, GUIContent.none);

            Rect woodRect = new Rect(Margin + Padding, Margin + Padding, PanelWidth - Padding * 2f, WoodLineHeight);
            GUI.Label(woodRect, $"Wood: {Mathf.FloorToInt(ResourceManager.Wood)}");

            Rect popRect = new Rect(
                Margin + Padding,
                Margin + Padding + WoodLineHeight + 4f,
                PanelWidth - Padding * 2f,
                PopLineHeight);
            GUI.Label(popRect, $"Pop: {PopulationManager.CurrentPopulation}/{PopulationManager.MaxPopulation}");

            Rect buttonRect = new Rect(
                Margin + Padding,
                Margin + Padding + WoodLineHeight + 4f + PopLineHeight + 4f,
                PanelWidth - Padding * 2f,
                ButtonHeight);

            bool canAfford = ResourceManager.Wood >= data.woodCost;
            bool inPlacementMode = BuildingPlacementManager.IsPlacementModeActive;

            GUI.enabled = canAfford && !inPlacementMode;
            if (GUI.Button(buttonRect, $"Build House ({data.woodCost} Wood)"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterHousePlacementMode(builders);
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
                if (!canAfford)
                {
                    Rect hintRect = new Rect(Margin + Padding, panelRect.yMax + 4f, PanelWidth, 20f);
                    GUI.Label(hintRect, "Need more Wood.");
                }
            }
        }
    }
}
