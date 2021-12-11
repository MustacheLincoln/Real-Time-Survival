using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVitals : MonoBehaviour
{
    public float maxMaxHealth = 100;
    public float maxHealth;
    public float health;
    public float maxMaxStamina = 100;
    public float maxStamina;
    public float stamina;
    public float maxCalories = 2000;
    public float calories;
    public float exertion;
    public float baseExertion = 1;

    private void Start()
    {
        maxHealth = maxMaxHealth;
        health = maxHealth;
        maxStamina = maxMaxStamina;
        stamina = maxStamina;
        calories = maxCalories;
        exertion = baseExertion;
    }

    private void Update()
    {
        if (health < maxHealth)
        {
            health += (calories/maxCalories) * Time.deltaTime;
            health = Mathf.Clamp(health, 0, maxHealth);
        }

        if (stamina < maxStamina)
        {
            stamina += (calories / maxCalories) * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0, maxStamina);
        }

        if (calories > 0)
        {
            calories -= exertion * Time.deltaTime;
            calories = Mathf.Clamp(calories, 0, maxCalories);
        }
    }

    public void Eat(float cals)
    {
        calories += cals;
    }
}
