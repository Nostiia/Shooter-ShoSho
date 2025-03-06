using Fusion;

public class Ball : NetworkBehaviour
{
    [Networked] private TickTimer Life { get; set; }

    public void Init()
    {
        Life = TickTimer.CreateFromSeconds(Runner, 10.0f);
    }

    public override void FixedUpdateNetwork()
    {
        if (Life.Expired(Runner))
            Runner.Despawn(Object);
        else
            transform.position += 1 * transform.up * Runner.DeltaTime;
    }
}
