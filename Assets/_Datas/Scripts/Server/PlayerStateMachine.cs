using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Shared;

namespace Server
{
    /// <summary>
    /// Máy trạng thái mạng (Network FSM) quản lý vòng đời trạng thái của nhân vật trên Server.
    /// </summary>
    [RequireComponent(typeof(Shared.PlayerNetworkHandler))]
    public class PlayerStateMachine : NetworkBehaviour
    {
        [Header("FSM Settings")]
        [SerializeField] private float _attackDuration = 0.5f;

        private Shared.PlayerNetworkHandler _networkHandler;
        private PlayerBaseState _currentState;
        private readonly Dictionary<PlayerState, PlayerBaseState> _states = new Dictionary<PlayerState, PlayerBaseState>();

        public float AttackDuration => _attackDuration;
        public Shared.PlayerNetworkHandler NetworkHandler => _networkHandler;

        private void Awake()
        {
            _networkHandler = GetComponent<Shared.PlayerNetworkHandler>();

            // Đăng ký các trạng thái vào FSM
            _states[PlayerState.Idle] = new PlayerIdleState(this);
            _states[PlayerState.Locomotion] = new PlayerLocomotionState(this);
            _states[PlayerState.Attacking] = new PlayerAttackingState(this);
            _states[PlayerState.Dead] = new PlayerDeadState(this);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Khởi động FSM trên Server với trạng thái mặc định là Idle
            if (IsServer)
            {
                TransitionTo(PlayerState.Idle);
            }
        }

        private void Update()
        {
            // Chỉ chạy logic cập nhật trạng thái FSM trên Server (Server-Authoritative)
            if (!IsServer) return;

            _currentState?.Update();
        }

        /// <summary>
        /// Thực hiện chuyển trạng thái trên Server và đồng bộ trạng thái mạng.
        /// </summary>
        public void TransitionTo(PlayerState newState)
        {
            if (!IsServer) return;

            // Thoát trạng thái hiện tại
            _currentState?.Exit();

            // Cập nhật trạng thái mạng để đồng bộ xuống toàn bộ Client
            _networkHandler.SetState(newState);

            // Chuyển sang trạng thái mới
            _currentState = _states[newState];
            _currentState?.Enter();
        }
    }
}
