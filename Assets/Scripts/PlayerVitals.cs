using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVitals : MonoBehaviour
{
    Player player;
    public float maxMaxHealth = 100;
    public float maxHealth;
    public float health;
    public float maxMaxStamina = 100;
    public float maxStamina;
    public float stamina;
    public float maxCalories = 2000;
    public float calories;
    public float maxMilliliters = 2000;
    public float milliliters;
    public float exertion;
    public float baseExertion = 1;
    public float healthExertion = 1;
    public float staminaExertion = 1;

    public bool starving;
    public bool dehydrated;

    private void Start()
    {
        player = Player.Instance;
        maxHealth = maxMaxHealth;
        health = maxHealth;
        maxStamina = maxMaxStamina;
        stamina = maxStamina;
        calories = maxCalories / 2;
        milliliters = maxMilliliters / 2;
    }

    private void Update()
    {
        switch (player.movementState)
        {
            case Player.MovementState.Idle:
                break;
            case Player.MovementState.Walking:
                break;
            case Player.MovementState.Running:
                stamina -= 10 * Time.deltaTime;
                maxStamina -= .1f * Time.deltaTime;
                break;
            case Player.MovementState.Crouching:
                break;
        }

        if (health < maxHealth)
        {
            health += (calories / maxCalories) * Time.deltaTime;
            healthExertion = 1;
            health = Mathf.Clamp(health, 0, maxHealth);
        }
        else
            healthExertion = 0;

        if (stamina < maxStamina)
        {
            if (player.movementState != Player.MovementState.Running)
            {
                stamina += (calories / maxCalories) * Time.deltaTime;
                staminaExertion = 1;
            }
            stamina = Mathf.Clamp(stamina, 0, maxStamina);

        }
        else
            staminaExertion = 0;

        if (calories > 0)
        {
            exertion = (baseExertion + healthExertion + staminaExertion);
            calories -= exertion * Time.deltaTime;
            calories = Mathf.Clamp(calories, 0, maxCalories);
        }
        starving = (calories <= 0);
        if (starving)
            Starving();

        if (milliliters > 0)
        {
            exertion = (baseExertion + healthExertion + staminaExertion);
            milliliters -= exertion * Time.deltaTime;
            milliliters = Mathf.Clamp(milliliters, 0, maxMilliliters);
        }
        dehydrated = (milliliters <= 0);
        if (dehydrated)
            Dehydrated();

        if (health <= 0)
        {
            player.Die();
        }
    }
    void Starving()
    {
        health -= Time.deltaTime;
    }
    void Dehydrated()
    {
        health -= Time.deltaTime;
    }
}
