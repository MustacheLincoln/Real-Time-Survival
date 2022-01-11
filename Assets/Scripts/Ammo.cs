using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : Item
{
    Player player;
    public int amount;
    public int maxAmount;

    public enum AmmoType { Rifle, Pistol }
    public AmmoType ammoType;

    private void Start()
    {
        name = displayName;
        goid = GetInstanceID().ToString();
        player = Player.Instance;
        descriptiveText = amount + " " + ammoType + " cartridges";
        Load();
    }

    public override void Save()
    {
        if (player)
        {
            ES3.Save(goid + "ammo", amount);
            base.Save();
        }
    }

    public override void Load()
    {
        amount = ES3.Load(goid + "ammo", amount);
        base.Load();
    }
}
