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
        descriptiveText = rifleAmmo + pistolAmmo + " cartridges";
        Load();
    }

    public override void AddToInventory()
    {
        player.pistolAmmo += pistolAmmo;
        player.rifleAmmo += rifleAmmo;
        gameObject.SetActive(false);
        transform.parent = null;
        player.RemoveItem(this, 0);
        Save();
    }
}
