using Unity.Netcode;
using UnityEngine;

namespace Shared
{
    public class PlayerNetworkMovement : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 12f;

        private Vector3 _serverMoveInput = Vector3.zero;

        private void Update()
        {
            // 1. Server xử lý di chuyển và xoay nhân vật dựa trên input state hiện tại
            if (IsServer)
            {
                if (_serverMoveInput != Vector3.zero)
                {
                    // Di chuyển nhân vật trên Server
                    transform.position += _serverMoveInput * moveSpeed * Time.deltaTime;

                    // Xoay mặt nhân vật mượt mà theo hướng di chuyển
                    Quaternion targetRotation = Quaternion.LookRotation(_serverMoveInput, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }

            // 2. Client (Owner) liên tục gửi input lên Server để cập nhật trạng thái
            if (IsClient && IsOwner)
            {
#if !UNITY_SERVER
                if (Client.InputManager.Instance != null)
                {
                    Vector2 moveInput = Client.InputManager.Instance.MoveInput;
                    Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
                    
                    // Gửi input hiện tại (kể cả Vector3.zero khi thả tay khỏi phím)
                    MoveServerRpc(moveDir);
                }
#endif
            }
        }

        // RPC chạy trên Server để cập nhật input state từ Owner Client
        [ServerRpc]
        private void MoveServerRpc(Vector3 direction)
        {
            // Giới hạn input để tránh cheat tốc độ
            _serverMoveInput = Vector3.ClampMagnitude(direction, 1f);
        }
    }
}
