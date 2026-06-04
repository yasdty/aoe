using AoE.RTS.Input;
using UnityEngine;

namespace AoE.RTS.Camera
{
    public class RTSCameraController : MonoBehaviour
    {
        [SerializeField] RTSInputReader input;
        [SerializeField] float moveSpeed = 25f;
        [SerializeField] float edgeScrollSpeed = 25f;
        [SerializeField] float edgeScrollBorderPixels = 12f;
        [SerializeField] float zoomSpeed = 25f;
        [SerializeField] float minHeight = 12f;
        [SerializeField] float maxHeight = 80f;
        [SerializeField] bool applyStartViewOnLoad = true;
        [SerializeField] Vector3 startFocusPoint;
        [SerializeField] float startHeight = 45f;
        [SerializeField] float startPitch = 55f;
        [SerializeField] float startYaw = -45f;

        void Start()
        {
            if (applyStartViewOnLoad)
                ApplyOverviewView(startFocusPoint);
        }

        public void ApplyOverviewView(Vector3 focusPoint)
        {
            Quaternion rotation = Quaternion.Euler(startPitch, startYaw, 0f);
            transform.rotation = rotation;

            Vector3 forward = rotation * Vector3.forward;
            if (Mathf.Abs(forward.y) < 0.001f)
                return;

            float distance = (focusPoint.y - startHeight) / forward.y;
            transform.position = focusPoint - forward * distance;
        }

        void ApplyCameraMove(Vector2 moveInput, float speed)
        {
            if (moveInput.sqrMagnitude <= 0.0001f)
                return;

            moveInput = moveInput.normalized;
            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = transform.right;
            right.y = 0f;
            right.Normalize();

            Vector3 delta = (forward * moveInput.y + right * moveInput.x) * speed * Time.deltaTime;
            transform.position += delta;
        }

        void Update()
        {
            if (input == null)
                return;

            Vector2 keyboardMove = input.CameraMove;
            Vector2 edgeMove = Vector2.zero;

            if (Application.isFocused && Time.timeSinceLevelLoad >= 0.25f)
            {
                Vector2 pointer = input.PointerScreenPosition;
                if (pointer.sqrMagnitude > 1f)
                {
                    if (pointer.x <= edgeScrollBorderPixels)
                        edgeMove.x -= 1f;
                    if (pointer.x >= Screen.width - edgeScrollBorderPixels)
                        edgeMove.x += 1f;
                    if (pointer.y <= edgeScrollBorderPixels)
                        edgeMove.y -= 1f;
                    if (pointer.y >= Screen.height - edgeScrollBorderPixels)
                        edgeMove.y += 1f;
                }
            }

            ApplyCameraMove(keyboardMove, moveSpeed);
            ApplyCameraMove(edgeMove, edgeScrollSpeed);

            float zoom = input.ZoomDelta;
            if (Mathf.Abs(zoom) > 0.001f)
            {
                float direction = Mathf.Sign(zoom);
                Vector3 position = transform.position;
                position.y = Mathf.Clamp(position.y - direction * zoomSpeed * Time.deltaTime, minHeight, maxHeight);
                transform.position = position;
            }
        }
    }
}
