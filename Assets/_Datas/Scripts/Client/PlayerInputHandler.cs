using Unity.Netcode;
using UnityEngine;

namespace Client
{
    /// <summary>
    /// Component chịu trách nhiệm thu thập dữ liệu di chuyển từ InputManager trên Client (Owner)
    /// và gửi lên Server thông qua PlayerNetworkHandler.
    /// </summary>
    [RequireComponent(typeof(Shared.PlayerNetworkHandler))]
    public class PlayerInputHandler : NetworkBehaviour
    {
        private Shared.PlayerNetworkHandler _networkHandler;

        private void Awake()
        {
            _networkHandler = GetComponent<Shared.PlayerNetworkHandler>();
        }

        private void Update()
        {
            // Chỉ Client sở hữu thực thể này mới thu thập input và gửi lên Server
            if (IsClient && IsOwner)
            {
#if !UNITY_SERVER
                if (InputManager.Instance != null)
                {
                    Vector2 moveInput = InputManager.Instance.MoveInput;
                    _networkHandler.SendMoveInputServerRpc(moveInput);
                }
#endif
            }
        }
    }
}
