using System.Collections.Generic;
using AoE.RTS.AI;
using AoE.RTS.Buildings;
using AoE.RTS.Economy;
using AoE.RTS.Units;
using UnityEngine;
using UnityEngine.InputSystem;

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
        const int PlayerCount = 4;

        static GameSessionManager instance;
        static MatchState staticState = MatchState.Playing;
        static readonly GameAge[] playerAges = new GameAge[PlayerCount];
        static readonly bool[] playerEliminated = new bool[PlayerCount];

        [SerializeField] MatchMode matchMode = MatchMode.FourPlayerFfa;
        [SerializeField] GameplayBalanceMode balanceMode = GameplayBalanceMode.Debug;
        [SerializeField] CpuAttackPace cpuAttackPace = CpuAttackPace.Relaxed;
        [SerializeField] AgeData feudalAgeData;
        [SerializeField] CivilizationData playerCivilization;
        [SerializeField] CivilizationData enemyCivilization;

        MatchState state = MatchState.Playing;

        public static GameSessionManager Instance => instance;
        public MatchMode MatchMode => matchMode;

        public static MatchState State => staticState;
        public static bool IsGameOver => staticState != MatchState.Playing;
        public static GameplayBalanceMode BalanceMode => GameplayBalance.Mode;
        public static CpuAttackPace CpuAttackPace =>
            instance != null ? instance.cpuAttackPace : CpuAttackPace.Relaxed;

        public static void SetCpuAttackPace(CpuAttackPace pace)
        {
            if (instance == null || instance.cpuAttackPace == pace)
                return;

            instance.cpuAttackPace = pace;
            CpuMilitaryAiManager[] militaryManagers = FindObjectsByType<CpuMilitaryAiManager>();
            for (int i = 0; i < militaryManagers.Length; i++)
            {
                if (militaryManagers[i] != null)
                    militaryManagers[i].NotifyCpuAttackPaceChanged(pace);
            }

            Debug.Log($"GameSession: CPU attack pace → {pace}");
        }

        public static void ToggleCpuAttackPace()
        {
            SetCpuAttackPace(CpuAttackPace == CpuAttackPace.Relaxed
                ? CpuAttackPace.Aggressive
                : CpuAttackPace.Relaxed);
        }

        public static void ToggleMatchMode()
        {
            if (instance == null)
                return;

            instance.matchMode = instance.matchMode == MatchMode.FourPlayerFfa
                ? MatchMode.OneVsOneCpu
                : MatchMode.FourPlayerFfa;
            Debug.Log($"GameSession: Match mode → {instance.matchMode} (reload scene to apply spawns)");
        }

        void Awake()
        {
            instance = this;
            staticState = MatchState.Playing;
            state = MatchState.Playing;
            for (int i = 0; i < PlayerCount; i++)
            {
                playerAges[i] = GameAge.Dark;
                playerEliminated[i] = false;
            }

            TechnologyState.Reset();
            GameplayBalance.SetMode(balanceMode);
        }

        void Update()
        {
            if (!Application.isPlaying || IsGameOver)
                return;

            if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
                ToggleMatchMode();
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public static GameAge GetAge(PlayerId playerId) => playerAges[PlayerIdMapping.ToIndex(playerId)];

        public static GameAge GetAge(UnitTeam team) => GetAge(PlayerIdMapping.FromLegacyTeam(team));

        public static CivilizationData GetCivilization(PlayerId playerId)
        {
            if (instance == null)
                return null;

            return playerId == PlayerId.Player0 ? instance.playerCivilization : instance.enemyCivilization;
        }

        public static CivilizationData GetCivilization(UnitTeam team) =>
            GetCivilization(PlayerIdMapping.FromLegacyTeam(team));

        public static bool CanBuild(PlacedBuildingData data, UnitTeam team) =>
            CanBuild(data, PlayerIdMapping.FromLegacyTeam(team));

        public static bool CanBuild(PlacedBuildingData data, PlayerId playerId)
        {
            if (data == null)
                return false;

            return GetAge(playerId) >= data.requiredAge;
        }

        public static bool CanAgeUpToFeudal(UnitTeam team) =>
            CanAgeUpToFeudal(PlayerIdMapping.FromLegacyTeam(team));

        public static bool CanAgeUpToFeudal(PlayerId playerId) =>
            GetAge(playerId) == GameAge.Dark && instance != null && instance.feudalAgeData != null;

        public static bool TryAgeUpToFeudal(TownCenter townCenter)
        {
            if (instance == null || townCenter == null || instance.feudalAgeData == null)
                return false;

            PlayerId playerId = townCenter.OwnerId;
            if (GetAge(playerId) >= GameAge.Feudal)
                return false;

            AgeData ageData = instance.feudalAgeData;
            float foodCost = GameplayBalance.ScaleResourceCost(ageData.upgradeFoodCost);
            float goldCost = GameplayBalance.ScaleResourceCost(ageData.upgradeGoldCost);

            if (foodCost > 0f && ResourceManager.GetFood(playerId) < foodCost)
                return false;

            if (goldCost > 0f && ResourceManager.GetGold(playerId) < goldCost)
                return false;

            if (foodCost > 0f && !ResourceManager.TrySpendFood(playerId, foodCost))
                return false;

            if (goldCost > 0f && !ResourceManager.TrySpendGold(playerId, goldCost))
            {
                if (foodCost > 0f)
                    ResourceManager.AddFood(playerId, foodCost);
                return false;
            }

            SetAge(playerId, GameAge.Feudal);
            Debug.Log($"GameSession: {playerId} advanced to Feudal Age");
            return true;
        }

        public static bool TryAgeUpForPlayer(PlayerId playerId)
        {
            TownCenter townCenter = ProductionManager.GetTownCenterForPlayer(playerId);
            if (townCenter == null)
                return false;

            return TryAgeUpToFeudal(townCenter);
        }

        public static bool TryAgeUpForTeam(UnitTeam team) =>
            TryAgeUpForPlayer(PlayerIdMapping.FromLegacyTeam(team));

        static void SetAge(PlayerId playerId, GameAge age) =>
            playerAges[PlayerIdMapping.ToIndex(playerId)] = age;

        public void ApplyBalanceModeFromInspector()
        {
            GameplayBalance.SetMode(balanceMode);
        }

        public static void NotifyTownCenterDestroyed(PlayerId destroyedPlayer)
        {
            if (IsGameOver)
                return;

            int index = PlayerIdMapping.ToIndex(destroyedPlayer);
            playerEliminated[index] = true;

            if (destroyedPlayer == PlayerId.Player0)
            {
                staticState = MatchState.Defeat;
                if (instance != null)
                    instance.state = staticState;
                Debug.Log("GameSession: DEFEAT");
                return;
            }

            if (!AllOpponentsEliminated())
                return;

            staticState = MatchState.Victory;
            if (instance != null)
                instance.state = staticState;
            Debug.Log("GameSession: VICTORY");
        }

        public static void NotifyTownCenterDestroyed(UnitTeam destroyedTeam) =>
            NotifyTownCenterDestroyed(PlayerIdMapping.FromLegacyTeam(destroyedTeam));

        static bool AllOpponentsEliminated()
        {
            IReadOnlyList<PlayerId> active = MatchSettings.ActivePlayers;
            for (int i = 0; i < active.Count; i++)
            {
                PlayerId playerId = active[i];
                if (!MatchSettings.IsOpponentOfHuman(playerId))
                    continue;

                if (!playerEliminated[PlayerIdMapping.ToIndex(playerId)]
                    && ProductionManager.HasAnyTownCenterForPlayer(playerId))
                    return false;
            }

            return true;
        }
    }
}
