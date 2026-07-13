namespace Shared
{
    /// <summary>
    /// Các trạng thái hành động của nhân vật chơi mạng được FSM quản lý.
    /// </summary>
    public enum PlayerState : byte
    {
        Idle,
        Locomotion,
        Attacking,
        Dead
    }
}
