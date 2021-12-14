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
    float speed = .5f;
    Vector3 newPosition;
    Quaternion newRotation;
    public bool isMoving;

    private void Start()
    {
        name = "Door";
        navMeshObstacle = GetComponent<NavMeshObstacle>();
        health = maxHealth;
        state = State.Closed;
        newPosition = transform.position;
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, newPosition, speed);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, speed);

        isMoving = (transform.position != newPosition);

        if (health <= 0)
            Destroy(gameObject);
    }

    public void Interact()
    {
        if (isMoving == false)
            switch (state)
            {
                case State.Closed:
                    state = State.Open;
                    newRotation = transform.rotation * Quaternion.Euler(0, 90, 0);
                    newPosition = transform.position + new Vector3(.75f, 0, .75f);
                    navMeshObstacle.enabled = false;
                    break;
                case State.Open:
                    state = State.Closed;
                    newRotation = transform.rotation * Quaternion.Euler(0, -90, 0);
                    newPosition = transform.position + new Vector3(-.75f, 0, -.75f);
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
