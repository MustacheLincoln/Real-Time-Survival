using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : Item
{
    public float calories;
    public float milliliters;
    public float eatingTime;

    private void Start()
    {
        name = displayName;
        nameForInspected = displayName;
        goid = GetInstanceID().ToString();
        descriptiveText = "Calories: " + calories + "\nmL: " + milliliters + "\nTime to eat: " + eatingTime;
        Load();
    }

    public override void Use(Player owner)
    {
        StartCoroutine(owner.Eat(this));
    }
}
