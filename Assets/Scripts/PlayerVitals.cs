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
    public float timeUntilStarving;
    public float timeUntilDehydrated;
    public float exertion;
    public float baseExertion = 1;
    public float healthExertion = 1;
    public float staminaExertion = 1;
    public float recuperation = 1;

    public bool starving;
    public bool dehydrated;

    DateTime startTime;
    DateTime logOffTime;
    public TimeSpan timeSurvived;
    float timeOffline;

    private void Start()
    {
        player = Player.Instance;
        if (PlayerPrefs.HasKey("startTime"))
        {
            long timeConversion = Convert.ToInt64(PlayerPrefs.GetString("startTime"));
            startTime = DateTime.FromBinary(timeConversion);
        }
        else
        {
            startTime = DateTime.Now;
            PlayerPrefs.SetString("startTime", startTime.ToBinary().ToString());
        }
        maxHealth = PlayerPrefs.GetFloat("maxHealth", maxMaxHealth);
        health = PlayerPrefs.GetFloat("health", maxHealth);
        maxStamina = PlayerPrefs.GetFloat("maxStamina", maxMaxStamina);
        stamina = PlayerPrefs.GetFloat("stamina", maxStamina);
        calories = PlayerPrefs.GetFloat("calories", 1000);
        milliliters = PlayerPrefs.GetFloat("milliliters", 1000);
        if (PlayerPrefs.HasKey("logOffTime"))
        {
            long timeConversion = Convert.ToInt64(PlayerPrefs.GetString("logOffTime"));
            logOffTime = DateTime.FromBinary(timeConversion);
            timeOffline = (float)DateTime.Now.Subtract(logOffTime).TotalSeconds;

            if (health < maxHealth)
            {
                health += ((calories + milliliters) / (maxCalories + maxMilliliters)) * 3 * timeOffline;
                health = Mathf.Clamp(health, 0, maxHealth);
            }

            if (stamina < maxStamina)
            {
                stamina += ((calories + milliliters) / (maxCalories + maxMilliliters)) * 3 * timeOffline;
                stamina = Mathf.Clamp(stamina, 0, maxStamina);
            }

            if (calories > 0)
            {
                exertion = ((baseExertion + healthExertion + staminaExertion) * .023f * timeOffline);
                calories -= exertion;
                calories = Mathf.Clamp(calories, 0, maxCalories);
            }
            starving = (calories <= 0);
            if (starving)
                Starving();

            if (milliliters > 0)
            {
                exertion = ((baseExertion + healthExertion + staminaExertion) * .023f * timeOffline);
                milliliters -= exertion;
                milliliters = Mathf.Clamp(milliliters, 0, maxMilliliters);
            }
            dehydrated = (milliliters <= 0);
            if (dehydrated)
                Dehydrated();
        }
    }

    private void CalculateTimeLeft()
    {
        timeUntilStarving = (calories + player.caloriesInInventory) / .000385f / 60 / 60 / 60;
        timeUntilDehydrated = (milliliters + player.millilitersInInventory) / .000385f / 60 / 60 / 60;
    }

    private void Update()
    {
        timeSurvived = DateTime.Now.Subtract(startTime);
        switch (player.movementState)
        {
            case Player.MovementState.Idle:
                recuperation = 3;
                break;
            case Player.MovementState.Walking:
                recuperation = 1;
                break;
            case Player.MovementState.Running:
                recuperation = 0;
                stamina -= 10 * Time.deltaTime;
                maxStamina -= .1f * Time.deltaTime;
                break;
            case Player.MovementState.Crouching:
                recuperation = 2;
                break;
        }

        if (health < maxHealth)
        {
            health += ((calories + milliliters) / (maxCalories + maxMilliliters)) * recuperation * Time.deltaTime;
            healthExertion = 1 * recuperation;
            health = Mathf.Clamp(health, 0, maxHealth);
        }
        else
            healthExertion = 0;

        if (stamina < maxStamina)
        {
            if (player.movementState != Player.MovementState.Running)
            {
                stamina += ((calories+milliliters) / (maxCalories+maxMilliliters)) * recuperation * Time.deltaTime;
                staminaExertion = 1 * recuperation;
            }
            stamina = Mathf.Clamp(stamina, 0, maxStamina);

        }
        else
            staminaExertion = 0;

        if (calories > 0)
        {
            exertion = ((baseExertion + healthExertion + staminaExertion) * .023f * Time.deltaTime);
            calories -= exertion;
            calories = Mathf.Clamp(calories, 0, maxCalories);
        }
        starving = (calories <= 0);
        if (starving)
            Starving();

        if (milliliters > 0)
        {
            exertion = ((baseExertion + healthExertion + staminaExertion) * .023f * Time.deltaTime);
            milliliters -= exertion;
            milliliters = Mathf.Clamp(milliliters, 0, maxMilliliters);
        }
        dehydrated = (milliliters <= 0);
        if (dehydrated)
            Dehydrated();

        if (health <= 0)
        {
            player.Die();
        }

        CalculateTimeLeft();
    }
    void Starving()
    {
        player.Die();
    }
    void Dehydrated()
    {
        player.Die();
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetFloat("maxHealth", maxHealth);
        PlayerPrefs.SetFloat("health", health);
        PlayerPrefs.SetFloat("maxStamina", maxStamina);
        PlayerPrefs.SetFloat("stamina", stamina);
        PlayerPrefs.SetFloat("calories", calories);
        PlayerPrefs.SetFloat("milliliters", milliliters);
        PlayerPrefs.SetString("logOffTime", DateTime.Now.ToBinary().ToString());
    }
}
