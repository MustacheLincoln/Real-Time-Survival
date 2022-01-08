using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public string displayName;
    public string goid;
    public string descriptiveText;
    public Sprite icon;

    public abstract void PickUp();

    public abstract void Save();

    public void Equip()
    {
        Player player = Player.Instance;
        gameObject.SetActive(true);
        GetComponent<Collider>().enabled = false;
        if (player.itemSelected == this)
            player.itemSelected = null;
        player.items.Remove(this);
    }

    public void Unequip()
    {
        Player player = Player.Instance;
        gameObject.SetActive(false);
        GetComponent<Collider>().enabled = true;
        if (player.itemSelected)
            player.items.Insert(player.items.IndexOf(player.itemSelected), this);
        else
            player.items.Add(this);
        player.itemSelected = this;
        Save();
    }

    public void AddToInventory()
    {
        Player player = Player.Instance;
        gameObject.SetActive(false);
        player.items.Add(this);
        if (player.itemSelected == null)
            player.itemSelected = this;
        Save();
    }

    private void OnApplicationQuit()
    {
        Save();
    }
}