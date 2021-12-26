using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoBox : Item
{
    Player player;
    string goid;
    int rifleAmmo;
    int pistolAmmo;

    public enum Type { Pistol, Rifle, Random }
    public Type type = Type.Random;

    private void Start()
    {
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
            case Type.Pistol:
                PistolSetup();
                break;
            case Type.Rifle:
                RifleSetup();
                break;
        }
    }
    private void Load()
    {
        gameObject.SetActive(ES3.Load(goid + "activeSelf", true));
        transform.position = ES3.Load(goid + "position", transform.position);
        transform.rotation = ES3.Load(goid + "rotation", transform.rotation);
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
        gameObject.SetActive(false);
        ES3.Save(goid + "activeSelf", gameObject.activeSelf);
    }
}
