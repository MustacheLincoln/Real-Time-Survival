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
        player.RemoveItem(this, -1);
    }

    public void Unequip()
    {
        Player player = Player.Instance;
        gameObject.SetActive(false);
        GetComponent<Collider>().enabled = true;
        if (player.itemSelected)
        {
            int index = player.items.IndexOf(player.itemSelected);
            if (index <= player.items.Count - 1)
                player.items.Insert(index, this);
            else
                player.items.Insert(0, this);
        }
        else
            player.items.Insert(0, this);
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