using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

namespace Server
{
    /// <summary>
    /// Component Server chịu trách nhiệm điều khiển NavMeshAgent di chuyển của quái vật.
    /// NavMeshAgent chỉ chạy trên Server. Phía Client sẽ tắt component này để tránh xung đột.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Shared.EnemyNetworkData))]
    public class EnemyMovement : NetworkBehaviour
    {
        private NavMeshAgent _agent;
        private Shared.EnemyNetworkData _networkData;

        private void Awake()
        {
            // Caching components một lần duy nhất tại Awake (Memory Optimization)
            _agent = GetComponent<NavMeshAgent>();
            _networkData = GetComponent<Shared.EnemyNetworkData>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Chỉ cho phép NavMeshAgent hoạt động trên Server
            if (IsServer)
            {
                _agent.enabled = true;
                if (_networkData.Config != null)
                {
                    _agent.speed = _networkData.Config.MoveSpeed;
                }
            }
            else
            {
                // Client nhận đồng bộ Transform qua NetworkTransform, không cần chạy NavMeshAgent
                _agent.enabled = false;
            }
        }

        /// <summary>
        /// Ra lệnh di chuyển tới đích (Chỉ gọi từ Server).
        /// </summary>
        public void MoveTo(Vector3 destination)
        {
            if (!IsServer || !_agent.enabled) return;

            if (_agent.isStopped)
            {
                _agent.isStopped = false;
            }
            _agent.SetDestination(destination);
        }

        /// <summary>
        /// Ra lệnh dừng di chuyển và triệt tiêu vận tốc (Chỉ gọi từ Server).
        /// </summary>
        public void Stop()
        {
            if (!IsServer || !_agent.enabled) return;

            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
        }
    }
}
