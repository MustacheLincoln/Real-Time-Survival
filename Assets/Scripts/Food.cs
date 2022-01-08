using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : Item
{
    Player player;
    public float calories;
    public float milliliters;
    public float eatingTime;

    public enum Type { Beans, Soda, Random }
    public Type type = Type.Random;

    private void Start()
    {
        name = displayName;
        goid = GetInstanceID().ToString();
        player = Player.Instance;
        type = ES3.Load(goid + "type", type);
        Initialize();
        Load();
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

    private void Load()
    {
        gameObject.SetActive(ES3.Load(goid + "activeSelf", true));
        if (player.items.Contains(this))
        {
            gameObject.layer = 0;
            transform.position = player.transform.position;
            transform.parent = player.transform;
        }
        else
        {
            transform.position = ES3.Load(goid + "position", transform.position);
            transform.rotation = ES3.Load(goid + "rotation", transform.rotation);
        }
    }

    public override void Save()
    {
        if (player)
        {
            ES3.Save(goid + "type", type);
            ES3.Save(goid + "activeSelf", gameObject.activeSelf);
            ES3.Save(goid + "position", transform.position);
            ES3.Save(goid + "rotation", transform.rotation);
        }
    }

    private void BeansSetup()
    {
        name = "Beans";
        calories = 350;
        milliliters = 150f;
        eatingTime = 2;
        descriptiveText = "Calories: " + calories + "\nmL: " + milliliters + "\nTime to eat: " + eatingTime;
    }

    private void SodaSetup()
    {
        name = "Soda";
        calories = 150;
        milliliters = 350;
        eatingTime = 1;
        descriptiveText = "Calories: " + calories + "\nmL: " + milliliters + "\nTime to drink: " + eatingTime;
    }

    public void Eat()
    {
        player.vitals.calories += calories;
        player.vitals.milliliters += milliliters;
        gameObject.SetActive(false);
        transform.parent = null;
        Save();
    }

    public override void PickUp()
    {
        if (!player.items.Contains(this))
        {
            gameObject.layer = 0;
            transform.position = player.transform.position;
            transform.parent = player.transform;
            AddToInventory();
        }
    }
}
