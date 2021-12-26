using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour, IDamageable<float>
{
    NavMeshAgent navMeshAgent;
    FieldOfView fov;
    Animator animator;
    Player player;
    public Transform spawnPoint;

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
    bool coolingDown;

    float fovRadius = 8;
    float fovAngle = 120;

    Quaternion newRotation;
    public enum State { Wander, Chase, Investigate }
    public State state;
    private Vector3 currentPos;
    private bool isMoving;
    private Vector3 lastPos;
    public string goid;

    private void Start()
    {
        player = Player.Instance;
        goid = GetInstanceID().ToString();
        name = "Zombie";
        navMeshAgent = GetComponent<NavMeshAgent>();
        fov = GetComponent<FieldOfView>();
        animator = GetComponentInChildren<Animator>();
        fov.radius = fovRadius;
        fov.angle = fovAngle;
        fov.targetMask = LayerMask.GetMask("Player");
        attackCooldown = attackSpeed;
        navMeshAgent.speed = runSpeed;
        transform.rotation = Quaternion.Euler(0, Random.Range(0,360), 0);

        navMeshAgent.Warp(ES3.Load(goid + "position", transform.position));
        transform.rotation = ES3.Load(goid + "rotation", transform.rotation);
        health = ES3.Load(goid + "health", maxHealth);
        navMeshAgent.destination = ES3.Load(goid + "destination", navMeshAgent.destination);
        navMeshAgent.speed = ES3.Load(goid + "speed", navMeshAgent.speed);
        state = ES3.Load(goid + "state", State.Wander);
        chasing = ES3.Load(goid + "chasing", chasing);
    }

    private void Update()
    {
        CalculateIsMoving();
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
            Die();

        Animate();
    }

    private void CalculateIsMoving()
    {
        currentPos = transform.position;
        isMoving = (currentPos != lastPos);
        lastPos = currentPos;
    }

    private void Animate()
    {
        animator.SetBool("isWalking", isMoving);
        animator.speed = walkSpeed / 3 + .25f;
        switch (state)
        {
            case State.Wander:
                animator.speed = walkSpeed/2 + .25f;
                break;
            case State.Chase:
                animator.speed = runSpeed/2;
                break;
            case State.Investigate:
                animator.speed = investigateSpeed/2;
                break;
        }
    }

    private void Die()
    {
        navMeshAgent.ResetPath();
        navMeshAgent.Warp(spawnPoint.position);
        health = maxHealth;
        state = State.Wander;
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
        StartCoroutine(EmitNoise());
        chasing -= Time.deltaTime;
        if (chasing <= 0)
        {
            state = State.Wander;
        }
    }

    private void Investigate()
    {
        StartCoroutine(EmitNoise());
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
        if (state == State.Chase)
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

    private IEnumerator EmitNoise()
    {
        if (coolingDown == false)
        {
            coolingDown = true;
            Collider[] hitZombies = Physics.OverlapSphere(transform.position, noiseSphereRadius, 1 << LayerMask.NameToLayer("Zombie"));
            if (hitZombies.Length > 0)
            {
                foreach (Collider zombie in hitZombies)
                    if (zombie.gameObject != this.gameObject)
                    {
                        Zombie z = zombie.GetComponent<Zombie>();
                        if (z.state != State.Chase)
                            z.StartInvestigating(navMeshAgent.destination);
                    }
            }
            yield return new WaitForSeconds(noiseCooldown);
            coolingDown = false;
        }
    }

    private void OnApplicationQuit()
    {
        if (player)
        {
            ES3.Save(goid + "position", transform.position);
            ES3.Save(goid + "rotation", transform.rotation);
            ES3.Save(goid + "health", health);
            ES3.Save(goid + "destination", navMeshAgent.destination);
            ES3.Save(goid + "speed", navMeshAgent.speed);
            ES3.Save(goid + "state", state);
            ES3.Save(goid + "chasing", chasing += 10);
        }
    }
}
