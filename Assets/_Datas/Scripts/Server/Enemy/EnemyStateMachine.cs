using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Shared;

namespace Server
{
    /// <summary>
    /// Máy trạng thái AI (Enemy FSM) chịu trách nhiệm cập nhật và điều phối các trạng thái trên Server.
    /// </summary>
    [RequireComponent(typeof(Shared.EnemyNetworkData))]
    public class EnemyStateMachine : NetworkBehaviour
    {
        private Shared.EnemyNetworkData _networkData;
        private EnemyMovement _movement;
        private EnemyServerCombat _combat;

        private EnemyBaseState _currentState;
        private readonly Dictionary<EnemyState, EnemyBaseState> _states = new Dictionary<EnemyState, EnemyBaseState>();

        // Lưu trữ người chơi mục tiêu hiện tại (Server-only)
        public PlayerNetworkHandler Target { get; set; }
        public float NextAttackTime { get; set; }

        public Shared.EnemyNetworkData NetworkData => _networkData;
        public EnemyMovement Movement => _movement;
        public EnemyServerCombat Combat => _combat;

        private void Awake()
        {
            // Caching components một lần duy nhất tại Awake (Memory Optimization)
            _networkData = GetComponent<Shared.EnemyNetworkData>();
            _movement = GetComponent<EnemyMovement>();
            _combat = GetComponent<EnemyServerCombat>();

            // Khởi tạo các trạng thái AI
            _states[EnemyState.Idle] = new EnemyIdleState(this);
            _states[EnemyState.Chase] = new EnemyChaseState(this);
            _states[EnemyState.Attack] = new EnemyAttackState(this);
            _states[EnemyState.Dead] = new EnemyDeadState(this);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Khởi chạy FSM trên Server với trạng thái mặc định là Idle
            if (IsServer)
            {
                TransitionTo(EnemyState.Idle);
            }
        }

        private void Update()
        {
            // FSM chỉ chạy logic xử lý trên Server (Server-Authoritative)
            if (!IsServer) return;

            _currentState?.Update();
        }

        /// <summary>
        /// Thực hiện chuyển trạng thái AI và đồng bộ biến mạng (Chỉ chạy trên Server).
        /// </summary>
        public void TransitionTo(EnemyState newState)
        {
            if (!IsServer) return;

            // Thoát trạng thái hiện tại
            _currentState?.Exit();

            // Cập nhật NetworkVariable để đồng bộ trạng thái xuống Client
            _networkData.SetState(newState);

            // Chuyển và khởi chạy trạng thái mới
            _currentState = _states[newState];
            _currentState?.Enter();

            Debug.Log($"[EnemyStateMachine] {gameObject.name} transitioned to state: {newState}");
        }
    }
}
