using UnityEngine;

namespace Shared
{
    /// <summary>
    /// ScriptableObject chứa cấu hình và chỉ số cân bằng của kỹ năng (Strategy Pattern).
    /// </summary>
    [CreateAssetMenu(fileName = "NewSkillData", menuName = "Configs/Skill Data")]
    public class SkillData : ScriptableObject
    {
        [Header("Skill Stats")]
        [SerializeField] private string _skillName = "Basic Attack";
        [SerializeField] private float _baseDamage = 20f;
        [SerializeField] private float _cooldown = 1f;
        [SerializeField] private float _manaCost = 15f;

        public string SkillName => _skillName;
        public float BaseDamage => _baseDamage;
        public float Cooldown => _cooldown;
        public float ManaCost => _manaCost;
    }
}
