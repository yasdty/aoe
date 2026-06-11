namespace AoE.RTS.View
{
    public static class PlacementPreviewViewRegistry
    {
        static IPlacementPreviewView current;

        public static IPlacementPreviewView Current => current;

        public static void Register(IPlacementPreviewView view)
        {
            current = view;
        }

        public static void Unregister(IPlacementPreviewView view)
        {
            if (current == view)
                current = null;
        }
    }
}
