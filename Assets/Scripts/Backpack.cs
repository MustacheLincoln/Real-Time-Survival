using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Backpack : Item
{
    Player player;
    public int storage;

    private void Start()
    {
        name = displayName;
        goid = GetInstanceID().ToString();
        player = Player.Instance;
        descriptiveText = "Adds " + storage + " Inventory Slots";
        Load();
    }

    public override void Equip()
    {
        int indexModifier = 0;
        player = Player.Instance;
        if (player.backpackEquipped)
            if (player.backpackEquipped != this)
            {
                player.backpackEquipped.Unequip();
                indexModifier = -1;
            }
        gameObject.SetActive(true);
        GetComponent<Collider>().enabled = false;
        player.RemoveItem(this, indexModifier);
        player.backpackEquipped = this;
        transform.position = player.backpackAttachPoint.position;
        transform.rotation = player.backpackAttachPoint.rotation;
        transform.parent = player.backpackAttachPoint;
    }
}
