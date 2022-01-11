using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public string displayName;
    public string goid;
    public string descriptiveText;
    public Sprite icon;

    public virtual void Save()
    {
        Player player = Player.Instance;
        if (player)
        {
            ES3.Save(goid + "activeSelf", gameObject.activeSelf);
            ES3.Save(goid + "colliderEnabled", GetComponent<Collider>().enabled);
            ES3.Save(goid + "parent", transform.parent);
            ES3.Save(goid + "position", transform.localPosition);
            ES3.Save(goid + "rotation", transform.localRotation);
        }
    }

    public virtual void Load()
    {
        gameObject.SetActive(ES3.Load(goid + "activeSelf", true));
        GetComponent<Collider>().enabled = ES3.Load(goid + "colliderEnabled", GetComponent<Collider>().enabled);
        transform.parent = ES3.Load(goid + "parent", transform.parent);
        transform.localPosition = ES3.Load(goid + "position", transform.localPosition);
        transform.localRotation = ES3.Load(goid + "rotation", transform.localRotation);
        if (transform.parent)
            if (transform.parent.GetComponent<Container>())
                gameObject.SetActive(false);
    }

    public virtual void Drop(Player owner)
    {
        gameObject.SetActive(true);
        GetComponent<Collider>().enabled = true;
        transform.parent = null;
        if (owner.items.Contains(this))
            owner.items.Remove(this);
    }

    public virtual void Equip(Player owner)
    {

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

    public virtual void AddToInventory()
    {
        Player player = Player.Instance;
        gameObject.SetActive(false);
        transform.position = player.transform.position;
        transform.parent = player.transform;
        if (!player.items.Contains(this))
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