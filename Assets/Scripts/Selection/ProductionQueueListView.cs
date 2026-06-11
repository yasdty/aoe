using System;
using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AoE.RTS.Selection
{
    public class ProductionQueueListView : MonoBehaviour
    {
        readonly List<Button> buttonPool = new List<Button>();

        Transform container;
        float buttonHeight = 24f;

        public void Initialize(Transform parent, float preferredButtonHeight)
        {
            buttonHeight = preferredButtonHeight;
            container = parent;
        }

        public void Refresh(IReadOnlyList<ProductionQueueEntry> entries, Action<int> onCancel)
        {
            if (container == null)
                return;

            int requiredCount = entries != null ? entries.Count : 0;
            EnsurePoolSize(requiredCount);

            for (int i = 0; i < buttonPool.Count; i++)
            {
                Button button = buttonPool[i];
                if (button == null)
                    continue;

                if (i >= requiredCount || onCancel == null)
                {
                    button.gameObject.SetActive(false);
                    continue;
                }

                ProductionQueueEntry entry = entries[i];
                int queueIndex = entry.queueIndex;
                button.gameObject.SetActive(true);
                HudUiFactory.SetButtonLabel(
                    button,
                    Localization.Format(
                        "ui.cancel_queue",
                        queueIndex + 1,
                        Localization.LocalizeDisplayName(entry.displayName)));

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onCancel(queueIndex));
            }
        }

        public void HideAll()
        {
            for (int i = 0; i < buttonPool.Count; i++)
            {
                if (buttonPool[i] != null)
                    buttonPool[i].gameObject.SetActive(false);
            }
        }

        void EnsurePoolSize(int count)
        {
            while (buttonPool.Count < count)
            {
                Button button = HudUiFactory.CreateButton(container, $"QueueCancel{buttonPool.Count}", buttonHeight);
                buttonPool.Add(button);
            }
        }
    }
}
