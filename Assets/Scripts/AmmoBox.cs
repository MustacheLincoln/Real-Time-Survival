using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoBox : Item
{
    Player player;

    int rifleAmmo;
    int pistolAmmo;

    public enum Type { Pistol, Rifle, Random }
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
            case Type.Pistol:
                PistolSetup();
                break;
            case Type.Rifle:
                RifleSetup();
                break;
        }
    }

    private void PistolSetup()
    {
        name = "Pistol Ammo";
        pistolAmmo = 20;
    }

    private void RifleSetup()
    {
        name = "Rifle Ammo";
        rifleAmmo = 10;
    }

    public override void PickUp()
    {
        player.pistolAmmo += pistolAmmo;
        player.rifleAmmo += rifleAmmo;
        Destroy(gameObject);
    }
}
