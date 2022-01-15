using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : Item
{
    public enum AmmoType { Rifle, Pistol }
    public AmmoType ammoType;

    private void Start()
    {
        goid = GetInstanceID().ToString();
        descriptiveText = "A box of " + ammoType + " cartridges";
        Load();
    }

    public void Update()
    {
        name = displayName + " (" + amount + ")";
    }

    public override void Save()
    {
        Player player = Player.Instance;
        if (player)
        {
            ES3.Save(goid + "ammo", amount);
            base.Save();
        }
    }

    public override void Use(Player owner)
    {
        if (owner.rangedWeaponEquipped)
            StartCoroutine(owner.Reloading(owner.rangedWeaponEquipped, this));
    }

    public override void Load()
    {
        amount = ES3.Load(goid + "ammo", amount);
        base.Load();
    }
}
