using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public virtual void PickUp()
    {

    }

    public virtual void Save()
    {

    }

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