using AoE.RTS.Buildings;
using AoE.RTS.Commands;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Input;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public class BarracksPanelView : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;
        [SerializeField] RTSInputReader input;

        const float PanelWidth = 220f;
        const float PanelHeight = 104f;
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

            int queueCount = BarracksProductionManager.GetQueueCount(barracks);
            bool isProducing = queueCount > 0;
            bool queueFull = queueCount >= BarracksProductionManager.MaxQueueSize;
            bool populationFull = !PopulationManager.CanTrainUnit();
            bool canAfford = ResourceManager.Wood >= data.trainWoodCost;
            GUI.enabled = !queueFull && !populationFull && canAfford && !GameSessionManager.IsGameOver;
            if (GUILayout.Button($"Create Militia (Q) ({data.trainWoodCost} Wood)"))
                CommandQueue.Enqueue(new TrainMilitiaCommand(barracks));
            GUI.enabled = true;

            if (queueCount > 0)
                GUILayout.Label($"Queue: {queueCount}");

            if (queueFull)
                GUILayout.Label("Queue full");
            else if (populationFull)
                GUILayout.Label("Population full");
            else if (!canAfford)
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

        void Update()
        {
            if (GameSessionManager.IsGameOver || selectionManager == null || input == null)
                return;

            Barracks barracks = selectionManager.SelectedBarracks;
            if (barracks == null)
                return;

            if (input.WasTrainVillagerPressedThisFrame())
                CommandQueue.Enqueue(new TrainMilitiaCommand(barracks));
        }
    }
}
