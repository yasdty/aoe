using System.Collections.Generic;
using AoE.RTS.Camera;
using AoE.RTS.Core;
using AoE.RTS.Input;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public class IdleUnitSelectionController : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;
        [SerializeField] RTSInputReader input;
        [SerializeField] RTSCameraController cameraController;

        readonly List<Unit> idleBuffer = new List<Unit>(16);
        int lastIdleVillagerIndex = -1;
        int lastIdleMilitaryIndex = -1;

        void Awake()
        {
            if (selectionManager == null)
                selectionManager = GetComponent<SelectionManager>();
            if (cameraController == null)
                cameraController = FindAnyObjectByType<RTSCameraController>();
        }

        void Update()
        {
            if (GameSessionManager.IsGameOver || input == null || selectionManager == null)
                return;

            if (input.WasSelectNextIdleVillagerPressedThisFrame())
            {
                if (input.IsShiftHeld)
                    SelectAllIdleVillagers();
                else
                    SelectNextIdleVillager();
                return;
            }

            if (input.WasSelectNextIdleMilitaryPressedThisFrame())
                SelectNextIdleMilitary();
        }

        public void SelectNextIdleVillager()
        {
            UnitIdleTracker.CopyIdleVillagersTo(idleBuffer, UnitTeam.Player);
            if (idleBuffer.Count == 0)
            {
                lastIdleVillagerIndex = -1;
                return;
            }

            lastIdleVillagerIndex = (lastIdleVillagerIndex + 1) % idleBuffer.Count;
            Unit unit = idleBuffer[lastIdleVillagerIndex];
            selectionManager.SelectSingleUnit(unit);
            FocusCameraOnUnit(unit);
        }

        public void SelectAllIdleVillagers()
        {
            UnitIdleTracker.CopyIdleVillagersTo(idleBuffer, UnitTeam.Player);
            if (idleBuffer.Count == 0)
                return;

            lastIdleVillagerIndex = idleBuffer.Count - 1;
            selectionManager.SelectUnits(idleBuffer);
            FocusCameraOnUnit(idleBuffer[0]);
        }

        public void SelectNextIdleMilitary()
        {
            UnitIdleTracker.CopyIdleMilitaryTo(idleBuffer, UnitTeam.Player);
            if (idleBuffer.Count == 0)
            {
                lastIdleMilitaryIndex = -1;
                return;
            }

            lastIdleMilitaryIndex = (lastIdleMilitaryIndex + 1) % idleBuffer.Count;
            Unit unit = idleBuffer[lastIdleMilitaryIndex];
            selectionManager.SelectSingleUnit(unit);
            FocusCameraOnUnit(unit);
        }

        void FocusCameraOnUnit(Unit unit)
        {
            if (cameraController == null || unit == null)
                return;

            cameraController.ApplyOverviewView(unit.transform.position);
        }
    }
}
