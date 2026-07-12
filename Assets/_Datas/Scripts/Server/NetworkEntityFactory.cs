using Shared;
using Unity.Netcode;
using UnityEngine;

namespace Server
{
    /// <summary>
    /// Factory quản lý việc sinh và cấu hình các thực thể mạng duy nhất trên Server (Server-Authoritative).
    /// Đảm bảo mọi thực thể mạng được lấy từ hệ thống Object Pool thay vì khởi tạo trực tiếp.
    /// </summary>
    public class NetworkEntityFactory : MonoBehaviour
    {
        public static NetworkEntityFactory Instance { get; private set; }

        [Header("Entity Prefabs")]
        [SerializeField] private NetworkObject _playerPrefab;
        [SerializeField] private int _playerPrewarmCount = 10;
        [SerializeField] private NetworkObject _groupParentPrefab;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            RegisterPrefabsToPool();
            this.SpawnGameplayManagerIfNeeded();
        }

        private void SpawnGameplayManagerIfNeeded()
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

            if (Shared.GameplayManager.Instance == null)
            {
                GameObject go = new GameObject("[GameplayManager]");
                var netObj = go.AddComponent<NetworkObject>();
                go.AddComponent<Shared.GameplayManager>();
                netObj.Spawn();
                Debug.Log("[NetworkEntityFactory] Dynamically spawned [GameplayManager] on Server.");
            }
        }

        /// <summary>
        /// Đăng ký toàn bộ Prefabs cần thiết vào hệ thống Object Pool của mạng.
        /// </summary>
        private void RegisterPrefabsToPool()
        {
            if (ServerAuthoritativeNetworkPool.Instance != null)
            {
                if (_playerPrefab != null)
                {
                    ServerAuthoritativeNetworkPool.Instance.RegisterPrefab(_playerPrefab, _playerPrewarmCount);
                }
                else
                {
                    Debug.LogWarning("[NetworkEntityFactory] Player prefab is not assigned in the Inspector.");
                }

                if (_groupParentPrefab != null)
                {
                    // Đăng ký prefab cha nhóm vào pool (prewarm 3 đối tượng cho [NetworkEntitys], [Players], [Enemies])
                    ServerAuthoritativeNetworkPool.Instance.RegisterPrefab(_groupParentPrefab, 3);
                }
            }
            else
            {
                Debug.LogWarning("[NetworkEntityFactory] ServerAuthoritativeNetworkPool instance is not found. Cannot register prefabs.");
            }
        }

        /// <summary>
        /// Tìm kiếm hoặc tự động sinh GameObject cha đồng bộ qua mạng ở runtime.
        /// </summary>
        private NetworkObject GetOrCreateRuntimeParent(string parentName, NetworkObject childOf = null)
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return null;

            // Tìm xem đã tồn tại đối tượng nào cùng tên và là NetworkObject đã Spawn chưa
            foreach (var netObj in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
            {
                if (netObj != null && netObj.gameObject != null && netObj.gameObject.name == parentName)
                {
                    return netObj;
                }
            }

            // Nếu không có, khởi tạo từ Prefab cha nhóm
            if (_groupParentPrefab == null)
            {
                Debug.LogError("[NetworkEntityFactory] _groupParentPrefab chưa được gán trong Inspector!");
                return null;
            }

            NetworkObject newParent = ServerAuthoritativeNetworkPool.Instance.GetNetworkObject(_groupParentPrefab, Vector3.zero, Quaternion.identity);
            
            // Spawn trên mạng trước để tránh cảnh báo NetworkVariable
            newParent.Spawn();
            
            // Gán tên Network Group Parent để đồng bộ sau khi đã Spawn
            var groupComponent = newParent.GetComponent<NetworkGroupParent>();
            if (groupComponent != null)
            {
                groupComponent.ParentName.Value = parentName;
            }
            newParent.gameObject.name = parentName;

            // Nếu có cha cấp cao hơn (ví dụ [Players] là con của [NetworkEntitys])
            if (childOf != null)
            {
                newParent.TrySetParent(childOf, false);
            }

            return newParent;
        }

        /// <summary>
        /// Gộp nhóm thực thể Player đã spawn vào nhóm cha [Players] dưới [NetworkEntitys].
        /// </summary>
        public void ParentPlayer(NetworkObject playerObj)
        {
            if (playerObj == null || NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

            NetworkObject rootParent = GetOrCreateRuntimeParent("[NetworkEntitys]");
            if (rootParent != null)
            {
                NetworkObject playersParent = GetOrCreateRuntimeParent("[Players]", rootParent);
                if (playersParent != null)
                {
                    playerObj.TrySetParent(playersParent, false);
                }
            }
        }

        /// <summary>
        /// Gộp nhóm thực thể Enemy đã spawn vào nhóm cha [Enemies] dưới [NetworkEntitys].
        /// </summary>
        public void ParentEnemy(NetworkObject enemyObj)
        {
            if (enemyObj == null || NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

            NetworkObject rootParent = GetOrCreateRuntimeParent("[NetworkEntitys]");
            if (rootParent != null)
            {
                NetworkObject enemiesParent = GetOrCreateRuntimeParent("[Enemies]", rootParent);
                if (enemiesParent != null)
                {
                    enemyObj.TrySetParent(enemiesParent, false);
                }
            }
        }

        /// <summary>
        /// Sinh ra một nhân vật chơi mạng từ Object Pool và gán quyền sở hữu cho Client.
        /// </summary>
        /// <param name="position">Vị trí spawn</param>
        /// <param name="rotation">Góc quay spawn</param>
        /// <param name="ownerClientId">ID của client sở hữu thực thể này</param>
        /// <returns>NetworkObject của nhân vật đã được spawn</returns>
        public NetworkObject SpawnPlayer(Vector3 position, Quaternion rotation, ulong ownerClientId)
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            {
                Debug.LogError("[NetworkEntityFactory] SpawnPlayer can only be called on the Server.");
                return null;
            }

            if (_playerPrefab == null)
            {
                Debug.LogError("[NetworkEntityFactory] Player prefab is not assigned.");
                return null;
            }

            // Lấy đối tượng từ pool trên Server
            NetworkObject networkObject = ServerAuthoritativeNetworkPool.Instance.GetNetworkObject(_playerPrefab, position, rotation);
            
            if (networkObject != null)
            {
                // Thực hiện Spawn mạng và gán quyền sở hữu Client trước
                networkObject.SpawnWithOwnership(ownerClientId);

                // Gộp nhóm nhân vật vào hierarchy thích hợp
                ParentPlayer(networkObject);
            }
            else
            {
                Debug.LogError("[NetworkEntityFactory] Failed to get Player NetworkObject from Pool.");
            }

            return networkObject;
        }

        /// <summary>
        /// Sinh ra một quái vật từ Object Pool (Server-Authoritative).
        /// </summary>
        /// <param name="enemyPrefab">Prefab của quái vật cần spawn</param>
        /// <param name="position">Vị trí spawn</param>
        /// <param name="rotation">Góc quay spawn</param>
        /// <returns>NetworkObject của quái vật đã được spawn</returns>
        public NetworkObject SpawnEnemy(NetworkObject enemyPrefab, Vector3 position, Quaternion rotation)
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            {
                Debug.LogError("[NetworkEntityFactory] SpawnEnemy can only be called on the Server.");
                return null;
            }

            if (enemyPrefab == null)
            {
                Debug.LogError("[NetworkEntityFactory] Enemy prefab is null.");
                return null;
            }

            // Đăng ký động prefab vào pool nếu chưa được đăng ký
            if (ServerAuthoritativeNetworkPool.Instance != null)
            {
                ServerAuthoritativeNetworkPool.Instance.RegisterPrefab(enemyPrefab, 5);
            }
            else
            {
                Debug.LogError("[NetworkEntityFactory] ServerAuthoritativeNetworkPool.Instance is null!");
                return null;
            }

            // Lấy đối tượng từ pool trên Server
            NetworkObject networkObject = ServerAuthoritativeNetworkPool.Instance.GetNetworkObject(enemyPrefab, position, rotation);
            
            if (networkObject != null)
            {
                // Thực hiện Spawn mạng trước
                networkObject.Spawn();

                // Gộp nhóm quái vật vào hierarchy thích hợp
                ParentEnemy(networkObject);
            }
            else
            {
                Debug.LogError("[NetworkEntityFactory] Failed to get Enemy NetworkObject from Pool.");
            }

            return networkObject;
        }
    }
}
