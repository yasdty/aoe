using System.Collections.Generic;

namespace AoE.RTS.View
{
    public interface IPlacementPreviewView
    {
        void ShowSinglePreview(PlacementPreviewState state);
        void ShowWallLinePreview(IReadOnlyList<PlacementPreviewState> segments);
        void HidePreview();
    }
}
