using UnityEngine;
using UnityEngine.SceneManagement;

namespace AoE.RTS.Economy
{
    /// <summary>
    /// Phase10.unity が Setup 前の古い状態でも Play 時に鉱山と MineralGatherManager を補完する。
    /// AoE → Setup Phase10 Scene 実行後は鉱山が既にあるため何もしない。
    /// </summary>
    public static class Phase10MineralBootstrap
    {
        static readonly Vector3[] PlayerGoldMinePositions =
        {
            new Vector3(16f, 0f, 12f)
        };

        static readonly Vector3[] PlayerStoneMinePositions =
        {
            new Vector3(-16f, 0f, 12f)
        };

        static readonly Vector3[] CpuGoldMinePositions =
        {
            new Vector3(8f, 0f, -30f)
        };

        static readonly Vector3[] CpuStoneMinePositions =
        {
            new Vector3(-8f, 0f, -32f)
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void EnsurePhase10Minerals()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name != "Phase10")
                return;

            if (Object.FindAnyObjectByType<GoldMineResource>() == null)
            {
                for (int i = 0; i < PlayerGoldMinePositions.Length; i++)
                    MineralMineRuntimeFactory.CreateGoldMine(PlayerGoldMinePositions[i]);
                for (int i = 0; i < CpuGoldMinePositions.Length; i++)
                    MineralMineRuntimeFactory.CreateGoldMine(CpuGoldMinePositions[i]);
                for (int i = 0; i < PlayerStoneMinePositions.Length; i++)
                    MineralMineRuntimeFactory.CreateStoneMine(PlayerStoneMinePositions[i]);
                for (int i = 0; i < CpuStoneMinePositions.Length; i++)
                    MineralMineRuntimeFactory.CreateStoneMine(CpuStoneMinePositions[i]);

                Debug.Log("Phase10: Gold/Stone mines were missing — spawned at runtime. Run AoE → Setup Phase10 Scene to bake them into the scene.");
            }

            if (Object.FindAnyObjectByType<MineralGatherManager>() == null)
            {
                GameObject systems = GameObject.Find("Systems");
                GameObject host = systems != null ? systems : new GameObject("Systems");
                host.AddComponent<MineralGatherManager>();
                Debug.Log("Phase10: MineralGatherManager was missing — added at runtime.");
            }
        }
    }
}
