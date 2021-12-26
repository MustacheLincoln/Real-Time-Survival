using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Door : MonoBehaviour, IDamageable<float>
{
    Player player;
    string goid;
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
        player = Player.Instance;
        goid = GetInstanceID().ToString();
        name = "Door";
        navMeshObstacle = GetComponent<NavMeshObstacle>();
        health = maxHealth;
        state = ES3.Load(goid + "state", State.Closed);
        transform.position = ES3.Load(goid + "position", transform.position);
        transform.rotation = ES3.Load(goid + "rotation", transform.rotation);
        gameObject.SetActive(ES3.Load(goid + "activeSelf", true));
        health = ES3.Load(goid + "health", maxHealth);
        newPosition = transform.position;
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, newPosition, speed);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, speed);

        isMoving = (transform.position != newPosition);

        if (health <= 0)
        {
            gameObject.SetActive(false);
            ES3.Save(goid + "activeSelf", gameObject.activeSelf);
        }
    }

    public void Interact()
    {
        if (isMoving == false)
            switch (state)
            {
                case State.Closed:
                    state = State.Open;
                    newRotation = transform.rotation * Quaternion.Euler(0, 90, 0);
                    newPosition = transform.position + new Vector3(1.25f, 0, 1.25f);
                    //navMeshObstacle.enabled = false;
                    break;
                case State.Open:
                    state = State.Closed;
                    newRotation = transform.rotation * Quaternion.Euler(0, -90, 0);
                    newPosition = transform.position + new Vector3(-1.25f, 0, -1.25f);
                    //navMeshObstacle.enabled = true;
                    break;
            }
    }

    public void TakeDamage(float damage)
    {
        if (state == State.Closed)
            health -= damage;
    }


    private void OnApplicationQuit()
    {
        if (player)
        {
            ES3.Save(goid + "state", state);
            ES3.Save(goid + "position", transform.position);
            ES3.Save(goid + "rotation", transform.rotation);
            ES3.Save(goid + "health", health);
        }
    }
}
