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

    private void Load()
    {
        gameObject.SetActive(ES3.Load(goid + "activeSelf", true));
        transform.parent = ES3.Load(goid + "parent", transform.parent);
        transform.localPosition = ES3.Load(goid + "position", transform.localPosition);
        transform.localRotation = ES3.Load(goid + "rotation", transform.localRotation);
    }

    public override void Save()
    {
        if (player)
        {
            ES3.Save(goid + "activeSelf", gameObject.activeSelf);
            ES3.Save(goid + "parent", transform.parent);
            ES3.Save(goid + "position", transform.localPosition);
            ES3.Save(goid + "rotation", transform.localRotation);
        }
    }

    public void Eat()
    {
        player.vitals.calories += calories;
        player.vitals.milliliters += milliliters;
        gameObject.SetActive(false);
        transform.parent = null;
        if (player.items.Contains(this))
            player.items.Remove(this);
        Save();
    }

    public override void PickUp()
    {
        if (!player.items.Contains(this))
        {
            transform.position = player.transform.position;
            transform.parent = player.transform;
            AddToInventory();
        }
    }
}
