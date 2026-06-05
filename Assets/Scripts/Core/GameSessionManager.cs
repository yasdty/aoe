using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Core
{
    public enum MatchState
    {
        Playing = 0,
        Victory = 1,
        Defeat = 2
    }

    public class GameSessionManager : MonoBehaviour
    {
        static GameSessionManager instance;
        static MatchState staticState = MatchState.Playing;

        MatchState state = MatchState.Playing;

        public static MatchState State => staticState;
        public static bool IsGameOver => staticState != MatchState.Playing;

        void Awake()
        {
            instance = this;
            staticState = MatchState.Playing;
            state = MatchState.Playing;
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public static void NotifyTownCenterDestroyed(UnitTeam destroyedTeam)
        {
            if (IsGameOver)
                return;

            if (destroyedTeam == UnitTeam.Player)
                staticState = MatchState.Defeat;
            else if (destroyedTeam == UnitTeam.Enemy)
                staticState = MatchState.Victory;
            else
                return;

            if (instance != null)
                instance.state = staticState;

            Debug.Log(staticState == MatchState.Victory
                ? "GameSession: VICTORY"
                : "GameSession: DEFEAT");
        }
    }
}
