using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public const byte MOUSEBUTTON1 = 2;
    public const byte SHOOT = 1;

    public NetworkButtons buttons;
    public Vector3 direction;
    public Vector3 shootDirection;
}
