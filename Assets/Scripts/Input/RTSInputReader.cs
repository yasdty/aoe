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
