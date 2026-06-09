using System.Collections.Generic;
using UnityEngine;

namespace AoE.RTS.Economy
{
    public static class SheepRegistry
    {
        static readonly List<SheepResource> sheep = new List<SheepResource>();

        public static IReadOnlyList<SheepResource> All => sheep;

        public static void Register(SheepResource sheepResource)
        {
            if (sheepResource == null || sheep.Contains(sheepResource))
                return;

            sheep.Add(sheepResource);
        }

        public static void Unregister(SheepResource sheepResource)
        {
            if (sheepResource == null)
                return;

            sheep.Remove(sheepResource);
        }
    }
}
