using Unity.Netcode;
using UnityEngine;

namespace Shared
{
    /// <summary>
    /// Component Shared quản lý dữ liệu chỉ số (HP/MP) của nhân vật được đồng bộ qua mạng.
    /// Chỉ Server mới có quyền thay đổi các chỉ số này.
    /// </summary>
    [RequireComponent(typeof(PlayerNetworkHandler))]
    public class PlayerNetworkData : NetworkBehaviour
    {
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

        // Đồng bộ năng lượng hiện tại
        public NetworkVariable<float> CurrentMP { get; } = new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        // Đồng bộ năng lượng tối đa
        public NetworkVariable<float> MaxMP { get; } = new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private PlayerNetworkHandler _networkHandler;

        private void Awake()
        {
            _networkHandler = GetComponent<PlayerNetworkHandler>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Khởi động chỉ số từ cấu hình nhân vật trên Server
            if (IsServer)
            {
                var config = _networkHandler.Config;
                if (config != null)
                {
                    MaxHP.Value = config.MaxHealth;
                    CurrentHP.Value = config.MaxHealth;
                    MaxMP.Value = config.MaxMana;
                    CurrentMP.Value = config.MaxMana;
                }
                else
                {
                    Debug.Log($"[PlayerNetworkData] CharacterConfig is null on Client {OwnerClientId}. Using default values (HP: 100, MP: 100).");
                    MaxHP.Value = 100f;
                    CurrentHP.Value = 100f;
                    MaxMP.Value = 100f;
                    CurrentMP.Value = 100f;
                }
            }
        }

        /// <summary>
        /// Sửa đổi HP hiện tại (Chỉ gọi từ Server).
        /// </summary>
        public void ModifyHP(float amount)
        {
            if (!IsServer) return;

            float newHP = Mathf.Clamp(CurrentHP.Value + amount, 0f, MaxHP.Value);
            CurrentHP.Value = newHP;

            // Nếu HP về 0, chuyển sang trạng thái Dead trên Server thông qua State Machine
            if (newHP <= 0f)
            {
                var stateMachine = GetComponent<Server.PlayerStateMachine>();
                if (stateMachine != null && _networkHandler.CurrentState.Value != PlayerState.Dead)
                {
                    stateMachine.TransitionTo(PlayerState.Dead);
                }
            }
        }

        /// <summary>
        /// Sửa đổi MP hiện tại (Chỉ gọi từ Server).
        /// </summary>
        public void ModifyMP(float amount)
        {
            if (!IsServer) return;

            float newMP = Mathf.Clamp(CurrentMP.Value + amount, 0f, MaxMP.Value);
            CurrentMP.Value = newMP;
        }
    }
}
