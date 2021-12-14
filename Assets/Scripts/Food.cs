using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour, IPickUpable
{
    Player player;

    float calories;
    float milliliters;

    public enum Type { Beans, Soda, Random }
    public Type type = Type.Random;

    private void Start()
    {
        player = Player.Instance;
        Initialize();
    }

    private void Initialize()
    {
        switch (type)
        {
            case Type.Random:
                int rand = Random.Range(0, (int)Type.Random);
                type = (Type)rand;
                Initialize();
                break;
            case Type.Beans:
                BeansSetup();
                break;
            case Type.Soda:
                SodaSetup();
                break;
        }
    }

    private void BeansSetup()
    {
        name = "Beans";
        calories = 350;
        milliliters = 150f;
    }

    private void SodaSetup()
    {
        name = "Soda";
        calories = 150;
        milliliters = 350;
    }

    public void Eat()
    {
        if(calories > milliliters)
        {
            if (player.vitals.calories < player.vitals.maxCalories - calories)
            {
                player.vitals.calories += calories;
                player.vitals.milliliters += milliliters;
            }
        }
        else
        {
            if (player.vitals.milliliters < player.vitals.maxMilliliters - milliliters)
            {
                player.vitals.milliliters += milliliters;
                player.vitals.calories += calories;
            }
        }
    }

    public void PickUp()
    {
        if (!player.food.Contains(gameObject))
        {
            player.food.Add(gameObject);
            gameObject.SetActive(false);
            transform.parent = player.transform;
        }
    }
}
