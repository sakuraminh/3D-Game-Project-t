using Unity.Netcode;
using UnityEngine;

namespace Shared
{
    /// <summary>
    /// Component Shared chịu trách nhiệm đồng bộ trạng thái mạng,
    /// tiếp nhận input thô từ Client qua RPC, validate và lưu trữ trạng thái để Server xử lý.
    /// </summary>
    [RequireComponent(typeof(PlayerNetworkData))]
    [RequireComponent(typeof(Server.PlayerServerCombat))]
    public class PlayerNetworkHandler : NetworkBehaviour
    {
        // NetworkVariable đồng bộ trạng thái nhân vật từ Server xuống toàn bộ Client
        public NetworkVariable<PlayerState> CurrentState { get; } = new NetworkVariable<PlayerState>(
            PlayerState.Idle, 
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Server
        );

        // Lưu trữ input di chuyển hiện tại trên Server
        public Vector2 MoveInput { get; private set; } = Vector2.zero;

        [Header("Character Config")]
        [SerializeField] private CharacterConfig _characterConfig;

        public CharacterConfig Config => _characterConfig;

        // Biến đệm cache component (Memory Caching)
        private Server.PlayerStateMachine _stateMachine;

        private void Awake()
        {
            _stateMachine = GetComponent<Server.PlayerStateMachine>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            // Thiết lập Network Culling trên Server
            if (IsServer)
            {
                var networkObject = GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    networkObject.CheckObjectVisibility = CheckVisibility;

                    // Tự động gộp nhóm Player vào Hierarchy
                    if (Server.NetworkEntityFactory.Instance != null)
                    {
                        Server.NetworkEntityFactory.Instance.ParentPlayer(networkObject);
                    }
                }
            }

            if (IsClient)
            {
                CurrentState.OnValueChanged += OnStateChanged;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsClient)
            {
                CurrentState.OnValueChanged -= OnStateChanged;
            }
            base.OnNetworkDespawn();
        }

        private void OnStateChanged(PlayerState oldState, PlayerState newState)
        {
            if (IsOwner && newState == PlayerState.Dead)
            {
                if (Client.ResurrectionUIHandler.Instance != null)
                {
                    Client.ResurrectionUIHandler.Instance.Show();
                }
            }
        }

