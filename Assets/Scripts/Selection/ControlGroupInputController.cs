using AoE.RTS.Core;
using AoE.RTS.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AoE.RTS.Selection
{
    public class ControlGroupInputController : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;
        [SerializeField] ControlGroupManager controlGroupManager;
        [SerializeField] RTSInputReader input;

        static readonly Key[] DigitKeys =
        {
            Key.Digit1,
            Key.Digit2,
            Key.Digit3,
            Key.Digit4,
            Key.Digit5,
            Key.Digit6,
            Key.Digit7,
            Key.Digit8,
            Key.Digit9
        };

        void Awake()
        {
            if (selectionManager == null)
                selectionManager = GetComponent<SelectionManager>();
            if (controlGroupManager == null)
                controlGroupManager = GetComponent<ControlGroupManager>();
        }

        void Update()
        {
            if (GameSessionManager.IsGameOver
                || selectionManager == null
                || controlGroupManager == null)
                return;

            int slotIndex = GetPressedDigitSlotIndex();
            if (slotIndex < 0)
                return;

            if (IsCtrlHeld())
            {
                controlGroupManager.SaveGroup(slotIndex, selectionManager.SelectedUnits);
                return;
            }

            if (input != null && input.IsShiftHeld)
                controlGroupManager.RecallGroup(slotIndex, additive: true);
            else
                controlGroupManager.RecallGroup(slotIndex, additive: false);
        }

        static int GetPressedDigitSlotIndex()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return -1;

            for (int i = 0; i < DigitKeys.Length; i++)
            {
                if (keyboard[DigitKeys[i]].wasPressedThisFrame)
                    return i;
            }

            return -1;
        }

        static bool IsCtrlHeld()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return false;

            return keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
        }
    }
}
