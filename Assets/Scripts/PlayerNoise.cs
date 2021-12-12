using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNoise : MonoBehaviour
{
    SphereCollider noiseSphere;
    PlayerController player;
    float idleRadius = 2;
    float walkRadius = 5;
    float runRadius = 10;
    float crouchRadius = 3;

    private void Start()
    {
        player = GetComponent<PlayerController>();
        noiseSphere = GetComponent<SphereCollider>();
    }

    private void Update()
    {
        switch (player.state)
        {
            case PlayerController.State.Idle:
                noiseSphere.radius = idleRadius;
                break;
            case PlayerController.State.Walking:
                noiseSphere.radius = walkRadius;
                break;
            case PlayerController.State.Running:
                noiseSphere.radius = runRadius;
                break;
            case PlayerController.State.Crouching:
                noiseSphere.radius = crouchRadius;
                break;
        }
    }
}
