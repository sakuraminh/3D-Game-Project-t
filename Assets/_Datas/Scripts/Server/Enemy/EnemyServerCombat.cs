using Unity.Netcode;
using UnityEngine;
using Shared;

namespace Server
{
    /// <summary>
    /// Component Server xử lý logic chiến đấu của quái vật bao gồm tấn công người chơi và nhận sát thương.
    /// </summary>
    [RequireComponent(typeof(Shared.EnemyNetworkData))]
    public class EnemyServerCombat : NetworkBehaviour
    {
        private Shared.EnemyNetworkData _networkData;

        private void Awake()
        {
            // Caching component một lần duy nhất tại Awake (Memory Optimization)
            _networkData = GetComponent<Shared.EnemyNetworkData>();
        }

        /// <summary>
        /// Thực thi đòn đánh vật lý lên mục tiêu người chơi (Chỉ chạy trên Server).
        /// </summary>
        public void PerformAttack(PlayerNetworkHandler playerTarget)
        {
            if (!IsServer || playerTarget == null) return;

            // Kiểm tra xem người chơi mục tiêu có còn sống không
            if (playerTarget.CurrentState.Value == PlayerState.Dead) return;

            var playerHPData = playerTarget.GetComponent<PlayerNetworkData>();
            if (playerHPData != null)
            {
                float damageDealt = 10f; // Sát thương mặc định
                if (_networkData.Config != null)
                {
                    damageDealt = _networkData.Config.Damage;
                }

                // Thực hiện trừ máu người chơi trên Server
                playerHPData.ModifyHP(-damageDealt);

                // Gọi RPC phát hiệu ứng số nhảy dame tại vị trí người chơi
                playerTarget.PlayHitEffectClientRpc((int)damageDealt, playerTarget.transform.position);

                Debug.Log($"[EnemyServerCombat] {gameObject.name} attacked Client {playerTarget.OwnerClientId} dealing {damageDealt} damage.");
            }
        }

        /// <summary>
        /// Xử lý khi quái vật bị nhận sát thương từ người chơi (Chỉ gọi từ Server).
        /// </summary>
        public void TakeDamage(float damageAmount)
        {
            if (!IsServer) return;

            // Nếu quái đã chết rồi thì bỏ qua
            if (_networkData.CurrentState.Value == EnemyState.Dead) return;

            // Trừ máu quái vật
            _networkData.ModifyHP(-damageAmount);

            // Gọi RPC phát số nhảy sát thương trên đầu quái vật cho tất cả Client
            PlayHitEffectClientRpc((int)damageAmount, transform.position);

            Debug.Log($"[EnemyServerCombat] {gameObject.name} took {damageAmount} damage. Current HP: {_networkData.CurrentHP.Value}/{_networkData.MaxHP.Value}");
        }

        /// <summary>
        /// RPC từ Server gửi xuống toàn bộ Client để phát hiệu ứng số nhảy sát thương cho quái vật.
        /// </summary>
        [ClientRpc]
        public void PlayHitEffectClientRpc(int damageAmount, Vector3 position)
        {
            if (Client.DamageTextPoolManager.Instance != null)
            {
                Client.DamageTextPoolManager.Instance.SpawnDamageText(damageAmount, position);
            }
        }
    }
}
