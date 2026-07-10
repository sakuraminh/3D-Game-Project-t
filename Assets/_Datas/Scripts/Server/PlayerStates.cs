using UnityEngine;

namespace Server
{
    /// <summary>
    /// Trạng thái Đứng yên (Idle). Chờ nhận input để chuyển sang di chuyển.
    /// </summary>
    public class PlayerIdleState : PlayerBaseState
    {
        public PlayerIdleState(PlayerStateMachine stateMachine) : base(stateMachine) {}

        public override void Update()
        {
            if (_stateMachine.NetworkHandler.MoveInput != Vector2.zero)
            {
                _stateMachine.TransitionTo(Shared.PlayerState.Locomotion);
            }
        }
    }

    /// <summary>
    /// Trạng thái Di chuyển (Locomotion).
    /// </summary>
    public class PlayerLocomotionState : PlayerBaseState
    {
        public PlayerLocomotionState(PlayerStateMachine stateMachine) : base(stateMachine) {}

        public override void Update()
        {
            if (_stateMachine.NetworkHandler.MoveInput == Vector2.zero)
            {
                _stateMachine.TransitionTo(Shared.PlayerState.Idle);
            }
        }
    }

    /// <summary>
    /// Trạng thái Tấn công (Attacking). Sau một khoảng thời gian tự động quay lại Idle/Locomotion.
    /// </summary>
    public class PlayerAttackingState : PlayerBaseState
    {
        private float _timer;

        public PlayerAttackingState(PlayerStateMachine stateMachine) : base(stateMachine) {}

        public override void Enter()
        {
            _timer = 0f;
        }

        public override void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= _stateMachine.AttackDuration)
            {
                if (_stateMachine.NetworkHandler.MoveInput != Vector2.zero)
                {
                    _stateMachine.TransitionTo(Shared.PlayerState.Locomotion);
                }
                else
                {
                    _stateMachine.TransitionTo(Shared.PlayerState.Idle);
                }
            }
        }
    }

    /// <summary>
    /// Trạng thái Chết (Dead). Khóa toàn bộ hoạt động.
    /// </summary>
    public class PlayerDeadState : PlayerBaseState
    {
        public PlayerDeadState(PlayerStateMachine stateMachine) : base(stateMachine) {}

        public override void Enter()
        {
            // Reset input khi chết để tránh tiếp tục trượt đi
            _stateMachine.NetworkHandler.ResetMoveInputOnServer();
        }
    }
}
