using System.Collections.Generic;
using AoE.RTS.Commands;
using AoE.RTS.Core;
using UnityEngine;

namespace AoE.RTS.AI
{
    public class CpuAiActionQueue : MonoBehaviour, ISimulationTickable
    {
        struct ScheduledAction
        {
            public float executeAt;
            public PlayerId playerId;
            public IGameCommand command;
        }

        static CpuAiActionQueue instance;
        static readonly Dictionary<PlayerId, int> cycleBudget = new Dictionary<PlayerId, int>(4);
        readonly List<ScheduledAction> pending = new List<ScheduledAction>(64);

        void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;

            SimulationTick.Unregister(this);
        }

        void Start()
        {
            SimulationTick.Register(this);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void EnsureExistsInPlayMode()
        {
            if (!Application.isPlaying || instance != null)
                return;

            GameObject systems = GameObject.Find("Systems");
            Transform parent = systems != null ? systems.transform : null;
            GameObject queueObject = new GameObject("CpuAiActionQueue");
            if (parent != null)
                queueObject.transform.SetParent(parent);

            queueObject.AddComponent<CpuAiActionQueue>();
        }

        public void TickSimulation(float fixedDeltaTime)
        {
            if (pending.Count == 0)
                return;

            float now = Time.timeSinceLevelLoad;
            for (int i = pending.Count - 1; i >= 0; i--)
            {
                ScheduledAction action = pending[i];
                if (action.executeAt > now)
                    continue;

                CpuAiCommandQueue.Enqueue(action.playerId, action.command);
                pending.RemoveAt(i);
            }
        }

        public static void BeginCycle(PlayerId playerId, int maxActionsPerCycle)
        {
            cycleBudget[playerId] = Mathf.Max(0, maxActionsPerCycle);
        }

        public static bool TrySchedule(PlayerId playerId, CpuAiActionKind kind, IGameCommand command)
        {
            if (command == null)
                return false;

            if (!cycleBudget.TryGetValue(playerId, out int budget) || budget <= 0)
                return false;

            CpuDifficultyProfile profile = CpuDifficultySettings.Current;
            float delay = profile.ReactionDelay + SampleHumanDelay(kind);
            float executeAt = Time.timeSinceLevelLoad + delay;

            if (instance == null)
            {
                CpuAiCommandQueue.Enqueue(playerId, command);
                cycleBudget[playerId] = budget - 1;
                return true;
            }

            instance.pending.Add(new ScheduledAction
            {
                executeAt = executeAt,
                playerId = playerId,
                command = command
            });
            cycleBudget[playerId] = budget - 1;
            return true;
        }

        static float SampleHumanDelay(CpuAiActionKind kind)
        {
            return kind switch
            {
                CpuAiActionKind.Build => Random.Range(0.5f, 2f),
                CpuAiActionKind.Train => Random.Range(0.3f, 1f),
                CpuAiActionKind.Attack => Random.Range(1f, 3f),
                CpuAiActionKind.Gather => Random.Range(0.5f, 1.5f),
                _ => 0f
            };
        }
    }
}
