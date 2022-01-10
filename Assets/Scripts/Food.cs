using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : Item
{
    Player player;
    public float calories;
    public float milliliters;
    public float eatingTime;

    private void Start()
    {
        name = displayName;
        goid = GetInstanceID().ToString();
        player = Player.Instance;
        descriptiveText = "Calories: " + calories + "\nmL: " + milliliters + "\nTime to eat: " + eatingTime;
        Load();
    }

    public void Eat()
    {
        player.vitals.calories += calories;
        player.vitals.milliliters += milliliters;
        gameObject.SetActive(false);
        transform.parent = null;
        player.RemoveItem(this, 0);
        Save();
    }
}
