using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Shared
{
    /// <summary>
    /// Hệ thống Mạng Object Pooling quản lý việc sinh và hủy NetworkObject bằng cách tái sử dụng
    /// các đối tượng từ bộ đệm (Queue Cache), triệt tiêu Instantiate() và Destroy() lúc runtime.
    /// </summary>
    public class ServerAuthoritativeNetworkPool : MonoBehaviour
    {
        public static ServerAuthoritativeNetworkPool Instance { get; private set; }

        [System.Serializable]
        public struct PoolConfig
        {
            public NetworkObject Prefab;
            public int PrewarmCount;
        }

        [Header("Pool Settings")]
        [SerializeField] private List<PoolConfig> _poolConfigs = new List<PoolConfig>();

        private readonly Dictionary<NetworkObject, Queue<NetworkObject>> _pools = new Dictionary<NetworkObject, Queue<NetworkObject>>();
        private readonly Dictionary<NetworkObject, NetworkObject> _instanceToPrefabMap = new Dictionary<NetworkObject, NetworkObject>();
        private readonly List<PooledPrefabInstanceHandler> _handlers = new List<PooledPrefabInstanceHandler>();

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
            InitializePools();
        }

        private void OnDestroy()
        {
            UnregisterHandlers();
        }

        /// <summary>
        /// Khởi tạo các pool và đăng ký handler với NetworkManager.
        /// </summary>
        private void InitializePools()
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogWarning("[ServerAuthoritativeNetworkPool] NetworkManager.Singleton is null. Skipping registration.");
                return;
            }

            foreach (var config in _poolConfigs)
            {
                if (config.Prefab == null) continue;

                if (!_pools.ContainsKey(config.Prefab))
                {
                    _pools[config.Prefab] = new Queue<NetworkObject>();

                    // Đăng ký custom handler với NetworkManager cho prefab này
                    var handler = new PooledPrefabInstanceHandler(config.Prefab, this);
                    NetworkManager.Singleton.PrefabHandler.AddHandler(config.Prefab, handler);
                    _handlers.Add(handler);

                    // Khởi tạo trước (Prewarm) số lượng đối tượng yêu cầu
                    for (int i = 0; i < config.PrewarmCount; i++)
                    {
                        NetworkObject obj = CreateNewInstance(config.Prefab);
                        obj.gameObject.SetActive(false);
                        _pools[config.Prefab].Enqueue(obj);
                    }
                }
            }
        }

        /// <summary>
        /// Đăng ký động một prefab vào pool lúc runtime (thường gọi bởi Factory).
        /// </summary>
        public void RegisterPrefab(NetworkObject prefab, int prewarmCount)
        {
            if (prefab == null) return;

            if (NetworkManager.Singleton == null)
            {
                Debug.LogWarning("[ServerAuthoritativeNetworkPool] NetworkManager.Singleton is null. Cannot register prefab dynamically.");
                return;
            }

            if (!_pools.ContainsKey(prefab))
            {
                _pools[prefab] = new Queue<NetworkObject>();

                // Đăng ký custom handler
                var handler = new PooledPrefabInstanceHandler(prefab, this);
                NetworkManager.Singleton.PrefabHandler.AddHandler(prefab, handler);
                _handlers.Add(handler);

                // Prewarm
                for (int i = 0; i < prewarmCount; i++)
                {
                    NetworkObject obj = CreateNewInstance(prefab);
                    obj.gameObject.SetActive(false);
                    _pools[prefab].Enqueue(obj);
                }
            }
        }

        /// <summary>
        /// Hủy đăng ký tất cả các handler khi pool bị hủy.
        /// </summary>
        private void UnregisterHandlers()
        {
            if (NetworkManager.Singleton == null || NetworkManager.Singleton.PrefabHandler == null) return;

            foreach (var handler in _handlers)
            {
                NetworkManager.Singleton.PrefabHandler.RemoveHandler(handler.Prefab);
            }
            _handlers.Clear();
        }

        /// <summary>
        /// Tạo một instance mới từ prefab và lưu vết ánh xạ để tái sử dụng.
        /// </summary>
        private NetworkObject CreateNewInstance(NetworkObject prefab)
        {
            NetworkObject obj = Instantiate(prefab);
            _instanceToPrefabMap[obj] = prefab;
            return obj;
        }

        /// <summary>
        /// Lấy một NetworkObject từ pool hoặc khởi tạo mới nếu pool trống.
        /// </summary>
        public NetworkObject GetNetworkObject(NetworkObject prefab, Vector3 position, Quaternion rotation)
        {
            NetworkObject obj = null;

            if (_pools.TryGetValue(prefab, out var queue) && queue.Count > 0)
            {
                obj = queue.Dequeue();
            }
            else
            {
                // Tự động mở rộng (auto-expand) pool nếu hết đối tượng sẵn có
                obj = CreateNewInstance(prefab);
            }

            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.gameObject.SetActive(true);
            return obj;
        }

        /// <summary>
        /// Trả NetworkObject về lại pool tương ứng hoặc hủy nếu đối tượng không thuộc quản lý.
        /// </summary>
        public void ReturnNetworkObject(NetworkObject obj)
        {
            obj.gameObject.SetActive(false);

            if (_instanceToPrefabMap.TryGetValue(obj, out var prefab))
            {
                if (_pools.TryGetValue(prefab, out var queue))
                {
                    queue.Enqueue(obj);
                }
                else
                {
                    // Fallback bảo vệ trong trường hợp queue bị mất dấu
                    Destroy(obj.gameObject);
                }
            }
            else
            {
                // Trường hợp đối tượng không nằm trong hệ thống pool
                Destroy(obj.gameObject);
            }
        }
    }

    /// <summary>
    /// Triển khai INetworkPrefabInstanceHandler của Unity NGO để định tuyến các lệnh sinh/hủy
    /// của NetworkManager qua hệ thống pool tương ứng.
    /// </summary>
    public class PooledPrefabInstanceHandler : INetworkPrefabInstanceHandler
    {
        public NetworkObject Prefab { get; private set; }
        private readonly ServerAuthoritativeNetworkPool _pool;

        public PooledPrefabInstanceHandler(NetworkObject prefab, ServerAuthoritativeNetworkPool pool)
        {
            Prefab = prefab;
            _pool = pool;
        }

        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            return _pool.GetNetworkObject(Prefab, position, rotation);
        }

        public void Destroy(NetworkObject networkObject)
        {
            _pool.ReturnNetworkObject(networkObject);
        }
    }
}
