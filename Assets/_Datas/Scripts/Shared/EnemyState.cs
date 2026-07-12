namespace Shared
{
    /// <summary>
    /// Các trạng thái của quái vật được FSM quản lý trên Server và đồng bộ qua mạng.
    /// </summary>
    public enum EnemyState : byte
    {
        Idle,
        Chase,
        Attack,
        Dead
    }
}
