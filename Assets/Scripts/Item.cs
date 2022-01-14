using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public string displayName;
    public string goid;
    public string descriptiveText;
    public int amount;
    public int maxAmount;
    public Sprite icon;

    public virtual void Save()
    {
        Player player = Player.Instance;
        if (player)
        {
            ES3.Save(goid + "rendererEnabled", GetComponent<Renderer>().enabled);
            ES3.Save(goid + "colliderEnabled", GetComponent<Collider>().enabled);
            ES3.Save(goid + "parent", transform.parent);
            ES3.Save(goid + "position", transform.localPosition);
            ES3.Save(goid + "rotation", transform.localRotation);
        }
    }

    public virtual void Load()
    {
        GetComponent<Renderer>().enabled = ES3.Load(goid + "rendererEnabled", GetComponent<Renderer>().enabled);
        GetComponent<Collider>().enabled = ES3.Load(goid + "colliderEnabled", GetComponent<Collider>().enabled);
        transform.parent = ES3.Load(goid + "parent", transform.parent);
        transform.localPosition = ES3.Load(goid + "position", transform.localPosition);
        transform.localRotation = ES3.Load(goid + "rotation", transform.localRotation);
        if (transform.parent)
            if (transform.parent.GetComponent<Container>())
            {
                GetComponent<Renderer>().enabled = false;
                GetComponent<Collider>().enabled = false;
            }
    }

    public virtual void Drop()
    {
        GetComponent<Renderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
        transform.parent = null;
    }

    public virtual void Equip(Player owner) { }

    public virtual void Use(Player owner) 
    {
        Equip(owner);
    }

    public void Unequip(Player owner)
    {
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        if (!owner.items.Contains(this))
        {
            if (owner.itemSelected)
            {
                int index = owner.items.IndexOf(owner.itemSelected);
                if (index <= owner.items.Count - 1)
                    owner.items.Insert(index, this);
            }
            else
                owner.items.Add(this);
            owner.itemSelected = this;
        }
    }

    public virtual void AddToInventory()
    {
        Player player = Player.Instance;
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        transform.position = player.transform.position;
        transform.parent = player.transform;
        if (!player.items.Contains(this))
            player.items.Add(this);
        if (player.itemSelected == null)
            player.itemSelected = this;
    }

    public void Destroy()
    {
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        transform.parent = null;
    }

    private void OnApplicationQuit()
    {
        Save();
    }
}