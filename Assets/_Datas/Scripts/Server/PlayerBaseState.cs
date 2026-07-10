namespace Server
{
    /// <summary>
    /// Lớp cơ sở (Abstract State) định nghĩa hành vi và giao diện cho các trạng thái cụ thể của Player.
    /// </summary>
    public abstract class PlayerBaseState
    {
        protected PlayerStateMachine _stateMachine;

        protected PlayerBaseState(PlayerStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public virtual void Enter() {}
        public virtual void Update() {}
        public virtual void Exit() {}
    }
}
