using AoE.RTS.AI;
using AoE.RTS.Buildings;
using AoE.RTS.Camera;
using AoE.RTS.Combat;
using AoE.RTS.Commands;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Input;
using AoE.RTS.Selection;
using AoE.RTS.Spatial;
using AoE.RTS.Units;
using AoE.RTS.View;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AoE.RTS.EditorTools
{
    public static class Phase10SceneBuilder
    {
        const string ScenePath = "Assets/Scenes/Phase10.unity";
        const string FfaPlayerCountEditorPrefsKey = "AoE.Phase10.FfaPlayerCount";
        const float DefaultAttackWaveIntervalSeconds = 30f;
        const float DefaultBarracksBuildDelaySeconds = 60f;
        const float DefaultRelaxedFirstAttackGraceSeconds = 120f;
        const float DefaultRelaxedBarracksBuildDelaySeconds = 90f;
        const float DefaultRelaxedAttackWaveIntervalSeconds = 300f;

        static readonly Vector3 PlayerTownCenterPosition = Vector3.zero;
        static readonly Vector3 CpuTownCenterPosition = new Vector3(0f, 0f, -60f);
        static readonly Vector3 CameraFocus = new Vector3(0f, 0f, -30f);
        static readonly Vector3 FourPlayerCameraFocus = new Vector3(0f, 0f, 0f);
        static readonly Vector3 FourPlayerGroundScale = new Vector3(24f, 1f, 24f);
        static readonly Vector3 FourPlayerGroundPosition = Vector3.zero;
        const float FourPlayerSpawnInset = 72f;

        static readonly Vector3[] FourPlayerTownCenterPositions =
        {
            new Vector3(-FourPlayerSpawnInset, 0f, -FourPlayerSpawnInset),
            new Vector3(FourPlayerSpawnInset, 0f, FourPlayerSpawnInset),
            new Vector3(-FourPlayerSpawnInset, 0f, FourPlayerSpawnInset),
            new Vector3(FourPlayerSpawnInset, 0f, -FourPlayerSpawnInset)
        };

        /// <summary>2人=SW vs NE、3人=SE なし、4人=全隅。</summary>
        static readonly int[][] FfaCornerIndicesByPlayerCount =
        {
            null,
            null,
            new[] { 0, 1 },
            new[] { 0, 1, 2 },
            new[] { 0, 1, 2, 3 }
        };

        /// <summary>+X/+Z は常にマップ中心方向（各隅で MirrorStartOffset）。</summary>
        static readonly Vector3[] VillagerSpawnOffsets =
        {
            new Vector3(4f, 1f, 8f),
            new Vector3(8f, 1f, 4f),
            new Vector3(6f, 1f, 10f)
        };

        /// <summary>AoE2 標準: 6 Berry（Player0 基準オフセット、各隅でミラー）。</summary>
        static readonly Vector3[] AoE2BerryOffsets =
        {
            new Vector3(10f, 0f, 6f),
            new Vector3(-8f, 0f, 8f),
            new Vector3(6f, 0f, -6f),
            new Vector3(12f, 0f, 4f),
            new Vector3(-10f, 0f, 5f),
            new Vector3(4f, 0f, 10f)
        };

        /// <summary>AoE2 標準: 8 Sheep。</summary>
        static readonly Vector3[] AoE2SheepOffsets =
        {
            new Vector3(-2f, 0f, 10f),
            new Vector3(9f, 0f, 3f),
            new Vector3(-4f, 0f, 12f),
            new Vector3(6f, 0f, 11f),
            new Vector3(-8f, 0f, 8f),
            new Vector3(11f, 0f, 6f),
            new Vector3(2f, 0f, 14f),
            new Vector3(-6f, 0f, 14f)
        };

        /// <summary>AoE2 標準: 2 Boar。</summary>
        static readonly Vector3[] AoE2BoarOffsets =
        {
            new Vector3(4f, 0f, 11f),
            new Vector3(-4f, 0f, 9f)
        };

        /// <summary>AoE2 標準: Gold×2 / Stone×2。</summary>
        static readonly Vector3[] AoE2GoldMineOffsets =
        {
            new Vector3(16f, 0f, 12f),
            new Vector3(20f, 0f, 18f)
        };

        static readonly Vector3[] AoE2StoneMineOffsets =
        {
            new Vector3(-16f, 0f, 12f),
            new Vector3(-20f, 0f, 18f)
        };

        /// <summary>AoE2 標準: 3 森クラスター × 5 本（100 Wood/本）。</summary>
        static readonly Vector3[][] AoE2TreeClumpOffsets =
        {
            new[]
            {
                new Vector3(10f, 0f, 8f),
                new Vector3(12f, 0f, 10f),
                new Vector3(8f, 0f, 10f),
                new Vector3(14f, 0f, 8f),
                new Vector3(10f, 0f, 12f)
            },
            new[]
            {
                new Vector3(18f, 0f, 2f),
                new Vector3(20f, 0f, 4f),
                new Vector3(16f, 0f, 4f),
                new Vector3(22f, 0f, 0f),
                new Vector3(18f, 0f, -2f)
            },
            new[]
            {
                new Vector3(-12f, 0f, 6f),
                new Vector3(-14f, 0f, 8f),
                new Vector3(-10f, 0f, 8f),
                new Vector3(-16f, 0f, 4f),
                new Vector3(-12f, 0f, 10f)
            }
        };

        static readonly Vector3 SandboxGroundScale = new Vector3(18f, 1f, 18f);
        static readonly Vector3 SandboxGroundPosition = new Vector3(0f, 0f, -30f);

        static readonly Vector3[] TreePositions =
        {
            new Vector3(12f, 0f, 8f),
            new Vector3(-14f, 0f, 10f),
            new Vector3(18f, 0f, -6f),
            new Vector3(-10f, 0f, -12f),
            new Vector3(22f, 0f, 14f),
            new Vector3(-20f, 0f, -8f),
            new Vector3(8f, 0f, -18f),
            new Vector3(-16f, 0f, 18f),
            new Vector3(0f, 0f, 12f),
            new Vector3(6f, 0f, 16f),
            new Vector3(-8f, 0f, 14f),
            new Vector3(14f, 0f, 0f),
            new Vector3(-18f, 0f, 2f),
            new Vector3(4f, 0f, -8f),
            new Vector3(-6f, 0f, -6f),
            new Vector3(16f, 0f, 6f),
            new Vector3(-12f, 0f, 16f),
            new Vector3(20f, 0f, 4f),
            new Vector3(-22f, 0f, 6f),
            new Vector3(10f, 0f, 20f),
            new Vector3(-8f, 0f, 20f),
            new Vector3(24f, 0f, -4f),
            new Vector3(-18f, 0f, -4f),
            new Vector3(2f, 0f, 18f),
            new Vector3(-4f, 0f, -14f),
            new Vector3(0f, 0f, -18f),
            new Vector3(10f, 0f, -24f),
            new Vector3(-12f, 0f, -22f),
            new Vector3(6f, 0f, -32f),
            new Vector3(-8f, 0f, -34f),
            new Vector3(0f, 0f, -36f),
            new Vector3(14f, 0f, -38f),
            new Vector3(-14f, 0f, -40f),
            new Vector3(8f, 0f, -44f),
            new Vector3(-6f, 0f, -46f),
            new Vector3(0f, 0f, -48f),
            new Vector3(12f, 0f, -52f),
            new Vector3(-12f, 0f, -54f),
            new Vector3(8f, 0f, -58f),
            new Vector3(-8f, 0f, -56f),
            new Vector3(16f, 0f, -62f),
            new Vector3(-16f, 0f, -64f),
            new Vector3(6f, 0f, -68f),
            new Vector3(-6f, 0f, -70f),
            new Vector3(0f, 0f, -72f),
            new Vector3(14f, 0f, -66f),
            new Vector3(-14f, 0f, -68f),
            new Vector3(10f, 0f, -74f),
            new Vector3(-10f, 0f, -76f)
        };

        static readonly Vector3[] PlayerVillagerPositions =
        {
            new Vector3(-5f, 1f, 5f),
            new Vector3(5f, 1f, 5f),
            new Vector3(0f, 1f, 8f)
        };

        static readonly Vector3[] CpuVillagerPositions =
        {
            new Vector3(-5f, 1f, -55f),
            new Vector3(5f, 1f, -55f),
            new Vector3(0f, 1f, -68f)
        };

        static readonly Vector3[] PlayerBerryBushPositions =
        {
            new Vector3(10f, 0f, 6f),
            new Vector3(-8f, 0f, 8f),
            new Vector3(6f, 0f, -6f),
            new Vector3(12f, 0f, 4f),
            new Vector3(-10f, 0f, 5f)
        };

        static readonly Vector3[] CpuBerryBushPositions =
        {
            new Vector3(-8f, 0f, -53f),
            new Vector3(8f, 0f, -57f),
            new Vector3(0f, 0f, -63f),
            new Vector3(-10f, 0f, -58f),
            new Vector3(10f, 0f, -62f)
        };

        static readonly Vector3[] PlayerDeerPositions =
        {
            new Vector3(7f, 0f, 8f),
            new Vector3(-6f, 0f, 7f),
            new Vector3(5f, 0f, 5f),
            new Vector3(8f, 0f, 10f)
        };

        static readonly Vector3[] PlayerSheepPositions =
        {
            new Vector3(-2f, 0f, 10f),
            new Vector3(9f, 0f, 3f),
            new Vector3(-4f, 0f, 12f)
        };

        static readonly Vector3[] PlayerBoarPositions =
        {
            new Vector3(4f, 0f, 11f),
            new Vector3(-4f, 0f, 9f),
            new Vector3(6f, 0f, 12f)
        };

        static readonly Vector3[] CpuBoarPositions =
        {
            new Vector3(-5f, 0f, -55f),
            new Vector3(5f, 0f, -61f)
        };

        static readonly Vector3[] CpuDeerPositions =
        {
            new Vector3(-6f, 0f, -51f),
            new Vector3(6f, 0f, -59f),
            new Vector3(0f, 0f, -65f)
        };

        static readonly Vector3[] CpuSheepPositions =
        {
            new Vector3(0f, 0f, -67f),
            new Vector3(5f, 0f, -64f)
        };

        static readonly Vector3[] PlayerGoldMinePositions =
        {
            new Vector3(16f, 0f, 12f),
            new Vector3(20f, 0f, 18f)
        };

        static readonly Vector3[] PlayerStoneMinePositions =
        {
            new Vector3(-16f, 0f, 12f),
            new Vector3(-20f, 0f, 18f)
        };

        static readonly Vector3[] CpuGoldMinePositions =
        {
            new Vector3(10f, 0f, -55f),
            new Vector3(18f, 0f, -70f)
        };

        static readonly Vector3[] CpuStoneMinePositions =
        {
            new Vector3(-10f, 0f, -57f),
            new Vector3(-18f, 0f, -72f)
        };

        [MenuItem("AoE/Setup Phase10 Scene", true)]
        static bool ValidateSetupPhase10Scene() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Setup Phase10 Scene")]
        public static void SetupPhase10Scene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            Phase1SceneBuilder.EnsureLayers();
            UnitData villagerData = Phase1SceneBuilder.EnsureDefaultUnitData();
            UnitData militiaData = Phase1SceneBuilder.EnsureMilitiaData();
            UnitData spearmanData = Phase1SceneBuilder.EnsureSpearmanData();
            UnitData archerData = Phase1SceneBuilder.EnsureArcherData();
            UnitData cavalryData = Phase1SceneBuilder.EnsureCavalryData();
            UnitData scoutData = Phase1SceneBuilder.EnsureScoutData();
            BuildingData townCenterData = Phase1SceneBuilder.EnsureTownCenterData(villagerData);
            ResourceNodeData treeData = Phase1SceneBuilder.EnsureDefaultTreeData();
            FoodNodeData berryBushData = Phase1SceneBuilder.EnsureDefaultBerryBushData();
            FoodNodeData deerData = Phase1SceneBuilder.EnsureDefaultDeerData();
            FoodNodeData sheepData = Phase1SceneBuilder.EnsureDefaultSheepData();
            FoodNodeData boarData = Phase1SceneBuilder.EnsureDefaultBoarData();
            MineralNodeData goldMineData = Phase1SceneBuilder.EnsureDefaultGoldMineData();
            MineralNodeData stoneMineData = Phase1SceneBuilder.EnsureDefaultStoneMineData();
            PlacedBuildingData houseData = Phase1SceneBuilder.EnsureHouseData();
            PlacedBuildingData barracksData = Phase1SceneBuilder.EnsureBarracksData(militiaData, spearmanData);
            PlacedBuildingData archeryRangeData = Phase1SceneBuilder.EnsureArcheryRangeData(archerData);
            PlacedBuildingData stableData = Phase1SceneBuilder.EnsureStableData(cavalryData, scoutData);
            PlacedBuildingData blacksmithData = Phase1SceneBuilder.EnsureBlacksmithData();
            PlacedBuildingData palisadeWallData = Phase1SceneBuilder.EnsurePalisadeWallData();
            PlacedBuildingData stoneWallData = Phase1SceneBuilder.EnsureStoneWallData();
            PlacedBuildingData gateData = Phase1SceneBuilder.EnsureGateData();
            PlacedBuildingData watchTowerData = Phase1SceneBuilder.EnsureWatchTowerData();
            PlacedBuildingData marketData = Phase1SceneBuilder.EnsureMarketData();
            MarketTradeData marketTradeData = Phase1SceneBuilder.EnsureMarketTradeData();
            PlacedBuildingData townCenterPlacementData = Phase1SceneBuilder.EnsureTownCenterPlacementData();
            PlacedBuildingData millData = Phase1SceneBuilder.EnsureMillData();
            UnitData manAtArmsData = Phase1SceneBuilder.EnsureManAtArmsData();
            TechnologyData infantryUpgradeTech = Phase1SceneBuilder.EnsureInfantryUpgradeTech(militiaData, manAtArmsData);
            Phase1SceneBuilder.EnsureFarmData();
            Phase1SceneBuilder.EnsureLumberCampData();
            Phase1SceneBuilder.EnsureMiningCampData();
            Phase1SceneBuilder.EnsureMillData();
            Phase1SceneBuilder.EnsureFeudalAgeData();
            InputActionAsset inputActions = RTSInputActionsFactory.EnsureAsset();
            if (inputActions == null)
            {
                Debug.LogError("Failed to create RTSInputActions. Setup aborted.");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Phase1SceneBuilder.CreateLighting();
            bool fourPlayerFfa = ShouldBuildFourPlayerFfa();
            CreateSandboxGround(fourPlayerFfa);
            GameObject playerTownCenter;
            if (fourPlayerFfa)
            {
                playerTownCenter = CreateFourPlayerMatch(
                    townCenterData,
                    villagerData,
                    treeData,
                    berryBushData,
                    sheepData,
                    boarData,
                    goldMineData,
                    stoneMineData);
            }
            else
            {
                playerTownCenter = Phase1SceneBuilder.CreateTownCenter(townCenterData, PlayerTownCenterPosition);
                TownCenter playerTc = playerTownCenter.GetComponent<TownCenter>();
                playerTc.SetOwner(PlayerId.Player0);
                GameObject cpuTownCenter = CreateCpuTownCenter(townCenterData);
                CreateTrees(treeData);
                CreateBerryBushes(berryBushData);
                CreateHuntableAnimals(deerData, sheepData);
                CreateBoars(boarData);
                CreateMineralMines(goldMineData, stoneMineData);
                CreateStartingVillagersForPlayer(villagerData, PlayerVillagerPositions, PlayerId.Player0);
                CreateStartingVillagersForPlayer(villagerData, CpuVillagerPositions, PlayerId.Player1);
            }

            GameObject cameraRig = Phase1SceneBuilder.CreateCameraRig(inputActions);
            Vector3 cameraFocus = ShouldBuildFourPlayerFfa() ? FourPlayerCameraFocus : CameraFocus;
            Phase1SceneBuilder.ApplyOverviewCamera(cameraRig.transform, cameraFocus);
            CreateManagers(
                inputActions,
                cameraRig.GetComponent<UnityEngine.Camera>(),
                houseData,
                barracksData,
                archeryRangeData,
                stableData,
                blacksmithData,
                palisadeWallData,
                stoneWallData,
                gateData,
                watchTowerData,
                marketData,
                marketTradeData,
                townCenterPlacementData,
                millData,
                infantryUpgradeTech,
                villagerData,
                militiaData);

            Phase1SceneBuilder.AssignInputActionsToReaders(inputActions);
            EditorSceneManager.SaveScene(scene, ScenePath);
            UnityEditor.Selection.activeGameObject = playerTownCenter;

            Debug.Log("Phase10 scene created at " + ScenePath);
        }

        [MenuItem("AoE/Add Huntable Animals (Phase10)", true)]
        static bool ValidateAddHuntableAnimals() => !EditorApplication.isPlaying;

        /// <summary>
        /// 既存 Phase10 シーンに Deer / Sheep を追加（フル Setup 不要）。
        /// Phase 24 以降、古い Phase10.unity を使っている場合に実行する。
        /// </summary>
        [MenuItem("AoE/Add Huntable Animals (Phase10)")]
        public static void AddHuntableAnimalsToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            Phase1SceneBuilder.EnsureLayers();
            FoodNodeData deerData = Phase1SceneBuilder.EnsureDefaultDeerData();
            FoodNodeData sheepData = Phase1SceneBuilder.EnsureDefaultSheepData();

            RemoveExistingHuntableAnimals();
            CreateHuntableAnimals(deerData, sheepData);
            ResetAllSheepToNeutral();
            EnsureAnimalManagers();

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added Deer/Sheep to the open scene. Save the scene (Ctrl+S) if needed.");
        }

        [MenuItem("AoE/Add Selection Info Panel (Phase10)", true)]
        static bool ValidateAddSelectionInfoPanel() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add Selection Info Panel (Phase10)")]
        public static void AddSelectionInfoPanelToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            SelectionManager selectionManager = Object.FindAnyObjectByType<SelectionManager>();
            if (selectionManager == null)
            {
                Debug.LogError("SelectionManager not found in the open scene.");
                return;
            }

            SelectionInfoPanelView existing = selectionManager.GetComponent<SelectionInfoPanelView>();
            if (existing != null)
            {
                Debug.Log("SelectionInfoPanelView already exists on SelectionManager.");
                return;
            }

            SelectionInfoPanelView infoPanel = selectionManager.gameObject.AddComponent<SelectionInfoPanelView>();
            SerializedObject serializedInfoPanel = new SerializedObject(infoPanel);
            serializedInfoPanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedInfoPanel.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added SelectionInfoPanelView. Save the scene (Ctrl+S) if needed.");
        }

        [MenuItem("AoE/Add Boars (Phase10)", true)]
        static bool ValidateAddBoars() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add Boars (Phase10)")]
        public static void AddBoarsToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            Phase1SceneBuilder.EnsureLayers();
            FoodNodeData boarData = Phase1SceneBuilder.EnsureDefaultBoarData();
            EnsureBoarManagers();

            BoarResource[] existing = Object.FindObjectsByType<BoarResource>();
            for (int i = 0; i < existing.Length; i++)
                Object.DestroyImmediate(existing[i].gameObject);

            CreateBoars(boarData);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added Boars to the open scene. Save the scene (Ctrl+S) if needed.");
        }

        static void EnsureBoarManagers()
        {
            if (Object.FindAnyObjectByType<BoarAggroManager>() == null)
            {
                GameObject systems = GameObject.Find("Systems");
                Transform parent = systems != null ? systems.transform : null;
                GameObject aggroObject = new GameObject("BoarAggroManager");
                if (parent != null)
                    aggroObject.transform.SetParent(parent);
                aggroObject.AddComponent<BoarAggroManager>();
            }

            if (Object.FindAnyObjectByType<BoarAttackManager>() == null)
            {
                GameObject systems = GameObject.Find("Systems");
                Transform parent = systems != null ? systems.transform : null;
                GameObject attackObject = new GameObject("BoarAttackManager");
                if (parent != null)
                    attackObject.transform.SetParent(parent);
                attackObject.AddComponent<BoarAttackManager>();
            }
        }

        [MenuItem("AoE/Reset Sheep to Neutral (Phase10)", true)]
        static bool ValidateResetSheepToNeutral() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Reset Sheep to Neutral (Phase10)")]
        public static void ResetSheepToNeutralInOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            ResetAllSheepToNeutral();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Reset all Sheep to Neutral. Save the scene (Ctrl+S) if needed.");
        }

        [MenuItem("AoE/Add Animal Locomotion (Phase10)", true)]
        static bool ValidateAddAnimalLocomotion() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add Animal Locomotion (Phase10)")]
        public static void AddAnimalLocomotionToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            EnsureAnimalManagers();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added AnimalDiscoveryManager + PassiveAnimalLocomotionManager. Save the scene (Ctrl+S) if needed.");
        }

        [MenuItem("AoE/Add Unit Aggro (Phase10)", true)]
        static bool ValidateAddUnitAggro() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add Unit Aggro (Phase10)")]
        public static void AddUnitAggroToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            EnsureUnitAggroManager();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added UnitAggroManager. Save the scene (Ctrl+S) if needed.");
        }

        [MenuItem("AoE/Add Stance & Attack-Move (Phase40)", true)]
        static bool ValidateAddStanceAttackMove() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add Stance & Attack-Move (Phase40)")]
        public static void AddStanceAttackMoveToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            InputActionAsset inputActions = RTSInputActionsFactory.EnsureAsset();
            if (inputActions == null)
            {
                Debug.LogError("Failed to create RTSInputActions. Setup aborted.");
                return;
            }

            Phase1SceneBuilder.AssignInputActionsToReaders(inputActions);
            EnsureAttackMoveManager();
            EnsureFormationMoveManager();

            SelectionManager selectionManager = Object.FindAnyObjectByType<SelectionManager>();
            if (selectionManager != null)
            {
                UnitStancePanelView existing = selectionManager.GetComponent<UnitStancePanelView>();
                if (existing == null)
                {
                    UnitStancePanelView stancePanel = selectionManager.gameObject.AddComponent<UnitStancePanelView>();
                    SerializedObject serializedStancePanel = new SerializedObject(stancePanel);
                    serializedStancePanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
                    serializedStancePanel.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            else
            {
                Debug.LogWarning("SelectionManager not found — UnitStancePanelView was not added.");
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added AttackMoveManager + UnitStancePanelView and refreshed input actions. Save the scene (Ctrl+S) if needed.");
        }

        [MenuItem("AoE/Add Blacksmith & Tech (Phase43)", true)]
        static bool ValidateAddBlacksmithTech() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add Blacksmith & Tech (Phase43)")]
        public static void AddBlacksmithTechToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            UnitData militiaData = Phase1SceneBuilder.EnsureMilitiaData();
            UnitData manAtArmsData = Phase1SceneBuilder.EnsureManAtArmsData();
            TechnologyData infantryUpgradeTech = Phase1SceneBuilder.EnsureInfantryUpgradeTech(militiaData, manAtArmsData);
            PlacedBuildingData blacksmithData = Phase1SceneBuilder.EnsureBlacksmithData();

            GameObject systems = GameObject.Find("Systems");
            Transform systemsTransform = systems != null ? systems.transform : null;
            if (Object.FindAnyObjectByType<BlacksmithResearchManager>() == null)
            {
                GameObject blacksmithResearchObject = new GameObject("BlacksmithResearchManager");
                if (systemsTransform != null)
                    blacksmithResearchObject.transform.SetParent(systemsTransform);
                blacksmithResearchObject.AddComponent<BlacksmithResearchManager>();
            }

            SelectionManager selectionManager = Object.FindAnyObjectByType<SelectionManager>();
            if (selectionManager != null)
            {
                BlacksmithPanelView blacksmithPanel = selectionManager.GetComponent<BlacksmithPanelView>();
                if (blacksmithPanel == null)
                    blacksmithPanel = selectionManager.gameObject.AddComponent<BlacksmithPanelView>();

                RTSInputReader inputReader = Object.FindAnyObjectByType<RTSInputReader>();
                SerializedObject serializedBlacksmithPanel = new SerializedObject(blacksmithPanel);
                serializedBlacksmithPanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
                serializedBlacksmithPanel.FindProperty("input").objectReferenceValue = inputReader;
                serializedBlacksmithPanel.FindProperty("infantryUpgradeTech").objectReferenceValue = infantryUpgradeTech;
                serializedBlacksmithPanel.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogWarning("SelectionManager not found — BlacksmithPanelView was not added.");
            }

            BuildingPlacementManager placementManager = Object.FindAnyObjectByType<BuildingPlacementManager>();
            if (placementManager != null)
            {
                SerializedObject serializedPlacement = new SerializedObject(placementManager);
                serializedPlacement.FindProperty("blacksmithData").objectReferenceValue = blacksmithData;
                serializedPlacement.ApplyModifiedPropertiesWithoutUndo();
            }

            ResourceHudView resourceHud = Object.FindAnyObjectByType<ResourceHudView>();
            if (resourceHud != null)
            {
                SerializedObject serializedResourceHud = new SerializedObject(resourceHud);
                serializedResourceHud.FindProperty("blacksmithData").objectReferenceValue = blacksmithData;
                serializedResourceHud.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added Blacksmith & Tech wiring. Save the scene (Ctrl+S) if needed.");
        }

        [MenuItem("AoE/Add Defense (Phase44)", true)]
        static bool ValidateAddDefense() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add Defense (Phase44)")]
        public static void AddDefenseToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            PlacedBuildingData palisadeWallData = Phase1SceneBuilder.EnsurePalisadeWallData();
            PlacedBuildingData stoneWallData = Phase1SceneBuilder.EnsureStoneWallData();
            PlacedBuildingData gateData = Phase1SceneBuilder.EnsureGateData();
            PlacedBuildingData watchTowerData = Phase1SceneBuilder.EnsureWatchTowerData();

            GameObject systems = GameObject.Find("Systems");
            Transform systemsTransform = systems != null ? systems.transform : null;
            if (Object.FindAnyObjectByType<WatchTowerDefenseManager>() == null)
            {
                GameObject watchTowerDefenseObject = new GameObject("WatchTowerDefenseManager");
                if (systemsTransform != null)
                    watchTowerDefenseObject.transform.SetParent(systemsTransform);
                watchTowerDefenseObject.AddComponent<WatchTowerDefenseManager>();
            }

            BuildingPlacementManager placementManager = Object.FindAnyObjectByType<BuildingPlacementManager>();
            if (placementManager != null)
            {
                SerializedObject serializedPlacement = new SerializedObject(placementManager);
                serializedPlacement.FindProperty("palisadeWallData").objectReferenceValue = palisadeWallData;
                serializedPlacement.FindProperty("stoneWallData").objectReferenceValue = stoneWallData;
                serializedPlacement.FindProperty("watchTowerData").objectReferenceValue = watchTowerData;
                serializedPlacement.FindProperty("gateData").objectReferenceValue = gateData;
                serializedPlacement.ApplyModifiedPropertiesWithoutUndo();
            }

            ResourceHudView resourceHud = Object.FindAnyObjectByType<ResourceHudView>();
            if (resourceHud != null)
            {
                SerializedObject serializedResourceHud = new SerializedObject(resourceHud);
                serializedResourceHud.FindProperty("palisadeWallData").objectReferenceValue = palisadeWallData;
                serializedResourceHud.FindProperty("stoneWallData").objectReferenceValue = stoneWallData;
                serializedResourceHud.FindProperty("gateData").objectReferenceValue = gateData;
                serializedResourceHud.FindProperty("watchTowerData").objectReferenceValue = watchTowerData;
                serializedResourceHud.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added Defense wiring. Save the scene (Ctrl+S) if needed.");
        }

        [MenuItem("AoE/Add Market (Phase45)", true)]
        static bool ValidateAddMarket() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add Market (Phase45)")]
        public static void AddMarketToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            PlacedBuildingData marketData = Phase1SceneBuilder.EnsureMarketData();
            MarketTradeData marketTradeData = Phase1SceneBuilder.EnsureMarketTradeData();

            SelectionManager selectionManager = Object.FindAnyObjectByType<SelectionManager>();
            if (selectionManager != null)
            {
                MarketPanelView marketPanel = selectionManager.GetComponent<MarketPanelView>();
                if (marketPanel == null)
                    marketPanel = selectionManager.gameObject.AddComponent<MarketPanelView>();

                SerializedObject serializedMarketPanel = new SerializedObject(marketPanel);
                serializedMarketPanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
                serializedMarketPanel.FindProperty("tradeRates").objectReferenceValue = marketTradeData;
                serializedMarketPanel.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogWarning("SelectionManager not found — MarketPanelView was not added.");
            }

            BuildingPlacementManager placementManager = Object.FindAnyObjectByType<BuildingPlacementManager>();
            if (placementManager != null)
            {
                SerializedObject serializedPlacement = new SerializedObject(placementManager);
                serializedPlacement.FindProperty("marketData").objectReferenceValue = marketData;
                serializedPlacement.ApplyModifiedPropertiesWithoutUndo();
            }

            ResourceHudView resourceHud = Object.FindAnyObjectByType<ResourceHudView>();
            if (resourceHud != null)
            {
                SerializedObject serializedResourceHud = new SerializedObject(resourceHud);
                serializedResourceHud.FindProperty("marketData").objectReferenceValue = marketData;
                serializedResourceHud.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added Market wiring. Save the scene (Ctrl+S) if needed.");
        }

        [MenuItem("AoE/Add Civilization (Phase46)", true)]
        static bool ValidateAddCivilization() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add Civilization (Phase46)")]
        public static void AddCivilizationToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            CivilizationData playerCivilization = Phase1SceneBuilder.EnsureDefaultPlayerCivilizationData();
            CivilizationData cpuCivilization = Phase1SceneBuilder.EnsureDefaultCpuCivilizationData();

            GameSessionManager sessionManager = Object.FindAnyObjectByType<GameSessionManager>();
            if (sessionManager != null)
            {
                SerializedObject serializedSession = new SerializedObject(sessionManager);
                serializedSession.FindProperty("playerCivilization").objectReferenceValue = playerCivilization;
                serializedSession.FindProperty("enemyCivilization").objectReferenceValue = cpuCivilization;
                serializedSession.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(sessionManager);
            }
            else
            {
                Debug.LogWarning("GameSessionManager not found — civilization data was not wired.");
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added Civilization wiring. Save the scene (Ctrl+S) if needed.");
        }

        [MenuItem("AoE/Add Second TC (Phase47)", true)]
        static bool ValidateAddSecondTc() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add Second TC (Phase47)")]
        public static void AddSecondTcToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            PlacedBuildingData townCenterPlacementData = Phase1SceneBuilder.EnsureTownCenterPlacementData();

            BuildingPlacementManager placementManager = Object.FindAnyObjectByType<BuildingPlacementManager>();
            if (placementManager != null)
            {
                SerializedObject serializedPlacement = new SerializedObject(placementManager);
                serializedPlacement.FindProperty("townCenterPlacementData").objectReferenceValue =
                    townCenterPlacementData;
                serializedPlacement.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(placementManager);
            }
            else
            {
                Debug.LogWarning("BuildingPlacementManager not found — Second TC data was not wired.");
            }

            ResourceHudView resourceHud = Object.FindAnyObjectByType<ResourceHudView>();
            if (resourceHud != null)
            {
                SerializedObject serializedResourceHud = new SerializedObject(resourceHud);
                serializedResourceHud.FindProperty("townCenterPlacementData").objectReferenceValue =
                    townCenterPlacementData;
                serializedResourceHud.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(resourceHud);
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added Second TC wiring. Save the scene (Ctrl+S) if needed.");
        }

        [MenuItem("AoE/Add View Layer (Phase52)", true)]
        static bool ValidateAddViewLayerPhase52() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add View Layer (Phase52)")]
        public static void AddViewLayerToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            EnsureGameplayCanvas();
            PlacementPreviewView previewView = EnsurePlacementPreviewView();
            BuildingPlacementManager placementManager = Object.FindAnyObjectByType<BuildingPlacementManager>();
            if (placementManager != null && previewView != null)
            {
                SerializedObject serializedPlacement = new SerializedObject(placementManager);
                serializedPlacement.FindProperty("placementPreviewViewHost").objectReferenceValue = previewView;
                serializedPlacement.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(placementManager);
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added View Layer (Phase52). Save the scene (Ctrl+S) if needed.");
        }

        [MenuItem("AoE/Migrate HUD to uGUI (Phase53)", true)]
        static bool ValidateMigrateHudPhase53() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Migrate HUD to uGUI (Phase53)")]
        public static void MigrateHudToUgUiPhase53()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            EnsureHudUi();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Migrated HUD shell to uGUI (Phase53). Play mode builds panel widgets at runtime. Save the scene (Ctrl+S) if needed.");
        }

        public static void EnsureHudUi()
        {
            EnsureGameplayCanvas();
            EnsureInputSystemEventSystem();
            Transform hudRoot = HudUiFactory.GetHudRoot();
            if (hudRoot == null)
                return;

            HudUiFactory.GetOrCreateHudChild(hudRoot, "ResourceHudPanel");
            HudUiFactory.GetOrCreateHudChild(hudRoot, "IdleHudPanel");
            HudUiFactory.GetOrCreateHudChild(hudRoot, "GameTimeHudPanel");
            HudUiFactory.GetOrCreateHudChild(hudRoot, "VictoryOverlay");
            HudUiFactory.GetOrCreateHudChild(hudRoot, "PlacementHintPanel");
            HudUiFactory.GetOrCreateHudChild(hudRoot, "MinimapPanel");
            HudBottomLeftStack.GetOrCreate();
        }

        [MenuItem("AoE/Add Minimap (Phase54)", true)]
        static bool ValidateAddMinimapPhase54() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add Minimap (Phase54)")]
        public static void AddMinimapPhase54()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            EnsureMinimap();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added Minimap shell (Phase54). Play mode builds widgets at runtime. Save the scene (Ctrl+S) if needed.");
        }

        public static void EnsureMinimap()
        {
            EnsureHudUi();
            Transform hudRoot = HudUiFactory.GetHudRoot();
            if (hudRoot == null)
                return;

            HudUiFactory.GetOrCreateHudChild(hudRoot, "MinimapPanel");

            SelectionManager selectionManager = Object.FindAnyObjectByType<SelectionManager>();
            if (selectionManager != null && selectionManager.GetComponent<MinimapView>() == null)
                selectionManager.gameObject.AddComponent<MinimapView>();
        }

        [MenuItem("AoE/Add Combat Feedback (Phase56)", true)]
        static bool ValidateAddCombatFeedbackPhase56() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add Combat Feedback (Phase56)")]
        public static void AddCombatFeedbackPhase56()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            EnsureCombatFeedbackView();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added CombatFeedbackView (Phase56). Save the scene (Ctrl+S) if needed.");
        }

        public static void EnsureCombatFeedbackView()
        {
            GameObject systems = GameObject.Find("Systems");
            CombatFeedbackView.Ensure(systems);
        }

        static void EnsureInputSystemEventSystem()
        {
            EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventSystemObject = new GameObject("EventSystem");
                eventSystem = eventSystemObject.AddComponent<EventSystem>();
            }

            StandaloneInputModule legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (legacyModule != null)
                Object.DestroyImmediate(legacyModule);

            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                EditorUtility.SetDirty(eventSystem);
            }
        }

        static GameObject EnsureGameplayCanvas()
        {
            EnsureInputSystemEventSystem();

            GameObject canvasObject = GameObject.Find("GameplayCanvas");
            if (canvasObject != null)
                return canvasObject;

            canvasObject = new GameObject("GameplayCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject hudRoot = new GameObject("HudRoot");
            RectTransform hudRootTransform = hudRoot.AddComponent<RectTransform>();
            hudRootTransform.SetParent(canvasObject.transform, false);
            hudRootTransform.anchorMin = Vector2.zero;
            hudRootTransform.anchorMax = Vector2.one;
            hudRootTransform.offsetMin = Vector2.zero;
            hudRootTransform.offsetMax = Vector2.zero;

            return canvasObject;
        }

        static PlacementPreviewView EnsurePlacementPreviewView()
        {
            PlacementPreviewView existing = Object.FindAnyObjectByType<PlacementPreviewView>();
            if (existing != null)
                return existing;

            GameObject viewObject = new GameObject("PlacementPreviewView");
            return viewObject.AddComponent<PlacementPreviewView>();
        }

        [MenuItem("AoE/Add Debug Playtest Input (Phase47)", true)]
        static bool ValidateAddDebugPlaytestInput() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add Debug Playtest Input (Phase47)")]
        public static void AddDebugPlaytestInputToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            GameObject systems = GameObject.Find("Systems");
            if (systems == null)
            {
                Debug.LogWarning("Systems object not found — DebugPlaytestInput was not added.");
                return;
            }

            if (systems.GetComponent<DebugPlaytestInput>() == null)
                systems.AddComponent<DebugPlaytestInput>();

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added DebugPlaytestInput to Systems. Save the scene (Ctrl+S) if needed.");
        }

        public static void WireTownCenterDataInOpenScene()
        {
            UnitData villagerData = Phase1SceneBuilder.EnsureDefaultUnitData();
            BuildingData townCenterData = Phase1SceneBuilder.EnsureTownCenterData(villagerData);
            if (townCenterData == null)
                return;

            TownCenter[] townCenters = Object.FindObjectsByType<TownCenter>();
            int wired = 0;
            for (int i = 0; i < townCenters.Length; i++)
            {
                TownCenter townCenter = townCenters[i];
                if (townCenter == null || townCenter.Data == townCenterData)
                    continue;

                SerializedObject serialized = new SerializedObject(townCenter);
                serialized.FindProperty("data").objectReferenceValue = townCenterData;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(townCenter);
                wired++;
            }

            if (wired > 0)
                Debug.Log($"Wired TownCenterData on {wired} Town Center(s).");
        }

        public static void WireUnitsInOpenScene()
        {
            UnitData villagerData = Phase1SceneBuilder.EnsureDefaultUnitData();
            UnitData militiaData = Phase1SceneBuilder.EnsureMilitiaData();
            if (villagerData == null)
                return;

            int wiredUnits = 0;
            Unit[] units = Object.FindObjectsByType<Unit>();
            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (unit == null || unit.Data != null)
                    continue;

                UnitData assign = villagerData;
                if (militiaData != null && unit.CanAttack)
                    assign = militiaData;

                SerializedObject serialized = new SerializedObject(unit);
                serialized.FindProperty("data").objectReferenceValue = assign;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                unit.SetData(assign);
                EditorUtility.SetDirty(unit);
                wiredUnits++;
            }

            UnitPool unitPool = Object.FindAnyObjectByType<UnitPool>();
            if (unitPool != null)
            {
                SerializedObject serializedPool = new SerializedObject(unitPool);
                SerializedProperty villagerProp = serializedPool.FindProperty("prewarmVillagerData");
                SerializedProperty militiaProp = serializedPool.FindProperty("prewarmMilitiaData");
                bool poolDirty = false;

                if (villagerProp != null && villagerProp.objectReferenceValue != villagerData)
                {
                    villagerProp.objectReferenceValue = villagerData;
                    poolDirty = true;
                }

                if (militiaProp != null && militiaData != null && militiaProp.objectReferenceValue != militiaData)
                {
                    militiaProp.objectReferenceValue = militiaData;
                    poolDirty = true;
                }

                if (poolDirty)
                {
                    serializedPool.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(unitPool);
                }
            }

            if (wiredUnits > 0)
                Debug.Log($"Wired UnitData on {wiredUnits} unit(s). Villagers should show blue + 'Villager'.");
        }

        static void EnsureAttackMoveManager()
        {
            if (Object.FindAnyObjectByType<AttackMoveManager>() != null)
                return;

            GameObject systems = GameObject.Find("Systems");
            Transform parent = systems != null ? systems.transform : null;
            GameObject attackMoveObject = new GameObject("AttackMoveManager");
            if (parent != null)
                attackMoveObject.transform.SetParent(parent);
            attackMoveObject.AddComponent<AttackMoveManager>();
        }

        static void EnsureFormationMoveManager()
        {
            if (Object.FindAnyObjectByType<FormationMoveManager>() != null)
                return;

            GameObject systems = GameObject.Find("Systems");
            Transform parent = systems != null ? systems.transform : null;
            GameObject formationObject = new GameObject("FormationMoveManager");
            if (parent != null)
                formationObject.transform.SetParent(parent);
            formationObject.AddComponent<FormationMoveManager>();
        }

        [MenuItem("AoE/Add Formation (Phase41)", true)]
        static bool ValidateAddFormation() => !EditorApplication.isPlaying;

        [MenuItem("AoE/Add Formation (Phase41)")]
        public static void AddFormationToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            EnsureFormationMoveManager();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Added FormationMoveManager. Save the scene (Ctrl+S) if needed.");
        }

        static void EnsureUnitAggroManager()
        {
            if (Object.FindAnyObjectByType<UnitAggroManager>() != null)
                return;

            GameObject systems = GameObject.Find("Systems");
            Transform parent = systems != null ? systems.transform : null;
            GameObject aggroObject = new GameObject("UnitAggroManager");
            if (parent != null)
                aggroObject.transform.SetParent(parent);
            aggroObject.AddComponent<UnitAggroManager>();
        }

        static void EnsureAnimalManagers()
        {
            if (Object.FindAnyObjectByType<AnimalDiscoveryManager>() == null)
            {
                GameObject systems = GameObject.Find("Systems");
                Transform parent = systems != null ? systems.transform : null;
                GameObject discoveryObject = new GameObject("AnimalDiscoveryManager");
                if (parent != null)
                    discoveryObject.transform.SetParent(parent);
                discoveryObject.AddComponent<AnimalDiscoveryManager>();
            }

            if (Object.FindAnyObjectByType<PassiveAnimalLocomotionManager>() == null)
            {
                GameObject systems = GameObject.Find("Systems");
                Transform parent = systems != null ? systems.transform : null;
                GameObject locomotionObject = new GameObject("PassiveAnimalLocomotionManager");
                if (parent != null)
                    locomotionObject.transform.SetParent(parent);
                locomotionObject.AddComponent<PassiveAnimalLocomotionManager>();
            }
        }

        static void ResetAllSheepToNeutral()
        {
            SheepResource[] sheep = Object.FindObjectsByType<SheepResource>();
            for (int i = 0; i < sheep.Length; i++)
            {
                if (sheep[i] == null)
                    continue;

                sheep[i].ResetToNeutral();
                EditorUtility.SetDirty(sheep[i]);
            }
        }

        static void RemoveExistingHuntableAnimals()
        {
            DeerResource[] deer = Object.FindObjectsByType<DeerResource>();
            for (int i = 0; i < deer.Length; i++)
                Object.DestroyImmediate(deer[i].gameObject);

            SheepResource[] sheep = Object.FindObjectsByType<SheepResource>();
            for (int i = 0; i < sheep.Length; i++)
                Object.DestroyImmediate(sheep[i].gameObject);
        }

        static bool ShouldBuildFourPlayerFfa() => true;

        static int GetFfaPlayerCount() =>
            Mathf.Clamp(EditorPrefs.GetInt(FfaPlayerCountEditorPrefsKey, 4), 2, 4);

        static void SetFfaPlayerCount(int count)
        {
            int clamped = Mathf.Clamp(count, 2, 4);
            EditorPrefs.SetInt(FfaPlayerCountEditorPrefsKey, clamped);
            Debug.Log($"Phase10 FFA player count → {clamped}. Run AoE → Setup Phase10 Scene to rebuild spawns.");
        }

        [MenuItem("AoE/Match/FFA Player Count/2 Players (Diagonal)")]
        static void SetFfaPlayerCount2() => SetFfaPlayerCount(2);

        [MenuItem("AoE/Match/FFA Player Count/2 Players (Diagonal)", true)]
        static bool ValidateFfaPlayerCount2() => GetFfaPlayerCount() == 2;

        [MenuItem("AoE/Match/FFA Player Count/3 Players")]
        static void SetFfaPlayerCount3() => SetFfaPlayerCount(3);

        [MenuItem("AoE/Match/FFA Player Count/3 Players", true)]
        static bool ValidateFfaPlayerCount3() => GetFfaPlayerCount() == 3;

        [MenuItem("AoE/Match/FFA Player Count/4 Players")]
        static void SetFfaPlayerCount4() => SetFfaPlayerCount(4);

        [MenuItem("AoE/Match/FFA Player Count/4 Players", true)]
        static bool ValidateFfaPlayerCount4() => GetFfaPlayerCount() == 4;

        static GameObject CreateFourPlayerMatch(
            BuildingData townCenterData,
            UnitData villagerData,
            ResourceNodeData treeData,
            FoodNodeData berryBushData,
            FoodNodeData sheepData,
            FoodNodeData boarData,
            MineralNodeData goldMineData,
            MineralNodeData stoneMineData)
        {
            int playerCount = GetFfaPlayerCount();
            int[] cornerIndices = FfaCornerIndicesByPlayerCount[playerCount];
            GameObject playerTownCenter = null;
            for (int i = 0; i < playerCount; i++)
            {
                PlayerId playerId = (PlayerId)i;
                int cornerIndex = cornerIndices[i];
                Vector3 tcPosition = FourPlayerTownCenterPositions[cornerIndex];
                GameObject townCenterObject = Phase1SceneBuilder.CreateTownCenter(townCenterData, tcPosition);
                TownCenter townCenter = townCenterObject.GetComponent<TownCenter>();
                townCenter.SetOwner(playerId);

                if (playerId == PlayerId.Player0)
                    playerTownCenter = townCenterObject;

                Vector3[] villagerPositions = new Vector3[VillagerSpawnOffsets.Length];
                for (int v = 0; v < VillagerSpawnOffsets.Length; v++)
                {
                    villagerPositions[v] = tcPosition
                        + MirrorStartOffset(VillagerSpawnOffsets[v], cornerIndex);
                }

                CreateStartingVillagersForPlayer(villagerData, villagerPositions, playerId);
                CreateAoE2StartResourcesForCorner(
                    cornerIndex,
                    tcPosition,
                    treeData,
                    berryBushData,
                    sheepData,
                    boarData,
                    goldMineData,
                    stoneMineData);
            }

            ResetAllSheepToNeutral();
            return playerTownCenter;
        }

        static Vector3 MirrorStartOffset(Vector3 offset, int cornerIndex)
        {
            float x = offset.x;
            float z = offset.z;
            return cornerIndex switch
            {
                0 => offset,
                1 => new Vector3(-x, offset.y, -z),
                2 => new Vector3(x, offset.y, -z),
                3 => new Vector3(-x, offset.y, z),
                _ => offset
            };
        }

        static void CreateAoE2StartResourcesForCorner(
            int cornerIndex,
            Vector3 tcPosition,
            ResourceNodeData treeData,
            FoodNodeData berryBushData,
            FoodNodeData sheepData,
            FoodNodeData boarData,
            MineralNodeData goldMineData,
            MineralNodeData stoneMineData)
        {
            for (int c = 0; c < AoE2TreeClumpOffsets.Length; c++)
            {
                Vector3[] clump = AoE2TreeClumpOffsets[c];
                for (int t = 0; t < clump.Length; t++)
                {
                    Phase1SceneBuilder.CreateTree(
                        treeData,
                        tcPosition + MirrorStartOffset(clump[t], cornerIndex));
                }
            }

            for (int b = 0; b < AoE2BerryOffsets.Length; b++)
            {
                Phase1SceneBuilder.CreateBerryBush(
                    berryBushData,
                    tcPosition + MirrorStartOffset(AoE2BerryOffsets[b], cornerIndex));
            }

            for (int s = 0; s < AoE2SheepOffsets.Length; s++)
            {
                Phase1SceneBuilder.CreateSheep(
                    sheepData,
                    tcPosition + MirrorStartOffset(AoE2SheepOffsets[s], cornerIndex));
            }

            for (int b = 0; b < AoE2BoarOffsets.Length; b++)
            {
                Phase1SceneBuilder.CreateBoar(
                    boarData,
                    tcPosition + MirrorStartOffset(AoE2BoarOffsets[b], cornerIndex));
            }

            for (int g = 0; g < AoE2GoldMineOffsets.Length; g++)
            {
                Phase1SceneBuilder.CreateGoldMine(
                    goldMineData,
                    tcPosition + MirrorStartOffset(AoE2GoldMineOffsets[g], cornerIndex));
            }

            for (int s = 0; s < AoE2StoneMineOffsets.Length; s++)
            {
                Phase1SceneBuilder.CreateStoneMine(
                    stoneMineData,
                    tcPosition + MirrorStartOffset(AoE2StoneMineOffsets[s], cornerIndex));
            }
        }

        static GameObject CreateCpuTownCenter(BuildingData townCenterData)
        {
            GameObject townCenterObject = Phase1SceneBuilder.CreateTownCenter(townCenterData, CpuTownCenterPosition);
            TownCenter townCenter = townCenterObject.GetComponent<TownCenter>();
            townCenter.SetOwner(PlayerId.Player1);
            EditorUtility.SetDirty(townCenter);
            return townCenterObject;
        }

        static void CreateStartingVillagersForPlayer(UnitData villagerData, Vector3[] positions, PlayerId playerId)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                GameObject unitObject = Phase1SceneBuilder.CreateUnit(
                    villagerData,
                    positions[i],
                    PlayerIdMapping.ToLegacyTeam(playerId));
                Unit unit = unitObject.GetComponent<Unit>();
                if (unit != null)
                    unit.SetOwner(playerId);
            }
        }

        static void CreateStartingVillagers(UnitData villagerData, Vector3[] positions, UnitTeam team)
        {
            CreateStartingVillagersForPlayer(
                villagerData,
                positions,
                PlayerIdMapping.FromLegacyTeam(team));
        }

        [MenuItem("AoE/Sync AoE2 Classic Start (Phase10)", true)]
        static bool ValidateSyncAoE2ClassicStart() => !EditorApplication.isPlaying;

        /// <summary>
        /// 既存 Phase10 シーンを AoE2 Classic 開始（200F/200W、両チーム村民3）に揃える。
        /// </summary>
        [MenuItem("AoE/Sync AoE2 Classic Start (Phase10)")]
        public static void SyncAoE2ClassicStartToOpenScene()
        {
            if (!Phase1SceneBuilder.EnsureEditModeForSceneSetup())
                return;

            UnitData villagerData = Phase1SceneBuilder.EnsureDefaultUnitData();
            ApplyClassicStartingResourcesToScene();
            ApplyPhase42SceneWiring();
            EnsureStartingVillagerCount(villagerData, UnitTeam.Player, PlayerVillagerPositions);
            EnsureStartingVillagerCount(villagerData, UnitTeam.Enemy, CpuVillagerPositions);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Synced AoE2 Classic start: 200 Food + 200 Wood, 3 Villagers per team. Save the scene (Ctrl+S).");
        }

        public static void BatchSyncPhase10SceneClassicStart()
        {
            EditorSceneManager.OpenScene(ScenePath);
            SyncAoE2ClassicStartToOpenScene();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        static void ApplyClassicStartingResourcesToScene()
        {
            ResourceManager resourceManager = Object.FindAnyObjectByType<ResourceManager>();
            if (resourceManager == null)
            {
                Debug.LogWarning("ResourceManager not found — starting resources were not updated.");
                return;
            }

            SerializedObject serialized = new SerializedObject(resourceManager);
            serialized.FindProperty("initialFoodPerPlayer").floatValue = 200f;
            serialized.FindProperty("initialWoodPerPlayer").floatValue = 200f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(resourceManager);
        }

        static void ApplyPhase42SceneWiring()
        {
            AgeData feudalAgeData = Phase1SceneBuilder.EnsureFeudalAgeData();
            CivilizationData playerCivilization = Phase1SceneBuilder.EnsureDefaultPlayerCivilizationData();
            CivilizationData cpuCivilization = Phase1SceneBuilder.EnsureDefaultCpuCivilizationData();

            GameSessionManager sessionManager = Object.FindAnyObjectByType<GameSessionManager>();
            if (sessionManager != null)
            {
                SerializedObject serializedSession = new SerializedObject(sessionManager);
                serializedSession.FindProperty("balanceMode").enumValueIndex = (int)GameplayBalanceMode.Debug;
                serializedSession.FindProperty("cpuAttackPace").enumValueIndex = (int)CpuAttackPace.Relaxed;
                serializedSession.FindProperty("feudalAgeData").objectReferenceValue = feudalAgeData;
                serializedSession.FindProperty("playerCivilization").objectReferenceValue = playerCivilization;
                serializedSession.FindProperty("enemyCivilization").objectReferenceValue = cpuCivilization;
                serializedSession.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(sessionManager);
            }

            ProductionPanelView productionPanel = Object.FindAnyObjectByType<ProductionPanelView>();
            if (productionPanel != null)
            {
                SerializedObject serializedPanel = new SerializedObject(productionPanel);
                serializedPanel.FindProperty("feudalAgeData").objectReferenceValue = feudalAgeData;
                serializedPanel.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(productionPanel);
            }
        }

        static void EnsureStartingVillagerCount(UnitData villagerData, UnitTeam team, Vector3[] spawnPositions)
        {
            RemoveTeamUnitsWithoutData(team);

            Unit[] units = Object.FindObjectsByType<Unit>();
            var villagers = new System.Collections.Generic.List<Unit>(spawnPositions.Length);
            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (unit == null || unit.Team != team || unit.Data == null)
                    continue;

                if (unit.Data.displayName != "Villager")
                    continue;

                villagers.Add(unit);
            }

            for (int i = villagers.Count - 1; i >= spawnPositions.Length; i--)
                Object.DestroyImmediate(villagers[i].gameObject);

            int villagerCount = Mathf.Min(villagers.Count, spawnPositions.Length);
            for (int i = villagerCount; i < spawnPositions.Length; i++)
            {
                GameObject unitObject = Phase1SceneBuilder.CreateUnit(
                    villagerData,
                    spawnPositions[i],
                    team);
                Unit unit = unitObject.GetComponent<Unit>();
                if (unit != null)
                    unit.SetTeam(team);
            }
        }

        static void RemoveTeamUnitsWithoutData(UnitTeam team)
        {
            Unit[] units = Object.FindObjectsByType<Unit>();
            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (unit == null || unit.Team != team || unit.Data != null)
                    continue;

                Object.DestroyImmediate(unit.gameObject);
            }
        }

        static void CreateBerryBushes(FoodNodeData bushData)
        {
            for (int i = 0; i < PlayerBerryBushPositions.Length; i++)
                Phase1SceneBuilder.CreateBerryBush(bushData, PlayerBerryBushPositions[i]);

            for (int i = 0; i < CpuBerryBushPositions.Length; i++)
                Phase1SceneBuilder.CreateBerryBush(bushData, CpuBerryBushPositions[i]);
        }

        static void CreateHuntableAnimals(FoodNodeData deerData, FoodNodeData sheepData)
        {
            for (int i = 0; i < PlayerDeerPositions.Length; i++)
                Phase1SceneBuilder.CreateDeer(deerData, PlayerDeerPositions[i]);

            for (int i = 0; i < PlayerSheepPositions.Length; i++)
                Phase1SceneBuilder.CreateSheep(sheepData, PlayerSheepPositions[i]);

            for (int i = 0; i < CpuDeerPositions.Length; i++)
                Phase1SceneBuilder.CreateDeer(deerData, CpuDeerPositions[i]);

            for (int i = 0; i < CpuSheepPositions.Length; i++)
                Phase1SceneBuilder.CreateSheep(sheepData, CpuSheepPositions[i]);

            ResetAllSheepToNeutral();
        }

        static void CreateSandboxGround(bool fourPlayerFfa)
        {
            GameObject ground = Phase1SceneBuilder.CreateGround();
            if (fourPlayerFfa)
            {
                ground.transform.localScale = FourPlayerGroundScale;
                ground.transform.position = FourPlayerGroundPosition;
            }
            else
            {
                ground.transform.localScale = SandboxGroundScale;
                ground.transform.position = SandboxGroundPosition;
            }
        }

        static void CreateBoars(FoodNodeData boarData)
        {
            for (int i = 0; i < PlayerBoarPositions.Length; i++)
                Phase1SceneBuilder.CreateBoar(boarData, PlayerBoarPositions[i]);

            for (int i = 0; i < CpuBoarPositions.Length; i++)
                Phase1SceneBuilder.CreateBoar(boarData, CpuBoarPositions[i]);
        }

        static void CreateMineralMines(MineralNodeData goldMineData, MineralNodeData stoneMineData)
        {
            for (int i = 0; i < PlayerGoldMinePositions.Length; i++)
                Phase1SceneBuilder.CreateGoldMine(goldMineData, PlayerGoldMinePositions[i]);

            for (int i = 0; i < PlayerStoneMinePositions.Length; i++)
                Phase1SceneBuilder.CreateStoneMine(stoneMineData, PlayerStoneMinePositions[i]);

            for (int i = 0; i < CpuGoldMinePositions.Length; i++)
                Phase1SceneBuilder.CreateGoldMine(goldMineData, CpuGoldMinePositions[i]);

            for (int i = 0; i < CpuStoneMinePositions.Length; i++)
                Phase1SceneBuilder.CreateStoneMine(stoneMineData, CpuStoneMinePositions[i]);
        }

        static void CreateTrees(ResourceNodeData treeData)
        {
            for (int i = 0; i < TreePositions.Length; i++)
                Phase1SceneBuilder.CreateTree(treeData, TreePositions[i]);
        }

        static void CreateManagers(
            InputActionAsset inputActions,
            UnityEngine.Camera mainCamera,
            PlacedBuildingData houseData,
            PlacedBuildingData barracksData,
            PlacedBuildingData archeryRangeData,
            PlacedBuildingData stableData,
            PlacedBuildingData blacksmithData,
            PlacedBuildingData palisadeWallData,
            PlacedBuildingData stoneWallData,
            PlacedBuildingData gateData,
            PlacedBuildingData watchTowerData,
            PlacedBuildingData marketData,
            MarketTradeData marketTradeData,
            PlacedBuildingData townCenterPlacementData,
            PlacedBuildingData millData,
            TechnologyData infantryUpgradeTech,
            UnitData villagerData,
            UnitData militiaData)
        {
            GameObject systems = new GameObject("Systems");
            AgeData feudalAgeData = Phase1SceneBuilder.EnsureFeudalAgeData();
            CivilizationData playerCivilization = Phase1SceneBuilder.EnsureDefaultPlayerCivilizationData();
            CivilizationData cpuCivilization = Phase1SceneBuilder.EnsureDefaultCpuCivilizationData();
            GameSessionManager sessionManager = systems.AddComponent<GameSessionManager>();
            systems.AddComponent<DebugPlaytestInput>();
            SerializedObject serializedSession = new SerializedObject(sessionManager);
            serializedSession.FindProperty("matchMode").enumValueIndex =
                ShouldBuildFourPlayerFfa() ? (int)MatchMode.FourPlayerFfa : (int)MatchMode.OneVsOneCpu;
            serializedSession.FindProperty("ffaPlayerCount").intValue = GetFfaPlayerCount();
            serializedSession.FindProperty("balanceMode").enumValueIndex = (int)GameplayBalanceMode.Debug;
            serializedSession.FindProperty("cpuDifficulty").enumValueIndex = (int)CpuDifficulty.Normal;
            serializedSession.FindProperty("cpuAttackPace").enumValueIndex = (int)CpuAttackPace.Relaxed;
            serializedSession.FindProperty("feudalAgeData").objectReferenceValue = feudalAgeData;
            serializedSession.FindProperty("playerCivilization").objectReferenceValue = playerCivilization;
            serializedSession.FindProperty("enemyCivilization").objectReferenceValue = cpuCivilization;
            serializedSession.ApplyModifiedPropertiesWithoutUndo();

            GameObject simulationTickObject = new GameObject("SimulationTick");
            simulationTickObject.transform.SetParent(systems.transform);
            simulationTickObject.AddComponent<SimulationTick>();

            GameObject commandQueueObject = new GameObject("CommandQueue");
            commandQueueObject.transform.SetParent(systems.transform);
            commandQueueObject.AddComponent<CommandQueue>();

            GameObject cpuAiActionQueueObject = new GameObject("CpuAiActionQueue");
            cpuAiActionQueueObject.transform.SetParent(systems.transform);
            cpuAiActionQueueObject.AddComponent<CpuAiActionQueue>();

            GameObject unitPoolObject = new GameObject("UnitPool");
            unitPoolObject.transform.SetParent(systems.transform);
            UnitPool unitPool = unitPoolObject.AddComponent<UnitPool>();
            SerializedObject serializedUnitPool = new SerializedObject(unitPool);
            serializedUnitPool.FindProperty("prewarmVillagerData").objectReferenceValue = villagerData;
            serializedUnitPool.FindProperty("prewarmMilitiaData").objectReferenceValue = militiaData;
            serializedUnitPool.ApplyModifiedPropertiesWithoutUndo();

            GameObject buildingPoolObject = new GameObject("BuildingPool");
            buildingPoolObject.transform.SetParent(systems.transform);
            buildingPoolObject.AddComponent<BuildingPool>();

            GameObject unitManagerObject = new GameObject("UnitManager");
            unitManagerObject.transform.SetParent(systems.transform);
            unitManagerObject.AddComponent<UnitManager>();

            GameObject unitSpatialIndexObject = new GameObject("UnitSpatialIndex");
            unitSpatialIndexObject.transform.SetParent(systems.transform);
            unitSpatialIndexObject.AddComponent<UnitSpatialIndex>();

            GameObject treeSpatialIndexObject = new GameObject("TreeSpatialIndex");
            treeSpatialIndexObject.transform.SetParent(systems.transform);
            treeSpatialIndexObject.AddComponent<TreeSpatialIndex>();

            GameObject berrySpatialIndexObject = new GameObject("BerryBushSpatialIndex");
            berrySpatialIndexObject.transform.SetParent(systems.transform);
            berrySpatialIndexObject.AddComponent<BerryBushSpatialIndex>();

            GameObject attackManagerObject = new GameObject("AttackManager");
            attackManagerObject.transform.SetParent(systems.transform);
            attackManagerObject.AddComponent<AttackManager>();

            GameObject unitAggroManagerObject = new GameObject("UnitAggroManager");
            unitAggroManagerObject.transform.SetParent(systems.transform);
            unitAggroManagerObject.AddComponent<UnitAggroManager>();

            GameObject attackMoveManagerObject = new GameObject("AttackMoveManager");
            attackMoveManagerObject.transform.SetParent(systems.transform);
            attackMoveManagerObject.AddComponent<AttackMoveManager>();

            GameObject formationMoveManagerObject = new GameObject("FormationMoveManager");
            formationMoveManagerObject.transform.SetParent(systems.transform);
            formationMoveManagerObject.AddComponent<FormationMoveManager>();

            GameObject boarAggroManagerObject = new GameObject("BoarAggroManager");
            boarAggroManagerObject.transform.SetParent(systems.transform);
            boarAggroManagerObject.AddComponent<BoarAggroManager>();

            GameObject boarAttackManagerObject = new GameObject("BoarAttackManager");
            boarAttackManagerObject.transform.SetParent(systems.transform);
            boarAttackManagerObject.AddComponent<BoarAttackManager>();

            GameObject animalDiscoveryManagerObject = new GameObject("AnimalDiscoveryManager");
            animalDiscoveryManagerObject.transform.SetParent(systems.transform);
            animalDiscoveryManagerObject.AddComponent<AnimalDiscoveryManager>();

            GameObject passiveAnimalLocomotionManagerObject = new GameObject("PassiveAnimalLocomotionManager");
            passiveAnimalLocomotionManagerObject.transform.SetParent(systems.transform);
            passiveAnimalLocomotionManagerObject.AddComponent<PassiveAnimalLocomotionManager>();

            GameObject populationManagerObject = new GameObject("PopulationManager");
            populationManagerObject.transform.SetParent(systems.transform);
            populationManagerObject.AddComponent<PopulationManager>();

            GameObject productionManagerObject = new GameObject("ProductionManager");
            productionManagerObject.transform.SetParent(systems.transform);
            productionManagerObject.AddComponent<ProductionManager>();

            GameObject barracksProductionObject = new GameObject("BarracksProductionManager");
            barracksProductionObject.transform.SetParent(systems.transform);
            barracksProductionObject.AddComponent<BarracksProductionManager>();

            GameObject archeryRangeProductionObject = new GameObject("ArcheryRangeProductionManager");
            archeryRangeProductionObject.transform.SetParent(systems.transform);
            archeryRangeProductionObject.AddComponent<ArcheryRangeProductionManager>();

            GameObject stableProductionObject = new GameObject("StableProductionManager");
            stableProductionObject.transform.SetParent(systems.transform);
            stableProductionObject.AddComponent<StableProductionManager>();

            GameObject blacksmithResearchObject = new GameObject("BlacksmithResearchManager");
            blacksmithResearchObject.transform.SetParent(systems.transform);
            blacksmithResearchObject.AddComponent<BlacksmithResearchManager>();

            GameObject watchTowerDefenseObject = new GameObject("WatchTowerDefenseManager");
            watchTowerDefenseObject.transform.SetParent(systems.transform);
            watchTowerDefenseObject.AddComponent<WatchTowerDefenseManager>();

            GameObject resourceManagerObject = new GameObject("ResourceManager");
            resourceManagerObject.transform.SetParent(systems.transform);
            ResourceManager resourceManager = resourceManagerObject.AddComponent<ResourceManager>();
            SerializedObject serializedResourceManager = new SerializedObject(resourceManager);
            serializedResourceManager.FindProperty("initialFoodPerPlayer").floatValue = 200f;
            serializedResourceManager.FindProperty("initialWoodPerPlayer").floatValue = 200f;
            serializedResourceManager.FindProperty("initialGoldPerPlayer").floatValue = 100f;
            serializedResourceManager.ApplyModifiedPropertiesWithoutUndo();

            GameObject gatherManagerObject = new GameObject("GatherManager");
            gatherManagerObject.transform.SetParent(systems.transform);
            gatherManagerObject.AddComponent<GatherManager>();

            GameObject foodGatherManagerObject = new GameObject("FoodGatherManager");
            foodGatherManagerObject.transform.SetParent(systems.transform);
            foodGatherManagerObject.AddComponent<FoodGatherManager>();

            GameObject mineralGatherManagerObject = new GameObject("MineralGatherManager");
            mineralGatherManagerObject.transform.SetParent(systems.transform);
            mineralGatherManagerObject.AddComponent<MineralGatherManager>();

            GameObject placementManagerObject = new GameObject("BuildingPlacementManager");
            placementManagerObject.transform.SetParent(systems.transform);
            BuildingPlacementManager placementManager = placementManagerObject.AddComponent<BuildingPlacementManager>();

            EnsureGameplayCanvas();
            EnsureHudUi();
            PlacementPreviewView placementPreviewView = EnsurePlacementPreviewView();
            placementPreviewView.transform.SetParent(systems.transform);

            CombatFeedbackView.Ensure(systems);

            CpuEconomyAiManager cpuEconomy = null;
            CpuMilitaryAiManager cpuMilitary = null;
            if (ShouldBuildFourPlayerFfa())
            {
                int ffaPlayerCount = GetFfaPlayerCount();
                for (int cpu = 1; cpu < ffaPlayerCount; cpu++)
                {
                    PlayerId cpuId = (PlayerId)cpu;
                    if (cpuEconomy == null)
                        cpuEconomy = CreateCpuEconomyAi(systems.transform, cpuId, houseData, millData);
                    else
                        CreateCpuEconomyAi(systems.transform, cpuId, houseData, millData);

                    if (cpuMilitary == null)
                        cpuMilitary = CreateCpuMilitaryAi(systems.transform, cpuId, barracksData, archeryRangeData, stableData);
                    else
                        CreateCpuMilitaryAi(systems.transform, cpuId, barracksData, archeryRangeData, stableData);
                }
            }
            else
            {
                cpuEconomy = CreateCpuEconomyAi(systems.transform, PlayerId.Player1, houseData, millData);
                cpuMilitary = CreateCpuMilitaryAi(systems.transform, PlayerId.Player1, barracksData, archeryRangeData, stableData);
            }

            GameObject selectionManagerObject = new GameObject("SelectionManager");
            selectionManagerObject.transform.SetParent(systems.transform);
            SelectionManager selectionManager = selectionManagerObject.AddComponent<SelectionManager>();
            selectionManagerObject.AddComponent<SelectionBoxView>();
            selectionManagerObject.AddComponent<ProductionPanelView>();
            selectionManagerObject.AddComponent<BarracksPanelView>();
            selectionManagerObject.AddComponent<ArcheryRangePanelView>();
            selectionManagerObject.AddComponent<StablePanelView>();
            selectionManagerObject.AddComponent<BlacksmithPanelView>();
            selectionManagerObject.AddComponent<MarketPanelView>();
            selectionManagerObject.AddComponent<UnitStancePanelView>();
            UnitHpBarView hpBarView = selectionManagerObject.AddComponent<UnitHpBarView>();
            SelectionInfoPanelView infoPanelView = selectionManagerObject.AddComponent<SelectionInfoPanelView>();
            ResourceHudView resourceHud = selectionManagerObject.AddComponent<ResourceHudView>();
            IdleUnitSelectionController idleSelection = selectionManagerObject.AddComponent<IdleUnitSelectionController>();
            IdleUnitHudView idleHud = selectionManagerObject.AddComponent<IdleUnitHudView>();
            ControlGroupManager controlGroupManager = selectionManagerObject.AddComponent<ControlGroupManager>();
            ControlGroupInputController controlGroupInput = selectionManagerObject.AddComponent<ControlGroupInputController>();
            selectionManagerObject.AddComponent<CpuHudView>();
            selectionManagerObject.AddComponent<GameTimeHudView>();
            selectionManagerObject.AddComponent<VictoryDefeatHudView>();
            selectionManagerObject.AddComponent<MinimapView>();

            RTSInputReader inputReader = mainCamera.GetComponent<RTSInputReader>();
            SerializedObject serializedSelection = new SerializedObject(selectionManager);
            serializedSelection.FindProperty("mainCamera").objectReferenceValue = mainCamera;
            serializedSelection.FindProperty("input").objectReferenceValue = inputReader;
            SerializedProperty boxView = serializedSelection.FindProperty("selectionBoxView");
            boxView.objectReferenceValue = selectionManagerObject.GetComponent<SelectionBoxView>();
            serializedSelection.ApplyModifiedPropertiesWithoutUndo();

            ProductionPanelView productionPanel = selectionManagerObject.GetComponent<ProductionPanelView>();
            SerializedObject serializedProductionPanel = new SerializedObject(productionPanel);
            serializedProductionPanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedProductionPanel.FindProperty("input").objectReferenceValue = inputReader;
            serializedProductionPanel.FindProperty("feudalAgeData").objectReferenceValue = feudalAgeData;
            serializedProductionPanel.ApplyModifiedPropertiesWithoutUndo();

            BarracksPanelView barracksPanel = selectionManagerObject.GetComponent<BarracksPanelView>();
            SerializedObject serializedBarracksPanel = new SerializedObject(barracksPanel);
            serializedBarracksPanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedBarracksPanel.FindProperty("input").objectReferenceValue = inputReader;
            serializedBarracksPanel.ApplyModifiedPropertiesWithoutUndo();

            ArcheryRangePanelView archeryRangePanel = selectionManagerObject.GetComponent<ArcheryRangePanelView>();
            SerializedObject serializedArcheryRangePanel = new SerializedObject(archeryRangePanel);
            serializedArcheryRangePanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedArcheryRangePanel.FindProperty("input").objectReferenceValue = inputReader;
            serializedArcheryRangePanel.ApplyModifiedPropertiesWithoutUndo();

            StablePanelView stablePanel = selectionManagerObject.GetComponent<StablePanelView>();
            SerializedObject serializedStablePanel = new SerializedObject(stablePanel);
            serializedStablePanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedStablePanel.FindProperty("input").objectReferenceValue = inputReader;
            serializedStablePanel.ApplyModifiedPropertiesWithoutUndo();

            BlacksmithPanelView blacksmithPanel = selectionManagerObject.GetComponent<BlacksmithPanelView>();
            SerializedObject serializedBlacksmithPanel = new SerializedObject(blacksmithPanel);
            serializedBlacksmithPanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedBlacksmithPanel.FindProperty("input").objectReferenceValue = inputReader;
            serializedBlacksmithPanel.FindProperty("infantryUpgradeTech").objectReferenceValue = infantryUpgradeTech;
            serializedBlacksmithPanel.ApplyModifiedPropertiesWithoutUndo();

            MarketPanelView marketPanel = selectionManagerObject.GetComponent<MarketPanelView>();
            SerializedObject serializedMarketPanel = new SerializedObject(marketPanel);
            serializedMarketPanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedMarketPanel.FindProperty("tradeRates").objectReferenceValue = marketTradeData;
            serializedMarketPanel.ApplyModifiedPropertiesWithoutUndo();

            UnitStancePanelView stancePanel = selectionManagerObject.GetComponent<UnitStancePanelView>();
            SerializedObject serializedStancePanel = new SerializedObject(stancePanel);
            serializedStancePanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedStancePanel.ApplyModifiedPropertiesWithoutUndo();

            RTSCameraController cameraController = mainCamera.GetComponent<RTSCameraController>();
            SerializedObject serializedIdleSelection = new SerializedObject(idleSelection);
            serializedIdleSelection.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedIdleSelection.FindProperty("input").objectReferenceValue = inputReader;
            serializedIdleSelection.FindProperty("cameraController").objectReferenceValue = cameraController;
            serializedIdleSelection.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedIdleHud = new SerializedObject(idleHud);
            serializedIdleHud.FindProperty("idleSelectionController").objectReferenceValue = idleSelection;
            serializedIdleHud.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedControlGroup = new SerializedObject(controlGroupManager);
            serializedControlGroup.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedControlGroup.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedControlGroupInput = new SerializedObject(controlGroupInput);
            serializedControlGroupInput.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedControlGroupInput.FindProperty("controlGroupManager").objectReferenceValue = controlGroupManager;
            serializedControlGroupInput.FindProperty("input").objectReferenceValue = inputReader;
            serializedControlGroupInput.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedHpBar = new SerializedObject(hpBarView);
            serializedHpBar.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedHpBar.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedInfoPanel = new SerializedObject(infoPanelView);
            serializedInfoPanel.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedInfoPanel.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedPlacement = new SerializedObject(placementManager);
            serializedPlacement.FindProperty("mainCamera").objectReferenceValue = mainCamera;
            serializedPlacement.FindProperty("input").objectReferenceValue = inputReader;
            serializedPlacement.FindProperty("placementPreviewViewHost").objectReferenceValue = placementPreviewView;
            serializedPlacement.FindProperty("houseData").objectReferenceValue = houseData;
            serializedPlacement.FindProperty("barracksData").objectReferenceValue = barracksData;
            serializedPlacement.FindProperty("archeryRangeData").objectReferenceValue = archeryRangeData;
            serializedPlacement.FindProperty("stableData").objectReferenceValue = stableData;
            serializedPlacement.FindProperty("blacksmithData").objectReferenceValue = blacksmithData;
            serializedPlacement.FindProperty("palisadeWallData").objectReferenceValue = palisadeWallData;
            serializedPlacement.FindProperty("stoneWallData").objectReferenceValue = stoneWallData;
            serializedPlacement.FindProperty("gateData").objectReferenceValue = gateData;
            serializedPlacement.FindProperty("watchTowerData").objectReferenceValue = watchTowerData;
            serializedPlacement.FindProperty("marketData").objectReferenceValue = marketData;
            serializedPlacement.FindProperty("townCenterPlacementData").objectReferenceValue = townCenterPlacementData;
            serializedPlacement.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedResourceHud = new SerializedObject(resourceHud);
            serializedResourceHud.FindProperty("selectionManager").objectReferenceValue = selectionManager;
            serializedResourceHud.FindProperty("houseData").objectReferenceValue = houseData;
            serializedResourceHud.FindProperty("barracksData").objectReferenceValue = barracksData;
            serializedResourceHud.FindProperty("archeryRangeData").objectReferenceValue = archeryRangeData;
            serializedResourceHud.FindProperty("stableData").objectReferenceValue = stableData;
            serializedResourceHud.FindProperty("blacksmithData").objectReferenceValue = blacksmithData;
            serializedResourceHud.FindProperty("palisadeWallData").objectReferenceValue = palisadeWallData;
            serializedResourceHud.FindProperty("stoneWallData").objectReferenceValue = stoneWallData;
            serializedResourceHud.FindProperty("gateData").objectReferenceValue = gateData;
            serializedResourceHud.FindProperty("watchTowerData").objectReferenceValue = watchTowerData;
            serializedResourceHud.FindProperty("marketData").objectReferenceValue = marketData;
            serializedResourceHud.FindProperty("townCenterPlacementData").objectReferenceValue = townCenterPlacementData;
            serializedResourceHud.FindProperty("millData").objectReferenceValue = millData;
            serializedResourceHud.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(resourceHud);
            EditorUtility.SetDirty(placementManager);
            if (cpuEconomy != null)
                EditorUtility.SetDirty(cpuEconomy);
            if (cpuMilitary != null)
                EditorUtility.SetDirty(cpuMilitary);
        }

        static CpuEconomyAiManager CreateCpuEconomyAi(
            Transform parent,
            PlayerId playerId,
            PlacedBuildingData houseData,
            PlacedBuildingData millData)
        {
            GameObject cpuEconomyObject = new GameObject($"CpuEconomyAi_{playerId}");
            cpuEconomyObject.transform.SetParent(parent);
            CpuEconomyAiManager cpuEconomy = cpuEconomyObject.AddComponent<CpuEconomyAiManager>();
            PlacedBuildingData miningCampData = Phase1SceneBuilder.EnsureMiningCampData();
            PlacedBuildingData farmData = Phase1SceneBuilder.EnsureFarmData();
            SerializedObject serializedCpuEconomy = new SerializedObject(cpuEconomy);
            serializedCpuEconomy.FindProperty("cpuPlayerId").enumValueIndex = (int)playerId;
            serializedCpuEconomy.FindProperty("houseData").objectReferenceValue = houseData;
            serializedCpuEconomy.FindProperty("millData").objectReferenceValue = millData;
            serializedCpuEconomy.FindProperty("miningCampData").objectReferenceValue = miningCampData;
            serializedCpuEconomy.FindProperty("farmData").objectReferenceValue = farmData;
            serializedCpuEconomy.ApplyModifiedPropertiesWithoutUndo();
            return cpuEconomy;
        }

        static CpuMilitaryAiManager CreateCpuMilitaryAi(
            Transform parent,
            PlayerId playerId,
            PlacedBuildingData barracksData,
            PlacedBuildingData archeryRangeData,
            PlacedBuildingData stableData)
        {
            GameObject cpuMilitaryObject = new GameObject($"CpuMilitaryAi_{playerId}");
            cpuMilitaryObject.transform.SetParent(parent);
            CpuMilitaryAiManager cpuMilitary = cpuMilitaryObject.AddComponent<CpuMilitaryAiManager>();
            SerializedObject serializedCpuMilitary = new SerializedObject(cpuMilitary);
            serializedCpuMilitary.FindProperty("cpuPlayerId").enumValueIndex = (int)playerId;
            serializedCpuMilitary.FindProperty("opponentPlayerId").enumValueIndex = (int)PlayerId.Player0;
            serializedCpuMilitary.FindProperty("barracksData").objectReferenceValue = barracksData;
            serializedCpuMilitary.FindProperty("archeryRangeData").objectReferenceValue = archeryRangeData;
            serializedCpuMilitary.FindProperty("stableData").objectReferenceValue = stableData;
            serializedCpuMilitary.FindProperty("barracksBuildDelaySeconds").floatValue = DefaultBarracksBuildDelaySeconds;
            serializedCpuMilitary.FindProperty("attackWaveIntervalSeconds").floatValue = DefaultAttackWaveIntervalSeconds;
            serializedCpuMilitary.FindProperty("relaxedFirstAttackGraceSeconds").floatValue = DefaultRelaxedFirstAttackGraceSeconds;
            serializedCpuMilitary.FindProperty("relaxedBarracksBuildDelaySeconds").floatValue = DefaultRelaxedBarracksBuildDelaySeconds;
            serializedCpuMilitary.FindProperty("relaxedAttackWaveIntervalSeconds").floatValue = DefaultRelaxedAttackWaveIntervalSeconds;
            serializedCpuMilitary.ApplyModifiedPropertiesWithoutUndo();
            return cpuMilitary;
        }
    }
}
