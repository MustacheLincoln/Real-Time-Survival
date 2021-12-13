using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour, IDamageable<float>, INoiseEmittable
{
    NavMeshAgent navMeshAgent;
    FieldOfView fov;

    float walkSpeed = 1;
    float investigateSpeed = 2;
    float runSpeed = 3;
    float turnSpeed = 1;
    float attackSpeed = .5f;
    float attackCooldown;
    float maxHealth = 100;
    float health;
    float attackDamage = 5;
    float noiseSphereRadius = 5;
    float wanderTime = 20;
    float wandering;
    float chaseTime = 10;
    float chasing;
    float noiseCooldown = 15;
    float noiseCooldownRemaining;

    float fovRadius = 8;
    float fovAngle = 120;

    Quaternion newRotation;
    public enum State { Wander, Chase, Investigate }
    public State state;

    //Temp
    //public GameObject player;


    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        fov = GetComponent<FieldOfView>();
        fov.radius = fovRadius;
        fov.angle = fovAngle;
        fov.targetMask = LayerMask.GetMask("Player");
        attackCooldown = attackSpeed;
        health = maxHealth;
        navMeshAgent.speed = runSpeed;
        state = State.Wander;
        transform.rotation = Quaternion.Euler(0, Random.Range(0,360), 0);
    }

    private void Update()
    {
        noiseCooldownRemaining -= Time.deltaTime;

        switch (state)
        {
            case State.Wander:
                Wander();
                break;
            case State.Chase:
                Chase();
                break;
            case State.Investigate:
                Investigate();
                break;
        }

        if (fov.target)
            StartChase(fov.target);

        if (health <= 0)
            Destroy(gameObject);
    }

    private void Wander()
    {
        wandering -= Time.deltaTime;
        if (wandering <= 0)
        {
            wandering = wanderTime + Random.Range(-10, 10);
            newRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            walkSpeed = Random.Range(0f, 1f);
            if (walkSpeed < .25f)
                walkSpeed = 0;
        }
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, turnSpeed * Time.deltaTime);
        navMeshAgent.ResetPath();
        navMeshAgent.Move(transform.forward * walkSpeed * Time.deltaTime);
    }

    private void Chase()
    {
        EmitNoise();
        chasing -= Time.deltaTime;
        if (chasing <= 0)
        {
            state = State.Wander;
        }
    }

    private void Investigate()
    {
        EmitNoise();
        chasing -= Time.deltaTime;
        if (chasing <= 0)
        {
            state = State.Wander;
        }
    }

    public void StartChase(GameObject target)
    {
        navMeshAgent.speed = runSpeed;
        navMeshAgent.destination = target.transform.position;
        state = State.Chase;
        chasing = chaseTime + Random.Range(-3, 3);
    }

    public void StartInvestigating(Vector3 pointOfInterest)
    {
        navMeshAgent.speed = investigateSpeed;
        navMeshAgent.destination = pointOfInterest;
        state = State.Investigate;
        chasing = chaseTime + Random.Range(-3, 3);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Destructable" || other.gameObject.name == "Player")
        {
            attackCooldown -= Time.deltaTime;
            if (attackCooldown <= 0)
            {
                other.GetComponent<IDamageable<float>>().TakeDamage(attackDamage);
                attackCooldown = attackSpeed;
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Destructable" || other.gameObject.name == "Player")
            attackCooldown = attackSpeed;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
    }

    public void EmitNoise()
    {
        if (noiseCooldownRemaining <= 0)
        {
            Collider[] hitZombies = Physics.OverlapSphere(transform.position, noiseSphereRadius, 1 << LayerMask.NameToLayer("Zombie"));
            if (hitZombies.Length > 0)
            {
                foreach (Collider zombie in hitZombies)
                    if (zombie.gameObject != this.gameObject)
                        zombie.gameObject.GetComponent<Zombie>().StartInvestigating(navMeshAgent.destination);
            }
            noiseCooldownRemaining = noiseCooldown;
        }
    }
}
