using AoE.RTS.Core;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AoE.RTS.Input
{
    public class RTSInputReader : MonoBehaviour
    {
        [SerializeField] InputActionAsset inputActions;

        bool ownsRuntimeInputActions;
        InputAction selectAction;
        InputAction commandAction;
        InputAction moveCameraAction;
        InputAction zoomAction;
        InputAction pointerAction;
        InputAction trainVillagerAction;
        InputAction trainSecondaryAction;
        InputAction selectNextIdleVillagerAction;
        InputAction selectNextIdleMilitaryAction;
        InputAction attackMoveAction;

        public Vector2 CameraMove => moveCameraAction?.ReadValue<Vector2>() ?? Vector2.zero;
        public float ZoomDelta => zoomAction?.ReadValue<float>() ?? 0f;
        public Vector2 PointerScreenPosition => pointerAction?.ReadValue<Vector2>() ?? Vector2.zero;

        void Awake()
        {
            if (inputActions == null)
                inputActions = TryResolveInputActions();

            if (inputActions == null)
            {
                inputActions = RTSInputActionsBuilder.Build();
                ownsRuntimeInputActions = true;
#if UNITY_EDITOR
                Debug.LogWarning(
                    "RTSInputReader: Using runtime-built InputActionAsset. " +
                    "Run AoE → Fix Phase1 Input References to create Assets/Input/RTSInputActions.inputactions.");
#endif
            }

            InputActionMap map = inputActions.FindActionMap("Gameplay", true);
            selectAction = map.FindAction("Select", true);
            commandAction = map.FindAction("Command", true);
            moveCameraAction = map.FindAction("MoveCamera", true);
            zoomAction = map.FindAction("Zoom", true);
            pointerAction = map.FindAction("PointerPosition", true);
            trainVillagerAction = map.FindAction("TrainVillager", false);
            trainSecondaryAction = map.FindAction("TrainSecondary", false);
            selectNextIdleVillagerAction = map.FindAction("SelectNextIdleVillager", false);
            selectNextIdleMilitaryAction = map.FindAction("SelectNextIdleMilitary", false);
            attackMoveAction = map.FindAction("AttackMove", false);
        }

        void OnEnable()
        {
            inputActions?.Enable();
        }

        void OnDisable()
        {
            inputActions?.Disable();
        }

        void OnDestroy()
        {
            if (!ownsRuntimeInputActions || inputActions == null)
                return;

            if (Application.isPlaying)
                Destroy(inputActions);
            else
                DestroyImmediate(inputActions);
        }

        public bool IsSelectPressed => selectAction != null && selectAction.IsPressed();

        public bool WasSelectPressedThisFrame()
        {
            return selectAction != null && selectAction.WasPressedThisFrame();
        }

        public bool WasSelectReleasedThisFrame()
        {
            return selectAction != null && selectAction.WasReleasedThisFrame();
        }

        public bool WasCommandPressedThisFrame()
        {
            return commandAction != null && commandAction.WasPressedThisFrame();
        }

        public bool IsShiftHeld =>
            Keyboard.current != null &&
            (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);

        public bool WasTrainVillagerPressedThisFrame()
        {
            return trainVillagerAction != null && trainVillagerAction.WasPressedThisFrame();
        }

        public bool WasTrainSecondaryPressedThisFrame()
        {
            return trainSecondaryAction != null && trainSecondaryAction.WasPressedThisFrame();
        }

        public bool WasSelectNextIdleVillagerPressedThisFrame()
        {
            return selectNextIdleVillagerAction != null && selectNextIdleVillagerAction.WasPressedThisFrame();
        }

        public bool WasSelectNextIdleMilitaryPressedThisFrame()
        {
            return selectNextIdleMilitaryAction != null && selectNextIdleMilitaryAction.WasPressedThisFrame();
        }

        public bool WasAttackMoveModifierHeld()
        {
            if (attackMoveAction != null && attackMoveAction.IsPressed())
                return true;

            return Keyboard.current != null && Keyboard.current.aKey.isPressed;
        }

        static InputActionAsset TryResolveInputActions()
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<InputActionAsset>(GameAssetPaths.RTSInputActions);
#else
            return null;
#endif
        }
    }
}
