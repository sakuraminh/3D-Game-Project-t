using Unity.Netcode;
using UnityEngine;

namespace Shared
{
    /// <summary>
    /// Component Shared quản lý dữ liệu chỉ số (HP) và trạng thái của quái vật được đồng bộ qua mạng.
    /// Chỉ Server mới có quyền thay đổi các chỉ số này.
    /// </summary>
    public class EnemyNetworkData : NetworkBehaviour
    {
        [Header("Config")]
        [SerializeField] private EnemyConfig _enemyConfig;

        public EnemyConfig Config => _enemyConfig;

        // Đồng bộ lượng máu hiện tại
        public NetworkVariable<float> CurrentHP { get; } = new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        // Đồng bộ lượng máu tối đa
        public NetworkVariable<float> MaxHP { get; } = new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        // Đồng bộ trạng thái quái vật
        public NetworkVariable<EnemyState> CurrentState { get; } = new NetworkVariable<EnemyState>(
            EnemyState.Idle,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        // Các sự kiện C# Action cho Client UI/VFX quan sát (Observer Pattern)
        public System.Action<float, float> OnHealthChanged;
        public System.Action<EnemyState> OnStateChanged;

        private Server.EnemyStateMachine _stateMachine;

        private void Awake()
        {
            _stateMachine = GetComponent<Server.EnemyStateMachine>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Đăng ký callback thay đổi NetworkVariable để kích hoạt các event C# Action
            CurrentHP.OnValueChanged += HandleHealthChanged;
            CurrentState.OnValueChanged += HandleStateChanged;

            // Khởi động chỉ số từ cấu hình quái vật trên Server
            if (IsServer)
            {
                // Bật Network Culling trên Server (giới hạn gửi tọa độ/dữ liệu nếu Client ở xa 50m)
                var networkObject = GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    networkObject.CheckObjectVisibility = CheckVisibility;
                }

                if (_enemyConfig != null)
                {
                    MaxHP.Value = _enemyConfig.MaxHealth;
                    CurrentHP.Value = _enemyConfig.MaxHealth;
                }
                else
                {
                    Debug.LogWarning($"[EnemyNetworkData] EnemyConfig is missing on {gameObject.name}. Using default values.");
                    MaxHP.Value = 100f;
                    CurrentHP.Value = 100f;
                }
                CurrentState.Value = EnemyState.Idle;
            }
        }

        public override void OnNetworkDespawn()
        {
            // Hủy đăng ký callback để tránh rò rỉ bộ nhớ (Memory Leak)
            CurrentHP.OnValueChanged -= HandleHealthChanged;
            CurrentState.OnValueChanged -= HandleStateChanged;

            base.OnNetworkDespawn();
        }

        private void HandleHealthChanged(float oldValue, float newValue)
        {
            OnHealthChanged?.Invoke(newValue, MaxHP.Value);
        }

        private void HandleStateChanged(EnemyState oldValue, EnemyState newValue)
        {
            OnStateChanged?.Invoke(newValue);
        }

        /// <summary>
        /// Bộ lọc hiển thị mạng (Network Culling) cho quái vật.
        /// Trả về true nếu thực thể này nằm trong phạm vi 50m của Client chỉ định.
        /// </summary>
        private bool CheckVisibility(ulong clientId)
        {
            if (NetworkManager.Singleton != null && 
                NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var clientConnection))
            {
                var clientPlayer = clientConnection.PlayerObject;
                if (clientPlayer != null)
                {
                    float distance = Vector3.Distance(transform.position, clientPlayer.transform.position);
                    return distance <= 50f; // Bán kính Culling quái vật là 50m
                }
            }
            return false;
        }

        /// <summary>
        /// Sửa đổi lượng HP của quái vật (Chỉ gọi trên Server).
        /// </summary>
        public void ModifyHP(float amount)
        {
            if (!IsServer) return;

            float newHP = Mathf.Clamp(CurrentHP.Value + amount, 0f, MaxHP.Value);
            CurrentHP.Value = newHP;

            // Nếu HP về 0, chuyển sang trạng thái Dead trên Server thông qua State Machine
            if (newHP <= 0f)
            {
                if (_stateMachine == null)
                {
                    _stateMachine = GetComponent<Server.EnemyStateMachine>();
                }

                if (_stateMachine != null && CurrentState.Value != EnemyState.Dead)
                {
                    _stateMachine.TransitionTo(EnemyState.Dead);
                }
            }
        }

        /// <summary>
        /// Cập nhật trạng thái (Chỉ gọi trên Server).
        /// </summary>
        public void SetState(EnemyState state)
        {
            if (!IsServer) return;
            CurrentState.Value = state;
        }
    }
}
