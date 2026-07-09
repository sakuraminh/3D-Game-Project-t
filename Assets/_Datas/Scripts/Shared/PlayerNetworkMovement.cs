using Unity.Netcode;
using UnityEngine;

namespace Shared
{
    public class PlayerNetworkMovement : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        private void Update()
        {
            // Chỉ Client sở hữu nhân vật mới gửi lệnh di chuyển lên Server
            if (!IsOwner) return;

#if !UNITY_SERVER
            if (Client.InputManager.Instance != null)
            {
                Vector2 moveInput = Client.InputManager.Instance.MoveInput;
                if (moveInput != Vector2.zero)
                {
                    Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
                    MoveServerRpc(moveDir);
                }
            }
#endif
        }

        // RPC chạy trên Server để xử lý và cập nhật vị trí
        [ServerRpc]
        private void MoveServerRpc(Vector3 direction)
        {
            // Di chuyển nhân vật trên Server. 
            // NetworkTransform (được đính kèm trên Prefab) sẽ tự động đồng bộ hóa vị trí mới này về tất cả các Client khác.
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }
}
