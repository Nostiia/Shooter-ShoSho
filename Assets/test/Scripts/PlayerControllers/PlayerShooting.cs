using Fusion;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] private PhysxBall _prefabPhysxBall;
    [SerializeField] private WeaponManager _weaponManager;
    private AmmoCount _ammoCounter;
    private Player _player;
    [SerializeField] private SpriteRenderer _weaponRenderer;
    [SerializeField] private SpriteRenderer _bodyRenderer;
    private Vector3 _shootDirection;

    [Networked] private TickTimer _delay { get; set; }

    private void Awake()
    {
        _player = transform.GetComponent<Player>();
        _ammoCounter = _player.transform.GetComponent<AmmoCount>();
    }

    public void HandleShooting()
    {
        if (_player.GetInput(out NetworkInputData data) && _player.HasStateAuthority && _delay.ExpiredOrNotRunning(Runner) && _ammoCounter.GetCurrentAmmo() > 0)
        {
            if (data.shootDirection.sqrMagnitude > 0)
            {
                _shootDirection = data.shootDirection;

                float angle = Mathf.Atan2(_shootDirection.y, _shootDirection.x) * Mathf.Rad2Deg;
                float angleY = 0;
                if (Vector2.Dot(transform.right, _shootDirection) < 0)
                {
                    _bodyRenderer.transform.localRotation = Quaternion.Euler(0, 180, 0);
                    angleY = 180;
                    angle = -angle;
                }
                else
                {
                    _bodyRenderer.transform.localRotation = Quaternion.identity;
                }
                _weaponRenderer.transform.rotation = Quaternion.Euler(angleY, 0, angle);
            }

            if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1) || data.buttons.IsSet(NetworkInputData.SHOOT))
            {
                data.shootDirection.Normalize();
                FireWeapon(data.shootDirection);
            }
        }
    }

    private void FireWeapon(Vector3 shootDirection)
    {
        _delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
        _ammoCounter.DecrementAmmo();


        Runner.Spawn(_prefabPhysxBall,
                     transform.position + shootDirection,
                     Quaternion.LookRotation(shootDirection),
                     Object.InputAuthority,
                     (runner, o) => o.GetComponent<PhysxBall>().Init(10 * shootDirection, _player));

        if (_weaponManager.GetWeaponIndex() == 1)
        {
            Vector3 rotatedForwardLeft = Quaternion.Euler(0, 0, 10) * shootDirection;
            Vector3 rotatedForwardRight = Quaternion.Euler(0, 0, -10) * shootDirection;

            Runner.Spawn(_prefabPhysxBall, transform.position + rotatedForwardLeft, Quaternion.LookRotation(rotatedForwardLeft), Object.InputAuthority,
                        (runner, o) => o.GetComponent<PhysxBall>().Init(10 * rotatedForwardLeft, _player));

            Runner.Spawn(_prefabPhysxBall, transform.position + rotatedForwardRight, Quaternion.LookRotation(rotatedForwardRight), Object.InputAuthority,
                        (runner, o) => o.GetComponent<PhysxBall>().Init(10 * rotatedForwardRight, _player));
        }
    }

    public int GetWeaponIndex()
    {
        return _weaponManager.GetWeaponIndex();
    }

    public void InvisibleWeapon()
    {
        _weaponRenderer.enabled = false;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_MakeWeaponInvisible()
    {
        InvisibleWeapon(); 
    }
}
