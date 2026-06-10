using AoE.RTS.AI;
using AoE.RTS.Buildings;
using AoE.RTS.Selection;
using AoE.RTS.Units;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AoE.RTS.Core
{
    public class DebugPlaytestInput : MonoBehaviour
    {
        const float TownCenterDebugDamage = 150f;

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
                    DamageSelectedTownCenter();
            }
        }

        static void DamageSelectedTownCenter()
        {
            SelectionManager selectionManager = FindAnyObjectByType<SelectionManager>();
            TownCenter townCenter = selectionManager != null ? selectionManager.SelectedTownCenter : null;
            if (townCenter == null || townCenter.Team != UnitTeam.Player)
            {
                Debug.Log("[Debug] Select your Town Center, then press K to deal test damage.");
                return;
            }

            BuildingHealth health = townCenter.GetComponent<BuildingHealth>();
            if (health == null || !health.IsAlive)
                return;

            health.TakeDamage(TownCenterDebugDamage);
            Debug.Log(
                $"[Debug] Town Center damaged ({Mathf.CeilToInt(health.CurrentHp)}/{Mathf.CeilToInt(health.MaxHp)} HP remaining)");
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
