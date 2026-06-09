using System.Collections.Generic;
using UnityEngine;

namespace AoE.RTS.Economy
{
    public static class DeerRegistry
    {
        static readonly List<DeerResource> deer = new List<DeerResource>();

        public static IReadOnlyList<DeerResource> All => deer;

        public static void Register(DeerResource deerResource)
        {
            if (deerResource == null || deer.Contains(deerResource))
                return;

            deer.Add(deerResource);
        }

        public static void Unregister(DeerResource deerResource)
        {
            if (deerResource == null)
                return;

            deer.Remove(deerResource);
        }
    }
}
