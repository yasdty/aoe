using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Core;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public static class ProductionQueuePanelUi
    {
        public const int ShiftBatchQueueCount = 5;

        public static void DrawCancelableQueue(
            IReadOnlyList<ProductionQueueEntry> entries,
            System.Action<int> onCancel)
        {
            if (entries == null || entries.Count == 0 || onCancel == null)
                return;

            for (int i = 0; i < entries.Count; i++)
            {
                ProductionQueueEntry entry = entries[i];
                int queueIndex = entry.queueIndex;
                if (GUILayout.Button(Localization.Format(
                    "ui.cancel_queue",
                    queueIndex + 1,
                    Localization.LocalizeDisplayName(entry.displayName))))
                    onCancel(queueIndex);
            }
        }
    }
}
