public class RunningState : PlayerStateBase
{
    public RunningState(Player player) : base(player) { }

    public override void Enter()
    {
        _player.SetAnimation("isRunning", true);
    }

    public override void Update()
    {
        if (!_player.HasMovementInput())
        {
            _player.SetState(new IdleState(_player));
        }
        else
        {
            _player.MovePlayer();
        }
    }

    public override void Exit() { }
}

