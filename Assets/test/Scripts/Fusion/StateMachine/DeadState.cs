public class DeadState : PlayerStateBase
{
    public DeadState(Player player) : base(player) { }

    public override void Enter()
    {
        _player.SetAnimation("isRunning", false);
        _player.SetAnimation("isDead", true);
        _player.DisableMovement();
    }

    public override void Update() { }

    public override void Exit() { }
}

