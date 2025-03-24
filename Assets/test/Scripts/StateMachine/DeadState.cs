public class DeadState : PlayerStateBase
{
    public DeadState(Player player) : base(player) { }

    public override void Enter()
    {
        _player.SetAnimation("isRunning", false);
        _player.SetAnimation("isDied", true);
        _player.GetComponent<PlayerMovement>().DisableMovement();
        _player.GetComponent<PlayerShooting>().Rpc_MakeWeaponInvisible();
    }
}

