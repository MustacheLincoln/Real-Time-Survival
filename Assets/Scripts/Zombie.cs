using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour, IDamageable<float>
{
    NavMeshAgent navMeshAgent;

    float walkSpeed = 1;
    float runSpeed = 2;
    float attackSpeed = .5f;
    float windUp;
    float maxHealth = 100;
    float health;
    float attackDamage = 10;

    //Temp
    public GameObject player;


    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        windUp = attackSpeed;
        health = maxHealth;
        navMeshAgent.speed = runSpeed;
    }

    private void Update()
    {
        //Temp
        if(player)
            navMeshAgent.destination = player.transform.position;
    }

    public void ChaseTarget(GameObject target)
    {
        navMeshAgent.destination = target.transform.position;
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
