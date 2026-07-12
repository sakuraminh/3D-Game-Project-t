using System.Collections.Generic;
using UnityEngine;

namespace Shared
{
    [System.Serializable]
    public struct LootDropData
    {
        [SerializeField] private string _itemId;
        [SerializeField] [Range(0f, 1f)] private float _dropChance;

        public string ItemId => _itemId;
        /// <summary>
        /// Tỉ lệ rơi vật phẩm (từ 0.0 đến 1.0 tương đương 0% đến 100%)
        /// </summary>
        public float DropChance => _dropChance;
    }

    /// <summary>
    /// ScriptableObject chứa toàn bộ chỉ số cấu hình cơ bản của một loại quái vật (Data-Driven Design).
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemyConfig", menuName = "Configs/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Base Stats")]
        [SerializeField] private float _maxHealth = 50f;
        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _damage = 10f;

        [Header("Visual Settings")]
        [SerializeField] private string _modelAddressableKey;

        public string ModelAddressableKey => _modelAddressableKey;

        [Header("AI & Combat Settings")]
        [SerializeField] private float _detectionRadius = 8f;
        [SerializeField] private float _attackRange = 2f;
        [SerializeField] private float _attackCooldown = 1.5f;

        [Header("Loot Drop Table")]
        [SerializeField] private List<LootDropData> _lootDropTable = new List<LootDropData>();

        public float MaxHealth => _maxHealth;
        public float MoveSpeed => _moveSpeed;
        public float Damage => _damage;
        public float DetectionRadius => _detectionRadius;
        public float AttackRange => _attackRange;
        public float AttackCooldown => _attackCooldown;
        public IReadOnlyList<LootDropData> LootDropTable => _lootDropTable;
    }
}
