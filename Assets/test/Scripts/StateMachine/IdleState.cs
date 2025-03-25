public class IdleState : PlayerStateBase
{
    public IdleState(Player player) : base(player) { }

    public override void Enter()
    {
        _player.SetAnimation("isRunning", false);
    }

    public override void Update()
    {
        if (_player.GetComponent<PlayerMovement>().HasMovementInput())
        {
            _player.SetState(new RunningState(_player));
        }
    }
}
