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

            Vector2 pointer = input.PointerScreenPosition;
            if (pointer.x <= edgeScrollBorderPixels)
                edgeMove.x -= 1f;
            if (pointer.x >= Screen.width - edgeScrollBorderPixels)
                edgeMove.x += 1f;
            if (pointer.y <= edgeScrollBorderPixels)
                edgeMove.y -= 1f;
            if (pointer.y >= Screen.height - edgeScrollBorderPixels)
                edgeMove.y += 1f;

            ApplyCameraMove(keyboardMove, moveSpeed);
            ApplyCameraMove(edgeMove, edgeScrollSpeed);

            float zoom = input.ZoomDelta;
            if (Mathf.Abs(zoom) > 0.001f)
            {
                Vector3 position = transform.position;
                position.y = Mathf.Clamp(position.y - zoom * zoomSpeed * Time.deltaTime, minHeight, maxHeight);
                transform.position = position;
            }
        }
    }
}
