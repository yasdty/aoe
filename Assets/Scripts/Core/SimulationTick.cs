using System.Collections.Generic;
using UnityEngine;

namespace AoE.RTS.Core
{
    public class SimulationTick : MonoBehaviour
    {
        static SimulationTick instance;
        static readonly List<ISimulationTickable> pendingTickables = new List<ISimulationTickable>();

        readonly List<ISimulationTickable> tickables = new List<ISimulationTickable>();

        [SerializeField] int ticksPerSecond = 20;
        [SerializeField] int maxTicksPerFrame = 5;

        float accumulator;
        int currentTick;

        public static int CurrentTick => instance != null ? instance.currentTick : 0;
        public static float FixedDeltaTime => instance != null ? instance.TickInterval : 0.05f;

        float TickInterval => ticksPerSecond > 0 ? 1f / ticksPerSecond : 0.05f;

        void Awake()
        {
            instance = this;
            for (int i = 0; i < pendingTickables.Count; i++)
                RegisterInternal(pendingTickables[i]);

            pendingTickables.Clear();
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        void Update()
        {
            if (GameSessionManager.IsGameOver)
                return;

            float tickInterval = TickInterval;
            accumulator += Time.unscaledDeltaTime;

            int ticksThisFrame = 0;
            while (accumulator >= tickInterval && ticksThisFrame < maxTicksPerFrame)
            {
                accumulator -= tickInterval;
                currentTick++;
                ticksThisFrame++;

                for (int i = 0; i < tickables.Count; i++)
                    tickables[i].TickSimulation(tickInterval);
            }
        }

        public static void Register(ISimulationTickable tickable)
        {
            if (tickable == null)
                return;

            if (instance != null)
            {
                instance.RegisterInternal(tickable);
                return;
            }

            if (!pendingTickables.Contains(tickable))
                pendingTickables.Add(tickable);
        }

        public static void Unregister(ISimulationTickable tickable)
        {
            if (tickable == null)
                return;

            pendingTickables.Remove(tickable);
            instance?.tickables.Remove(tickable);
        }

        void RegisterInternal(ISimulationTickable tickable)
        {
            if (tickable == null || tickables.Contains(tickable))
                return;

            tickables.Add(tickable);
        }
    }
}
