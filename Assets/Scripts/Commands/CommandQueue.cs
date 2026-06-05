using System.Collections.Generic;
using AoE.RTS.Core;
using UnityEngine;

namespace AoE.RTS.Commands
{
    public class CommandQueue : MonoBehaviour, ISimulationTickable
    {
        static CommandQueue instance;
        static readonly Queue<IGameCommand> pendingBeforeAwake = new Queue<IGameCommand>();

        readonly Queue<IGameCommand> pending = new Queue<IGameCommand>();

        void Awake()
        {
            instance = this;
            SimulationTick.Register(this);

            while (pendingBeforeAwake.Count > 0)
                pending.Enqueue(pendingBeforeAwake.Dequeue());
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;

            SimulationTick.Unregister(this);
        }

        public static void Enqueue(IGameCommand command)
        {
            if (GameSessionManager.IsGameOver || command == null)
                return;

            if (instance == null)
            {
                pendingBeforeAwake.Enqueue(command);
                return;
            }

            instance.pending.Enqueue(command);
        }

        public void TickSimulation(float fixedDeltaTime)
        {
            int count = pending.Count;
            for (int i = 0; i < count; i++)
            {
                IGameCommand command = pending.Dequeue();
                command.Execute();
                CommandLog.Record(SimulationTick.CurrentTick, command);
            }
        }
    }
}
