using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Door : MonoBehaviour, IDamageable<float>
{
    NavMeshObstacle navMeshObstacle;
    float maxHealth = 100;
    public float health;
    public enum State { Open, Closed }
    public State state;

    private void Start()
    {
        navMeshObstacle = GetComponent<NavMeshObstacle>();
        health = maxHealth;
        state = State.Closed;
    }

    private void Update()
    {
        if (health <= 0)
            Destroy(gameObject);
    }

    public void Interact()
    {
        switch (state)
        {
            case State.Closed:
                state = State.Open;
                transform.rotation = transform.rotation * Quaternion.Euler(0, 90, 0);
                transform.position = transform.position + new Vector3(.75f, 0, .75f);
                navMeshObstacle.enabled = false;
                break;
            case State.Open:
                state = State.Closed;
                transform.rotation = transform.rotation * Quaternion.Euler(0, -90, 0);
                transform.position = transform.position + new Vector3(-.75f, 0, -.75f);
                navMeshObstacle.enabled = true;
                break;
        }
    }

    public void TakeDamage(float damage)
    {
        if (state == State.Closed)
            health -= damage;
    }
}
