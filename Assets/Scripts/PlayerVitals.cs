using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVitals : MonoBehaviour
{
    Player player;
    GameManager gameManager;
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
        gameManager = GameManager.Instance;
        if (ES3.KeyExists("startTime"))
        {
            long timeConversion = Convert.ToInt64(ES3.Load<string>("startTime"));
            startTime = DateTime.FromBinary(timeConversion);
        }
        else
        {
            startTime = DateTime.Now;
            ES3.Save("startTime", startTime.ToBinary().ToString());
        }
        maxHealth = ES3.Load("maxHealth", maxMaxHealth);
        health = ES3.Load("health", maxHealth);
        maxStamina = ES3.Load("maxStamina", maxMaxStamina);
        stamina = ES3.Load("stamina", maxStamina);
        calories = ES3.Load("calories", 1000f);
        milliliters = ES3.Load("milliliters", 1000f);
        if (ES3.KeyExists("logOffTime"))
        {
            long timeConversion = Convert.ToInt64(ES3.Load<string>("logOffTime"));
            logOffTime = DateTime.FromBinary(timeConversion);
            timeOffline = (float)DateTime.Now.Subtract(logOffTime).TotalSeconds;
            timeSurvived = logOffTime.Subtract(startTime);

            while (timeOffline > 0 && calories > 0 && milliliters > 0)
            {
                recuperation = 3;
                if (health < maxHealth)
                {
                    health += ((calories + milliliters) / (maxCalories + maxMilliliters)) * recuperation;
                    healthExertion = 1 * recuperation;
                    health = Mathf.Clamp(health, 0, maxHealth);
                }
                else
                    healthExertion = 0;

                if (stamina < maxStamina)
                {
                    if (player.movementState != Player.MovementState.Running)
                    {
                        stamina += ((calories + milliliters) / (maxCalories + maxMilliliters)) * recuperation;
                        staminaExertion = 1 * recuperation;
                    }
                    stamina = Mathf.Clamp(stamina, 0, maxStamina);

                }
                else
                    staminaExertion = 0;

                if (calories > 0)
                {
                    exertion = ((baseExertion + healthExertion + staminaExertion) * .023f);
                    calories -= exertion;
                    calories = Mathf.Clamp(calories, 0, maxCalories);
                }
                starving = (calories <= 0);
                if (starving)
                {
                    Starving();
                    break;
                }


                if (milliliters > 0)
                {
                    exertion = ((baseExertion + healthExertion + staminaExertion) * .023f);
                    milliliters -= exertion;
                    milliliters = Mathf.Clamp(milliliters, 0, maxMilliliters);
                }
                dehydrated = (milliliters <= 0);
                if (dehydrated)
                {
                    Dehydrated();
                    break;
                }
                timeSurvived += TimeSpan.FromSeconds(1);
                gameManager.timeSurvived = timeSurvived;
                timeOffline -= 1;
                if (timeOffline % 60 == 0)
                {
                    float deathChanceHourly = UnityEngine.Random.Range(0, 100);
                    print("Hourly death chance = " + deathChanceHourly);
                    //Broken
                    if (deathChanceHourly < player.danger)
                    {
                        player.Die();
                    }
                }
            }
        }
        float deathChanceFinal = UnityEngine.Random.Range(0, 100);
        print("Final death chance = " + deathChanceFinal);
        if (deathChanceFinal < player.danger)
        {
            player.Die();
        }
    }

    private void CalculateTimeLeft()
    {
        timeUntilStarving = (calories + player.caloriesInInventory) / .0003858f / 60 / 60 / 60;
        timeUntilDehydrated = (milliliters + player.millilitersInInventory) / .0003858f / 60 / 60 / 60;
    }

    private void Update()
    {
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
        else
        {
            timeSurvived = DateTime.Now.Subtract(startTime);
            CalculateTimeLeft();
        }


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
        ES3.Save("maxHealth", maxHealth);
        ES3.Save("health", health);
        ES3.Save("maxStamina", maxStamina);
        ES3.Save("stamina", stamina);
        ES3.Save("calories", calories);
        ES3.Save("milliliters", milliliters);
        ES3.Save("logOffTime", DateTime.Now.ToBinary().ToString());
    }
}
