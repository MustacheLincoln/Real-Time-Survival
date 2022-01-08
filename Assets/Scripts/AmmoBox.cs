using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoBox : Item
{
    Player player;
    public int rifleAmmo;
    public int pistolAmmo;

    private void Start()
    {
        name = displayName;
        goid = GetInstanceID().ToString();
        player = Player.Instance;
        descriptiveText = rifleAmmo + " rifle cartridges";
        Load();
    }

    private void Load()
    {
        gameObject.SetActive(ES3.Load(goid + "activeSelf", true));
        transform.parent = ES3.Load(goid + "parent", transform.parent);
        transform.position = ES3.Load(goid + "position", transform.position);
        transform.rotation = ES3.Load(goid + "rotation", transform.rotation);
    }

    public override void Save()
    {
        if (player)
        {
            ES3.Save(goid + "activeSelf", gameObject.activeSelf);
            ES3.Save(goid + "position", transform.position);
            ES3.Save(goid + "rotation", transform.rotation);
        }
    }

    public override void PickUp()
    {
        player.pistolAmmo += pistolAmmo;
        player.rifleAmmo += rifleAmmo;
        gameObject.SetActive(false);
        transform.parent = null;
        ES3.Save(goid + "activeSelf", gameObject.activeSelf);
        ES3.Save(goid + "parent", transform.parent);
    }
}
