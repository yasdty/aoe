using AoE.RTS.Economy;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public class CpuHudView : MonoBehaviour
    {
        const float Margin = 12f;
        const float PanelWidth = 180f;
        const float LineHeight = 22f;
        const float Padding = 8f;

        void OnGUI()
        {
            float panelHeight = Padding * 2f + LineHeight * 5f;
            float x = Screen.width - PanelWidth - Margin;
            Rect panelRect = new Rect(x, Margin, PanelWidth, panelHeight);
            GUI.Box(panelRect, GUIContent.none);

            float y = Margin + Padding;
            Rect woodRect = new Rect(x + Padding, y, PanelWidth - Padding * 2f, LineHeight);
            GUI.Label(woodRect, $"CPU Wood: {Mathf.FloorToInt(ResourceManager.GetWood(UnitTeam.Enemy))}");
            y += LineHeight;

            Rect foodRect = new Rect(x + Padding, y, PanelWidth - Padding * 2f, LineHeight);
            GUI.Label(foodRect, $"CPU Food: {Mathf.FloorToInt(ResourceManager.GetFood(UnitTeam.Enemy))}");
            y += LineHeight;

            Rect goldRect = new Rect(x + Padding, y, PanelWidth - Padding * 2f, LineHeight);
            GUI.Label(goldRect, $"CPU Gold: {Mathf.FloorToInt(ResourceManager.GetGold(UnitTeam.Enemy))}");
            y += LineHeight;

            Rect stoneRect = new Rect(x + Padding, y, PanelWidth - Padding * 2f, LineHeight);
            GUI.Label(stoneRect, $"CPU Stone: {Mathf.FloorToInt(ResourceManager.GetStone(UnitTeam.Enemy))}");
            y += LineHeight;

            Rect popRect = new Rect(x + Padding, y, PanelWidth - Padding * 2f, LineHeight);
            GUI.Label(
                popRect,
                $"CPU Pop: {PopulationManager.GetCurrentPopulation(UnitTeam.Enemy)}/{PopulationManager.GetMaxPopulation(UnitTeam.Enemy)}");
        }
    }
}
