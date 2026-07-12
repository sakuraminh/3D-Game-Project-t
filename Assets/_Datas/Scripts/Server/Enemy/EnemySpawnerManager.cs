using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Server
{
    /// <summary>
    /// Component quản lý hệ thống tự động sinh quái vật mạng trên Server (Server-only).
    /// </summary>
    public class EnemySpawnerManager : NetworkBehaviour
    {
        [Header("Spawn Configuration")]
        [SerializeField] private NetworkObject _enemyPrefab;
        [SerializeField] private float _spawnInterval = 5f;
        [SerializeField] private int _maxEnemies = 5;
        [SerializeField] private float _spawnRadius = 15f;

        private readonly List<NetworkObject> _activeEnemies = new List<NetworkObject>();
        private float _nextSpawnTime;
        private bool _hasLoggedNullFactory;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            // Thiết lập thời gian sinh quái đầu tiên
            if (IsServer)
            {
                _nextSpawnTime = Time.time + 1f; // Chờ 1 giây sau khi game bắt đầu để spawn đợt đầu
            }
        }

        private void Update()
        {
            // Spawner chỉ chạy logic khi đã được spawn trên mạng và là Server
            if (!IsSpawned || !IsServer) return;

            // 1. Dọn dẹp danh sách quái vật đã bị despawn (quay về Pool)
            _activeEnemies.RemoveAll(enemy => enemy == null || !enemy.IsSpawned);

            // 2. Kiểm tra điều kiện sinh quái vật mới
            if (_activeEnemies.Count < _maxEnemies && Time.time >= _nextSpawnTime)
            {
                _nextSpawnTime = Time.time + _spawnInterval;
                SpawnSingleEnemy();
            }
        }

        /// <summary>
        /// Thực thi lấy quái vật từ Pool mạng và spawn tại vị trí ngẫu nhiên xung quanh Spawner.
        /// </summary>
        private void SpawnSingleEnemy()
        {
            if (_enemyPrefab == null)
            {
                Debug.LogError($"[EnemySpawnerManager] Enemy Prefab is not assigned on {gameObject.name}.");
                return;
            }

            if (NetworkEntityFactory.Instance == null)
            {
                if (!_hasLoggedNullFactory)
                {
                    Debug.LogError("[EnemySpawnerManager] NetworkEntityFactory.Instance is null. Cannot spawn enemy. Hãy chắc chắn rằng component NetworkEntityFactory đã được gắn vào một GameObject (ví dụ: [Managers]) trong Scene.");
                    _hasLoggedNullFactory = true;
                }
                return;
            }
            _hasLoggedNullFactory = false;

            // Tính toán vị trí ngẫu nhiên
            Vector2 randomCircle = Random.insideUnitCircle * _spawnRadius;
            Vector3 randomPos = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

            // Chiếu điểm ngẫu nhiên lên lưới NavMesh để đảm bảo quái vật đứng trên đường đi được
            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, _spawnRadius, NavMesh.AllAreas))
            {
                randomPos = hit.position;
            }

            // Gọi Factory để xin cấp phát và kích hoạt thực thể mạng quái vật
            NetworkObject enemyInstance = NetworkEntityFactory.Instance.SpawnEnemy(_enemyPrefab, randomPos, Quaternion.identity);

            if (enemyInstance != null)
            {
                _activeEnemies.Add(enemyInstance);
                Debug.Log($"[EnemySpawnerManager] Spawned enemy {enemyInstance.name} at {randomPos}. Active count: {_activeEnemies.Count}/{_maxEnemies}");
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Vẽ vòng tròn bán kính spawn trong Editor để dễ quan sát trực quan
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _spawnRadius);
        }
    }
}
