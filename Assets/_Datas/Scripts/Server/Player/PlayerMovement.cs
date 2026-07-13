using Unity.Netcode;
using UnityEngine;

namespace Server
{
    /// <summary>
    /// Component chịu trách nhiệm tính toán vật lý di chuyển và cập nhật vị trí,
    /// xoay hướng của thực thể duy nhất trên Server (Server-Authoritative).
    /// </summary>
    [RequireComponent(typeof(Shared.PlayerNetworkHandler))]
    public class PlayerMovement : NetworkBehaviour
    {
        private Shared.PlayerNetworkHandler _networkHandler;

        private void Awake()
        {
            _networkHandler = GetComponent<Shared.PlayerNetworkHandler>();
        }

        private void Update()
        {
            // Chỉ thực hiện logic di chuyển và xoay hướng thực thể trên Server
            if (!IsServer) return;

            // Chỉ cho phép di chuyển vật lý khi đang ở trạng thái di chuyển Locomotion
            if (_networkHandler.CurrentState.Value != Shared.PlayerState.Locomotion) return;

            Vector2 input = _networkHandler.MoveInput;
            float cameraYaw = _networkHandler.CameraYaw;

            // Tính toán hướng di chuyển tương đối theo góc quay Y của camera
            Quaternion cameraRotation = Quaternion.Euler(0f, cameraYaw, 0f);
            Vector3 moveInputDirection = new Vector3(input.x, 0f, input.y);
            Vector3 moveDir = cameraRotation * moveInputDirection;

            if (moveDir != Vector3.zero)
            {
                // Lấy thông số tốc độ từ cấu hình CharacterConfig (Strategy Pattern)
                float speed = _networkHandler.Config != null ? _networkHandler.Config.MoveSpeed : 5f;
                float rotationSpeed = _networkHandler.Config != null ? _networkHandler.Config.RotationSpeed : 12f;

                // Cập nhật vị trí di chuyển
                transform.position += moveDir * speed * Time.deltaTime;

                // Xoay hướng mặt nhân vật mượt mà theo hướng di chuyển thực tế (moveDir)
                Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}
