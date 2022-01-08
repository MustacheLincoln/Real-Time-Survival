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
        gameObject.SetActive(true);
    }

    public void Unequip()
    {
        gameObject.SetActive(false);
        Save();
    }

    private void OnApplicationQuit()
    {
        Save();
    }
}