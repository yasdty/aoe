using AoE.RTS.AI;
using AoE.RTS.Buildings;
using AoE.RTS.Core;
using AoE.RTS.Selection;
using AoE.RTS.Units;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AoE.RTS.Core
{
    public class DebugPlaytestInput : MonoBehaviour
    {
        const float BuildingDebugDamage = 150f;

        void Update()
        {
            if (!Application.isPlaying || GameSessionManager.IsGameOver)
                return;

            if (GameplayBalance.Mode != GameplayBalanceMode.Debug)
                return;

            if (Keyboard.current == null)
                return;

            if (Keyboard.current.kKey.wasPressedThisFrame)
            {
                if (Keyboard.current.shiftKey.isPressed)
                    TriggerCpuAttackWave();
                else
                    DamageSelectedPlayerBuilding();
            }
        }

        static void DamageSelectedPlayerBuilding()
        {
            SelectionManager selectionManager = FindAnyObjectByType<SelectionManager>();
            if (selectionManager == null)
            {
                Debug.Log("[Debug] SelectionManager not found.");
                return;
            }

            TownCenter townCenter = selectionManager.SelectedTownCenter;
            if (townCenter != null && townCenter.Team == UnitTeam.Player)
            {
                ApplyDebugDamage(townCenter.GetComponent<BuildingHealth>(), "Town Center");
                return;
            }

            BuildingHealth placedBuilding = selectionManager.SelectedPlacedBuilding;
            if (placedBuilding != null && placedBuilding.Team == UnitTeam.Player)
            {
                ApplyDebugDamage(placedBuilding, GetBuildingDebugLabel(placedBuilding));
                return;
            }

            Debug.Log(
                "[Debug] Select your Town Center or a placed building (House, Barracks, etc.), then press K.");
        }

        static string GetBuildingDebugLabel(BuildingHealth building)
        {
            if (building == null)
                return "Building";

            if (building.GetComponent<House>() != null)
                return "House";

            if (building.GetComponent<Barracks>() != null)
                return "Barracks";

            if (building.GetComponent<ArcheryRange>() != null)
                return "Archery Range";

            if (building.GetComponent<Stable>() != null)
                return "Stable";

            if (building.GetComponent<Farm>() != null)
                return "Farm";

            if (building.GetComponent<LumberCamp>() != null)
                return "Lumber Camp";

            if (building.GetComponent<MiningCamp>() != null)
                return "Mining Camp";

            if (building.GetComponent<Mill>() != null)
                return "Mill";

            if (building.GetComponent<Gate>() != null)
                return "Gate";

            if (building.GetComponent<PalisadeWall>() != null)
                return "Palisade Wall";

            if (building.GetComponent<StoneWall>() != null)
                return "Stone Wall";

            return "Building";
        }

        static void ApplyDebugDamage(BuildingHealth health, string label)
        {
            if (health == null || !health.IsAlive)
                return;

            health.TakeDamage(BuildingDebugDamage);
            Debug.Log(
                $"[Debug] {label} damaged ({Mathf.CeilToInt(health.CurrentHp)}/{Mathf.CeilToInt(health.MaxHp)} HP remaining)");
        }

        static void TriggerCpuAttackWave()
        {
            CpuMilitaryAiManager military = CpuMilitaryAiManager.Instance;
            if (military == null)
            {
                Debug.LogWarning("[Debug] CpuMilitaryAiManager not found.");
                return;
            }

            military.ForceDebugAttackWave();
            Debug.Log("[Debug] Forced CPU attack wave (Shift+K). CPU targets nearest player unit first.");
        }
    }
}
