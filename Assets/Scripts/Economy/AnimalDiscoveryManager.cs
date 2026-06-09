using System.Collections.Generic;
using AoE.RTS.Core;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Economy
{
    public class AnimalDiscoveryManager : MonoBehaviour, ISimulationTickable
    {
        const float DiscoverRadius = 3f;

        static readonly List<Unit> unitBuffer = new List<Unit>();

        void Start()
        {
            SimulationTick.Register(this);
        }

        void OnDestroy()
        {
            SimulationTick.Unregister(this);
        }

        public void TickSimulation(float fixedDeltaTime)
        {
            IReadOnlyList<SheepResource> sheepList = SheepRegistry.All;
            if (sheepList.Count == 0)
                return;

            UnitManager.CopyUnitsTo(unitBuffer);
            for (int u = 0; u < unitBuffer.Count; u++)
            {
                Unit unit = unitBuffer[u];
                if (unit == null || !unit.IsAlive)
                    continue;

                Vector3 unitPosition = unit.transform.position;
                unitPosition.y = 0f;

                for (int s = 0; s < sheepList.Count; s++)
                {
                    SheepResource sheep = sheepList[s];
                    if (sheep == null || !sheep.IsNeutral || sheep.IsDepleted)
                        continue;

                    Vector3 sheepPosition = sheep.transform.position;
                    sheepPosition.y = 0f;
                    float radius = DiscoverRadius;
                    if ((sheepPosition - unitPosition).sqrMagnitude <= radius * radius)
                        sheep.Discover(unit.Team);
                }
            }
        }
    }
}
