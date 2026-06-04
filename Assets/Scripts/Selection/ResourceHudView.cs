using AoE.RTS.Economy;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public class ResourceHudView : MonoBehaviour
    {
        const float Margin = 12f;

        void OnGUI()
        {
            Rect rect = new Rect(Margin, Margin, 160f, 28f);
            GUI.Label(rect, $"Wood: {Mathf.FloorToInt(ResourceManager.Wood)}");
        }
    }
}
