using UnityEngine;
using Shared;

namespace Server
{
    /// <summary>
    /// Trạng thái Đứng yên (Idle) của AI. Tìm kiếm mục tiêu trong tầm nhìn.
    /// </summary>
    public class EnemyIdleState : EnemyBaseState
    {
        private float _nextDetectionTime;

        public EnemyIdleState(EnemyStateMachine stateMachine) : base(stateMachine) {}

        public override void Enter()
        {
            if (_stateMachine.Movement != null)
            {
                _stateMachine.Movement.Stop();
            }
            _stateMachine.Target = null;
            _nextDetectionTime = 0f;
        }

        public override void Update()
        {
            // Tối ưu hiệu năng: Chỉ chạy dò tìm 5 lần/giây (mỗi 0.2s) thay vì mỗi khung hình (Update)
            if (Time.time < _nextDetectionTime) return;
            _nextDetectionTime = Time.time + 0.2f;

            var config = _stateMachine.NetworkData.Config;
            if (config == null) return;

            // Dò tìm người chơi trong bán kính DetectionRadius
            Collider[] colliders = Physics.OverlapSphere(_stateMachine.transform.position, config.DetectionRadius);
            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent<PlayerNetworkHandler>(out var player))
                {
                    // Chỉ săn đuổi nếu người chơi còn sống
                    if (player.CurrentState.Value != PlayerState.Dead)
                    {
                        _stateMachine.Target = player;
                        _stateMachine.TransitionTo(EnemyState.Chase);
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Trạng thái Đuổi theo (Chase). Di chuyển về phía người chơi mục tiêu.
    /// </summary>
    public class EnemyChaseState : EnemyBaseState
    {
        public EnemyChaseState(EnemyStateMachine stateMachine) : base(stateMachine) {}

        public override void Update()
        {
            var target = _stateMachine.Target;
            var config = _stateMachine.NetworkData.Config;

            // Nếu mục tiêu mất hoặc đã chết, quay về Idle
            if (target == null || target.CurrentState.Value == PlayerState.Dead || config == null)
            {
                _stateMachine.TransitionTo(EnemyState.Idle);
                return;
            }

            float distance = Vector3.Distance(_stateMachine.transform.position, target.transform.position);

            // Kiểm tra nếu mục tiêu ngoài tầm nhìn
            if (distance > config.DetectionRadius * 1.25f)
            {
                _stateMachine.TransitionTo(EnemyState.Idle);
                return;
            }

            // Kiểm tra nếu mục tiêu trong tầm tấn công VÀ cooldown tấn công đã sẵn sàng
            if (distance <= config.AttackRange && Time.time >= _stateMachine.NextAttackTime)
            {
                _stateMachine.TransitionTo(EnemyState.Attack);
                return;
            }

            // Di chuyển về phía mục tiêu
            if (_stateMachine.Movement != null)
            {
                _stateMachine.Movement.MoveTo(target.transform.position);
            }
        }
    }

    /// <summary>
    /// Trạng thái Tấn công (Attack). Thực thi đòn đánh và quay lại Chase ngay lập tức để duy trì bám đuổi mượt mà.
    /// </summary>
    public class EnemyAttackState : EnemyBaseState
    {
        public EnemyAttackState(EnemyStateMachine stateMachine) : base(stateMachine) {}

        public override void Enter()
        {
            var target = _stateMachine.Target;
            var config = _stateMachine.NetworkData.Config;

            if (_stateMachine.Movement != null)
            {
                _stateMachine.Movement.Stop();
            }

            if (target != null && config != null)
            {
                // Quay mặt về phía người chơi mục tiêu ngay lập tức để thực hiện đòn đánh
                Vector3 direction = (target.transform.position - _stateMachine.transform.position).normalized;
                direction.y = 0f;
                if (direction != Vector3.zero)
                {
                    _stateMachine.transform.rotation = Quaternion.LookRotation(direction);
                }

                // Thực thi đòn đánh vật lý lên mục tiêu
                if (_stateMachine.Combat != null)
                {
                    _stateMachine.Combat.PerformAttack(target);
                }

                // Thiết lập cooldown thời gian cho đợt tấn công kế tiếp
                _stateMachine.NextAttackTime = Time.time + config.AttackCooldown;
            }

            // Quay trở lại trạng thái Chase ngay lập tức để bám đuổi mượt mà (tránh khựng đơ AI)
            _stateMachine.TransitionTo(EnemyState.Chase);
        }

        public override void Update()
        {
            // Trạng thái tự động chuyển về Chase tại Enter nên Update không cần xử lý.
            // Để an toàn, nếu có lỗi xảy ra vẫn cho tự động chuyển về Chase.
            _stateMachine.TransitionTo(EnemyState.Chase);
        }
    }

    /// <summary>
    /// Trạng thái Chết (Dead). Khóa toàn bộ AI và NavMesh.
    /// </summary>
    public class EnemyDeadState : EnemyBaseState
    {
        public EnemyDeadState(EnemyStateMachine stateMachine) : base(stateMachine) {}

        public override void Enter()
        {
            if (_stateMachine.Movement != null)
            {
                _stateMachine.Movement.Stop();
            }
            _stateMachine.Target = null;

            // Khởi chạy Coroutine để thu hồi quái vật về lại Pool sau 2 giây trễ (chỉ trên Server)
            _stateMachine.StartCoroutine(DespawnAfterDelay(2f));
        }

        private System.Collections.IEnumerator DespawnAfterDelay(float delay)
        {
            yield return new UnityEngine.WaitForSeconds(delay);

            if (_stateMachine.NetworkObject != null && _stateMachine.NetworkObject.IsSpawned)
            {
                _stateMachine.NetworkObject.Despawn();
            }
        }
    }
}
