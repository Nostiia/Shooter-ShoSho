public abstract class PlayerStateBase
{
    protected Player _player;

    public PlayerStateBase(Player player)
    {
        _player = player;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}