        /// <summary>
        /// Bộ lọc hiển thị mạng (Network Culling).
        /// Trả về true nếu thực thể này nằm trong phạm vi 30m của Client chỉ định.
        /// </summary>
        private bool CheckVisibility(ulong clientId)
        {
            // Owner Client luôn luôn nhìn thấy nhân vật của chính họ
            if (clientId == OwnerClientId) return true;

            // Kiểm tra khoảng cách tới Player đại diện của Client nhận dữ liệu
            if (NetworkManager.Singleton != null && 
                NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var clientConnection))
            {
                var clientPlayer = clientConnection.PlayerObject;
                if (clientPlayer != null)
                {
                    float distance = Vector3.Distance(transform.position, clientPlayer.transform.position);
                    return distance <= 30f; // Bán kính Culling 30m
                }
            }
            return false;
        }

        /// <summary>
        /// RPC chạy trên Server để tiếp nhận input từ Owner Client.
        /// </summary>
        [ServerRpc]
        public void SendMoveInputServerRpc(Vector2 input)
        {
            // Validate: Nếu đang Dead, từ chối nhận input di chuyển
            if (CurrentState.Value == PlayerState.Dead)
            {
                MoveInput = Vector2.zero;
                return;
            }

            // Thực hiện validate input ngay trên Server để triệt tiêu cheat tốc độ di chuyển
            MoveInput = Vector2.ClampMagnitude(input, 1f);
        }

        /// <summary>
        /// Cập nhật trạng thái của thực thể (chỉ chạy trên Server).
        /// </summary>
        public void SetState(PlayerState state)
        {
            if (!IsServer) return;
            CurrentState.Value = state;
        }

        /// <summary>
        /// Reset input di chuyển về không (chỉ chạy trên Server).
        /// </summary>
        public void ResetMoveInputOnServer()
        {
            if (!IsServer) return;
            MoveInput = Vector2.zero;
        }

        /// <summary>
        /// RPC thử nghiệm đòn đánh từ Client gửi lên Server.
        /// Thực hiện validate nghiêm ngặt trạng thái hiện tại.
        /// </summary>
        [ServerRpc]
        public void RequestAttackServerRpc()
        {
            // Validate: Đang Dead thì tuyệt đối không thể thực thi RPC tấn công
            if (CurrentState.Value == PlayerState.Dead)
            {
                Debug.LogWarning($"[PlayerNetworkHandler] Client {OwnerClientId} tried to Attack but is DEAD. Action rejected.");
                return;
            }

            // Validate: Nếu đang tấn công rồi thì không được kích hoạt đòn đánh mới đè lên
            if (CurrentState.Value == PlayerState.Attacking)
            {
                Debug.LogWarning($"[PlayerNetworkHandler] Client {OwnerClientId} is already Attacking. Action ignored.");
                return;
            }

            // Thực thi chuyển trạng thái sang Attacking trên Server (Sử dụng cache)
            if (_stateMachine != null)
            {
                _stateMachine.TransitionTo(PlayerState.Attacking);
                Debug.Log($"[PlayerNetworkHandler] Client {OwnerClientId} activated Attack state on Server.");
            }
        }

        /// <summary>
        /// RPC thử nghiệm chuyển đổi trạng thái sống/chết (Resurrect/Die) từ Client.
        /// </summary>
        [ServerRpc]
        public void RequestToggleDeadServerRpc()
        {
            if (_stateMachine != null)
            {
                if (CurrentState.Value == PlayerState.Dead)
                {
                    _stateMachine.TransitionTo(PlayerState.Idle);
                    Debug.Log($"[PlayerNetworkHandler] Client {OwnerClientId} resurrected on Server.");
                }
                else
                {
                    _stateMachine.TransitionTo(PlayerState.Dead);
                    Debug.Log($"[PlayerNetworkHandler] Client {OwnerClientId} died on Server.");
                }
            }
        }

        /// <summary>
        /// RPC từ Client gửi yêu cầu hồi máu lên Server.
        /// </summary>
        [ServerRpc]
        public void RequestHealServerRpc()
        {
            var serverCombat = GetComponent<Server.PlayerServerCombat>();
            if (serverCombat != null)
            {
                serverCombat.ProcessHealRequest();
            }
            else
            {
                Debug.LogWarning($"[PlayerNetworkHandler] PlayerServerCombat component not found on Client {OwnerClientId}'s Player object.");
            }
        }

        /// <summary>
        /// RPC từ Client gửi yêu cầu sử dụng kỹ năng lên Server.
        /// </summary>
        [ServerRpc]
        public void RequestCastSkillServerRpc(int skillIndex, NetworkObjectReference targetRef)
        {
            var serverCombat = GetComponent<Server.PlayerServerCombat>();
            if (serverCombat != null)
            {
                serverCombat.ProcessCastSkillRequest(skillIndex, targetRef);
            }
            else
            {
                Debug.LogWarning($"[PlayerNetworkHandler] PlayerServerCombat component not found on Client {OwnerClientId}'s Player object.");
            }
        }

        /// <summary>
        /// RPC từ Server gửi xuống toàn bộ Client để phát hiệu ứng số nhảy sát thương.
        /// </summary>
        [ClientRpc]
        public void PlayHitEffectClientRpc(int damageAmount, Vector3 position)
        {
            if (Client.DamageTextPoolManager.Instance != null)
            {
                Client.DamageTextPoolManager.Instance.SpawnDamageText(damageAmount, position);
            }
        }
    }
}
