using System.Collections.Generic;

namespace AoE.RTS.Core
{
    public enum MatchMode
    {
        OneVsOneCpu = 0,
        FourPlayerFfa = 1
    }

    public static class MatchSettings
    {
        static readonly PlayerId[] AllPlayers =
        {
            PlayerId.Player0,
            PlayerId.Player1,
            PlayerId.Player2,
            PlayerId.Player3
        };

        static readonly PlayerId[] ActivePlayerBuffer = new PlayerId[4];

        public static MatchMode Mode =>
            GameSessionManager.Instance != null
                ? GameSessionManager.Instance.MatchMode
                : MatchMode.FourPlayerFfa;

        public static int ActivePlayerCount
        {
            get
            {
                if (Mode == MatchMode.OneVsOneCpu)
                    return 2;

                if (GameSessionManager.Instance != null)
                    return GameSessionManager.Instance.FfaPlayerCount;

                return 4;
            }
        }

        public static bool IsCpu(PlayerId id) => id != PlayerId.Player0;

        public static IReadOnlyList<PlayerId> ActivePlayers
        {
            get
            {
                int count = ActivePlayerCount;
                for (int i = 0; i < count; i++)
                    ActivePlayerBuffer[i] = AllPlayers[i];

                return new System.ArraySegment<PlayerId>(ActivePlayerBuffer, 0, count);
            }
        }

        public static bool IsActivePlayer(PlayerId id)
        {
            IReadOnlyList<PlayerId> active = ActivePlayers;
            for (int i = 0; i < active.Count; i++)
            {
                if (active[i] == id)
                    return true;
            }

            return false;
        }

        public static bool IsOpponentOfHuman(PlayerId id) =>
            id != PlayerId.Player0 && IsActivePlayer(id);
    }
}
