using UnityEngine;
using UnityEngine.InputSystem;

namespace AoE.RTS.Input
{
    /// <summary>
    /// Input System API で RTS 用 InputActionAsset を構築する（.inputactions 手書き・JSON 直書き禁止）。
    /// Editor の RTSInputActionsFactory とランタイムフォールバックの共通実装。
    /// </summary>
    public static class RTSInputActionsBuilder
    {
        public static InputActionAsset Build()
        {
            InputActionAsset asset = ScriptableObject.CreateInstance<InputActionAsset>();
            asset.name = "RTSInputActions";

            InputActionMap map = asset.AddActionMap("Gameplay");

            map.AddAction("Select", InputActionType.Button)
                .AddBinding("<Mouse>/leftButton", groups: "Keyboard&Mouse");

            map.AddAction("Command", InputActionType.Button)
                .AddBinding("<Mouse>/rightButton", groups: "Keyboard&Mouse");

            InputAction moveCamera = map.AddAction("MoveCamera", InputActionType.Value, expectedControlLayout: "Vector2");
            moveCamera.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w", groups: "Keyboard&Mouse")
                .With("Down", "<Keyboard>/s", groups: "Keyboard&Mouse")
                .With("Left", "<Keyboard>/a", groups: "Keyboard&Mouse")
                .With("Right", "<Keyboard>/d", groups: "Keyboard&Mouse");

            map.AddAction("Zoom", InputActionType.Value, expectedControlLayout: "Axis")
                .AddBinding("<Mouse>/scroll/y", groups: "Keyboard&Mouse");

            map.AddAction("PointerPosition", InputActionType.Value, expectedControlLayout: "Vector2")
                .AddBinding("<Mouse>/position", groups: "Keyboard&Mouse");

            map.AddAction("TrainVillager", InputActionType.Button)
                .AddBinding("<Keyboard>/q", groups: "Keyboard&Mouse");

            map.AddAction("TrainSecondary", InputActionType.Button)
                .AddBinding("<Keyboard>/e", groups: "Keyboard&Mouse");

            map.AddAction("SelectNextIdleVillager", InputActionType.Button)
                .AddBinding("<Keyboard>/period", groups: "Keyboard&Mouse");

            map.AddAction("SelectNextIdleMilitary", InputActionType.Button)
                .AddBinding("<Keyboard>/comma", groups: "Keyboard&Mouse");

            asset.AddControlScheme("Keyboard&Mouse")
                .WithBindingGroup("Keyboard&Mouse")
                .WithRequiredDevice<Keyboard>()
                .WithRequiredDevice<Mouse>();

            return asset;
        }
    }
}
