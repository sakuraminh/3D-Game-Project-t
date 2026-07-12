namespace Server
{
    /// <summary>
    /// Lớp cơ sở (Abstract State) định nghĩa hành vi và vòng đời cho các trạng thái của Enemy AI.
    /// </summary>
    public abstract class EnemyBaseState
    {
        protected EnemyStateMachine _stateMachine;

        protected EnemyBaseState(EnemyStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public virtual void Enter() {}
        public virtual void Update() {}
        public virtual void Exit() {}
    }
}
