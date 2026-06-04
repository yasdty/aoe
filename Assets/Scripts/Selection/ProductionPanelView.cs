using AoE.RTS.Buildings;
using AoE.RTS.Economy;
using AoE.RTS.Input;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public class ProductionPanelView : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;
        [SerializeField] RTSInputReader input;

        const float PanelWidth = 220f;
        const float PanelHeight = 72f;
        const float Margin = 12f;

        void OnGUI()
        {
            if (selectionManager == null)
                return;

            TownCenter townCenter = selectionManager.SelectedTownCenter;
            if (townCenter == null)
                return;

            Rect panelRect = new Rect(Margin, Screen.height - PanelHeight - Margin, PanelWidth, PanelHeight);
            GUI.Box(panelRect, GUIContent.none);

            GUILayout.BeginArea(panelRect);
            GUILayout.Label("Town Center");

            bool isProducing = ProductionManager.IsProducing(townCenter);
            bool populationFull = !PopulationManager.CanTrainUnit();
            GUI.enabled = !isProducing && !populationFull;
            if (GUILayout.Button("Create Villager (Q)"))
                townCenter.TryQueueVillagerProduction();
            GUI.enabled = true;

            if (populationFull && !isProducing)
                GUILayout.Label("Population full");

            if (isProducing)
            {
                float total = ProductionManager.GetTotalSeconds(townCenter);
                float remaining = ProductionManager.GetRemainingSeconds(townCenter);
                float progress = total > 0f ? 1f - remaining / total : 0f;
                GUILayout.Label($"Training... {remaining:0.0}s");
                Rect progressRect = GUILayoutUtility.GetRect(PanelWidth - 24f, 18f);
                GUI.HorizontalSlider(progressRect, progress, 0f, 1f);
            }

            GUILayout.EndArea();
        }

        void Update()
        {
            if (selectionManager == null || input == null)
                return;

            TownCenter townCenter = selectionManager.SelectedTownCenter;
            if (townCenter == null)
                return;

            if (input.WasTrainVillagerPressedThisFrame())
                townCenter.TryQueueVillagerProduction();
        }
    }
}
