using UnityEngine;

namespace Shared
{
    /// <summary>
    /// ScriptableObject chứa toàn bộ chỉ số cân bằng của một loại nhân vật (Strategy Pattern).
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacterConfig", menuName = "Configs/Character Config")]
    public class CharacterConfig : ScriptableObject
    {
        [Header("Base Stats")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _baseDefense = 10f;
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 12f;

        public float MaxHealth => _maxHealth;
        public float BaseDefense => _baseDefense;
        public float MoveSpeed => _moveSpeed;
        public float RotationSpeed => _rotationSpeed;
    }
}
