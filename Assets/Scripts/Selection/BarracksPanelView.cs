using AoE.RTS.Buildings;
using AoE.RTS.Economy;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public class BarracksPanelView : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;

        const float PanelWidth = 220f;
        const float PanelHeight = 88f;
        const float Margin = 12f;

        void OnGUI()
        {
            if (selectionManager == null)
                return;

            Barracks barracks = selectionManager.SelectedBarracks;
            if (barracks == null || barracks.Data == null)
                return;

            PlacedBuildingData data = barracks.Data;
            Rect panelRect = new Rect(Margin, Screen.height - PanelHeight - Margin, PanelWidth, PanelHeight);
            GUI.Box(panelRect, GUIContent.none);

            GUILayout.BeginArea(panelRect);
            GUILayout.Label("Barracks");

            bool isProducing = BarracksProductionManager.IsProducing(barracks);
            bool populationFull = !PopulationManager.CanTrainUnit();
            bool canAfford = ResourceManager.Wood >= data.trainWoodCost;
            GUI.enabled = !isProducing && !populationFull && canAfford;
            if (GUILayout.Button($"Create Militia ({data.trainWoodCost} Wood)"))
                barracks.TryQueueMilitiaProduction();
            GUI.enabled = true;

            if (populationFull && !isProducing)
                GUILayout.Label("Population full");
            else if (!canAfford && !isProducing)
                GUILayout.Label("Need more Wood");

            if (isProducing)
            {
                float total = BarracksProductionManager.GetTotalSeconds(barracks);
                float remaining = BarracksProductionManager.GetRemainingSeconds(barracks);
                float progress = total > 0f ? 1f - remaining / total : 0f;
                GUILayout.Label($"Training... {remaining:0.0}s");
                Rect progressRect = GUILayoutUtility.GetRect(PanelWidth - 24f, 18f);
                GUI.HorizontalSlider(progressRect, progress, 0f, 1f);
            }

            GUILayout.EndArea();
        }
    }
}
