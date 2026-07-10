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

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            RegisterPrefabsToPool();
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
            }
            else
            {
                Debug.LogWarning("[NetworkEntityFactory] ServerAuthoritativeNetworkPool instance is not found. Cannot register prefabs.");
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
                // Thực hiện Spawn mạng và gán quyền sở hữu Client
                networkObject.SpawnWithOwnership(ownerClientId);
            }
            else
            {
                Debug.LogError("[NetworkEntityFactory] Failed to get Player NetworkObject from Pool.");
            }

            return networkObject;
        }
    }
}
