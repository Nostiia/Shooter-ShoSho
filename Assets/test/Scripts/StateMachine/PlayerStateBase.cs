public abstract class PlayerStateBase
{
    protected Player _player;

    public PlayerStateBase(Player player)
    {
        _player = player;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}
