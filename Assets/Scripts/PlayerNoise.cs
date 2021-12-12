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
    float noiseSphereRadius;
    float pulseTime = .5f;
    float pulse;

    private void Start()
    {
        player = GetComponent<PlayerController>();
    }

    private void Update()
    {
        switch (player.state)
        {
            case PlayerController.State.Idle:
                noiseSphereRadius = idleRadius;
                break;
            case PlayerController.State.Walking:
                noiseSphereRadius = walkRadius;
                break;
            case PlayerController.State.Running:
                noiseSphereRadius = runRadius;
                break;
            case PlayerController.State.Crouching:
                noiseSphereRadius = crouchRadius;
                break;
        }

        pulse -= 1 * Time.deltaTime;
        if (pulse <= 0)
        {
            EmitSound();
            pulse = pulseTime;
        }
    }

    public void EmitSound()
    {
        Collider[] hitZombies = Physics.OverlapSphere(transform.position, noiseSphereRadius, 1 << LayerMask.NameToLayer("Zombie"));
        if (hitZombies.Length > 0)
        {
            foreach (Collider zombie in hitZombies)
                zombie.gameObject.GetComponent<Zombie>().ChaseTarget(gameObject);
        }
    }
}
