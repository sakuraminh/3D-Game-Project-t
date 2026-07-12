using Unity.Netcode;
using UnityEngine;

namespace Shared
{
    /// <summary>
    /// Component Shared quản lý trạng thái chung của trận đấu và xử lý các yêu cầu
    /// từ Client không yêu cầu quyền sở hữu (ví dụ: yêu cầu hồi sinh).
    /// </summary>
    public class GameplayManager : NetworkBehaviour
    {
        private static GameplayManager _instance;
        public static GameplayManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindAnyObjectByType<GameplayManager>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// RPC từ Client gửi yêu cầu hồi sinh lên Server.
        /// Sử dụng InvokePermission = Everyone vì khi chết Player đã bị despawn (Client không sở hữu NetworkObject nào).
        /// </summary>
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void RequestRespawnServerRpc(RpcParams rpcParams = default)
        {
            if (!IsServer) return;

            ulong clientId = rpcParams.Receive.SenderClientId;
            Debug.Log($"[GameplayManager] Client {clientId} requested respawn.");

            // Điểm Spawn mặc định (có thể lấy vị trí 0, 1, 0)
            Vector3 spawnPos = new Vector3(0f, 1f, 0f);

            if (Server.NetworkEntityFactory.Instance != null)
            {
                // Gọi Factory để lấy Player mới từ Pool và spawn
                NetworkObject newPlayerObj = Server.NetworkEntityFactory.Instance.SpawnPlayer(spawnPos, Quaternion.identity, clientId);
                if (newPlayerObj != null)
                {
                    // Gán thực thể làm PlayerObject chính cho Client
                    newPlayerObj.SpawnAsPlayerObject(clientId, true);
                    Debug.Log($"[GameplayManager] Successfully respawned and assigned PlayerObject for Client {clientId}.");
                }
                else
                {
                    Debug.LogError($"[GameplayManager] Failed to spawn Player from Pool for Client {clientId}.");
                }
            }
            else
            {
                Debug.LogError("[GameplayManager] NetworkEntityFactory.Instance is null. Cannot spawn Player.");
            }
        }
    }
}
