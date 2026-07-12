using Unity.Netcode;
using UnityEngine;
using Shared;

namespace Server
{
    /// <summary>
    /// Component Server chịu trách nhiệm validate và thực thi các hành động chiến đấu như hồi máu, dùng kỹ năng.
    /// Script này chỉ chạy logic trên Server.
    /// </summary>
    [RequireComponent(typeof(PlayerNetworkHandler))]
    [RequireComponent(typeof(PlayerNetworkData))]
    public class PlayerServerCombat : NetworkBehaviour
    {
        [Header("Heal Settings")]
        [SerializeField] private float _healAmount = 25f;
        [SerializeField] private float _healCooldown = 2f;

        [Header("Skills Configuration")]
        [SerializeField] private SkillData[] _skills;

        private PlayerNetworkHandler _networkHandler;
        private PlayerNetworkData _networkData;
        private PlayerStateMachine _stateMachine;

        // Lưu trữ thời gian tiếp theo được phép sử dụng hành động
        private float _nextHealTime;
        private float[] _nextSkillTimes;

        private void Awake()
        {
            _networkHandler = GetComponent<PlayerNetworkHandler>();
            _networkData = GetComponent<PlayerNetworkData>();
            _stateMachine = GetComponent<PlayerStateMachine>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Chỉ khởi tạo cooldown trên Server
            if (IsServer)
            {
                if (_skills != null && _skills.Length > 0)
                {
                    _nextSkillTimes = new float[_skills.Length];
                }
                else
                {
                    _nextSkillTimes = new float[0];
                }
            }
        }

        /// <summary>
        /// Xử lý và validate yêu cầu hồi máu từ Client.
        /// </summary>
        public void ProcessHealRequest()
        {
            if (!IsServer) return;

            // 1. Kiểm tra trạng thái: Đang chết thì không thể hồi máu
            if (_networkHandler.CurrentState.Value == PlayerState.Dead)
            {
                Debug.Log($"[PlayerServerCombat] Client {OwnerClientId} tried to heal but is DEAD.");
                return;
            }

            // 2. Kiểm tra chỉ số: Máu đã đầy chưa
            if (_networkData.CurrentHP.Value >= _networkData.MaxHP.Value)
            {
                Debug.Log($"[PlayerServerCombat] Client {OwnerClientId} tried to heal but HP is already full.");
                return;
            }

            // 3. Kiểm tra Cooldown
            if (Time.time < _nextHealTime)
            {
                Debug.Log($"[PlayerServerCombat] Client {OwnerClientId} tried to heal but action is on cooldown.");
                return;
            }

            // Thực thi hồi máu và thiết lập cooldown mới
            _nextHealTime = Time.time + _healCooldown;
            _networkData.ModifyHP(_healAmount);

            Debug.Log($"[PlayerServerCombat] Client {OwnerClientId} healed {_healAmount} HP. Current HP: {_networkData.CurrentHP.Value}/{_networkData.MaxHP.Value}");
        }

        /// <summary>
        /// Xử lý và validate yêu cầu sử dụng kỹ năng từ Client.
        /// </summary>
        public void ProcessCastSkillRequest(int skillIndex, NetworkObjectReference targetRef)
        {
            if (!IsServer) return;

            // 1. Kiểm tra trạng thái: Đang chết hoặc đang trong trạng thái Attacking thì không được dùng kỹ năng mới
            if (_networkHandler.CurrentState.Value == PlayerState.Dead)
            {
                Debug.Log($"[PlayerServerCombat] Client {OwnerClientId} tried to cast skill but is DEAD.");
                return;
            }

            if (_networkHandler.CurrentState.Value == PlayerState.Attacking)
            {
                Debug.Log($"[PlayerServerCombat] Client {OwnerClientId} is already performing an action/skill.");
                return;
            }

            // 2. Kiểm tra chỉ số mảng kỹ năng
            if (_skills == null || skillIndex < 0 || skillIndex >= _skills.Length)
            {
                Debug.Log($"[PlayerServerCombat] Client {OwnerClientId} requested invalid skill index: {skillIndex}.");
                return;
            }

            SkillData skill = _skills[skillIndex];
            if (skill == null)
            {
                Debug.Log($"[PlayerServerCombat] Skill at index {skillIndex} is null.");
                return;
            }

            // 3. Kiểm tra Cooldown của kỹ năng
            if (Time.time < _nextSkillTimes[skillIndex])
            {
                Debug.Log($"[PlayerServerCombat] Skill '{skill.SkillName}' is on cooldown for Client {OwnerClientId}.");
                return;
            }

            // 4. Kiểm tra target hợp lệ (Lựa chọn A: nếu không có target hoặc target không hợp lệ thì bỏ qua)
            if (!targetRef.TryGet(out NetworkObject targetNetObj))
            {
                Debug.Log($"[PlayerServerCombat] Client {OwnerClientId} casted skill '{skill.SkillName}' rejected: No target selected.");
                return;
            }

            var enemyCombat = targetNetObj.GetComponent<EnemyServerCombat>();
            if (enemyCombat == null)
            {
                Debug.Log($"[PlayerServerCombat] Client {OwnerClientId} casted skill '{skill.SkillName}' rejected: Target does not have EnemyServerCombat component.");
                return;
            }

            // 5. Kiểm tra tài nguyên (MP)
            if (_networkData.CurrentMP.Value < skill.ManaCost)
            {
                Debug.Log($"[PlayerServerCombat] Client {OwnerClientId} does not have enough MP to cast '{skill.SkillName}'. Required: {skill.ManaCost}, Current: {_networkData.CurrentMP.Value}");
                return;
            }

            // Thực thi trừ MP, thiết lập cooldown và kích hoạt trạng thái tấn công trên FSM
            _nextSkillTimes[skillIndex] = Time.time + skill.Cooldown;
            _networkData.ModifyMP(-skill.ManaCost);

            // Gây sát thương lên quái vật thông qua EnemyServerCombat.TakeDamage
            float damageDealt = skill.BaseDamage;
            enemyCombat.TakeDamage(damageDealt);

            if (_stateMachine != null)
            {
                _stateMachine.TransitionTo(PlayerState.Attacking);
            }

            Debug.Log($"[PlayerServerCombat] Client {OwnerClientId} casted skill '{skill.SkillName}' successfully on {targetNetObj.name} and dealt {damageDealt} damage. Remaining MP: {_networkData.CurrentMP.Value}");
        }
    }
}
