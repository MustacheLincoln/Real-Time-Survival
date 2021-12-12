using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour, IDamageable<float>
{
    NavMeshAgent navMeshAgent;
    FieldOfView fov;

    float walkSpeed = 1;
    float runSpeed = 3;
    float turnSpeed = 1;
    float attackSpeed = .5f;
    float windUp;
    float maxHealth = 100;
    float health;
    float attackDamage = 5;
    float wanderTime = 20;
    float wandering;
    float chaseTime = 10;
    float chasing;
    Quaternion newRotation;
    public enum State { Wander, Chase }
    public State state;

    //Temp
    //public GameObject player;


    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        fov = GetComponent<FieldOfView>();
        windUp = attackSpeed;
        health = maxHealth;
        navMeshAgent.speed = runSpeed;
        state = State.Wander;
        transform.rotation = Quaternion.Euler(0, Random.Range(0,360), 0);
    }

    private void Update()
    {
        switch (state)
        {
            case State.Wander:
                Wander();
                break;
            case State.Chase:
                Chase();
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
        chasing -= Time.deltaTime;
        if (chasing <= 0)
        {
            state = State.Wander;
        }
    }

    public void StartChase(GameObject target)
    {
        navMeshAgent.destination = target.transform.position;
        state = State.Chase;
        chasing = chaseTime + Random.Range(-3, 3); ;
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Destructable" || other.gameObject.name == "Player")
        {
            windUp -= 1 * Time.deltaTime;
            if (windUp <= 0)
            {
                other.GetComponent<IDamageable<float>>().TakeDamage(attackDamage);
                windUp = attackSpeed;
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Destructable" || other.gameObject.name == "Player")
            windUp = attackSpeed;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
    }
}
