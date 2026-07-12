using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Client
{
    /// <summary>
    /// Component chịu trách nhiệm thu thập dữ liệu di chuyển và phím tắt tương tác từ Input System mới trên Client (Owner)
    /// và gửi lên Server / UI Controller để xử lý.
    /// </summary>
    [RequireComponent(typeof(Shared.PlayerNetworkHandler))]
    public class PlayerInputHandler : NetworkBehaviour
    {
        private Shared.PlayerNetworkHandler _networkHandler;
        private PlayerHUDUIHandler _hudUIHandler;

        private NetworkObject _currentTarget;

        private void Awake()
        {
            _networkHandler = GetComponent<Shared.PlayerNetworkHandler>();
            _hudUIHandler = GetComponent<PlayerHUDUIHandler>();
        }

        private void Update()
        {
            // Chỉ Client sở hữu thực thể này mới thu thập input và gửi đi
            if (IsClient && IsOwner)
            {
#if !UNITY_SERVER
                // 1. Gửi input di chuyển
                if (InputManager.Instance != null)
                {
                    Vector2 moveInput = InputManager.Instance.MoveInput;
                    _networkHandler.SendMoveInputServerRpc(moveInput);
                }

                // 2. Chọn mục tiêu quái vật khi nhấn chuột trái
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                    if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                    {
                        var enemyData = hit.collider.GetComponentInParent<Shared.EnemyNetworkData>();
                        if (enemyData != null)
                        {
                            var netObj = enemyData.GetComponent<NetworkObject>();
                            if (netObj != null && netObj.IsSpawned)
                            {
                                _currentTarget = netObj;
                                Debug.Log($"[PlayerInputHandler] Selected target: {enemyData.gameObject.name} (ID: {netObj.NetworkObjectId})", gameObject);
                            }
                        }
                        else
                        {
                            // Nếu click ra ngoài hoặc click vào thứ không phải quái vật, bỏ chọn mục tiêu
                            _currentTarget = null;
                            Debug.Log("[PlayerInputHandler] Target cleared.", gameObject);
                        }
                    }
                }

                // 3. Bắt phím tắt hành động sử dụng Input System mới (Keyboard.current)
                if (Keyboard.current != null)
                {
                    // Phím I: Mở/đóng hòm đồ (Local UI Toggle)
                    if (Keyboard.current.iKey.wasPressedThisFrame)
                    {
                        if (_hudUIHandler != null)
                        {
                            _hudUIHandler.ToggleInventory();
                        }
                    }

                    // Phím 1: Hồi máu (Server-validated RPC)
                    if (Keyboard.current.digit1Key.wasPressedThisFrame)
                    {
                        _networkHandler.RequestHealServerRpc();
                    }

                    // Phím 2: Dùng Kỹ năng 1 ở index 0 (Server-validated RPC)
                    if (Keyboard.current.digit2Key.wasPressedThisFrame)
                    {
                        _networkHandler.RequestCastSkillServerRpc(0, _currentTarget != null ? _currentTarget : default);
                    }

                    // Phím 3: Dùng Kỹ năng 2 ở index 1 (Server-validated RPC)
                    if (Keyboard.current.digit3Key.wasPressedThisFrame)
                    {
                        _networkHandler.RequestCastSkillServerRpc(1, _currentTarget != null ? _currentTarget : default);
                    }
                }
#endif
            }
        }
    }
}
