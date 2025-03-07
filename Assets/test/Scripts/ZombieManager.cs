using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieManager : NetworkBehaviour
{
    public float speed = 0.5f;
    private Vector2 target = Vector2.zero; // Center of scene

    void Update()
    {
        if (HasStateAuthority) // Move only if we have authority
        {
            transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
        }
    }
}
