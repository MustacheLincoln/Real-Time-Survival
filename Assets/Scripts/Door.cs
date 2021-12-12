using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IDamageable<float>
{
    float maxHealth = 100;
    public float health;

    private void Start()
    {
        health = maxHealth;
    }

    private void Update()
    {
        if (health <= 0)
            Destroy(gameObject);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
    }
}
