using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public string goid;
    public string descriptiveText;

    public abstract void PickUp();

    public abstract void Save();

    public void SetMesh()
    {
        foreach (Transform child in transform)
        {
            if (child.name == name)
                child.gameObject.SetActive(true);
            else
                child.gameObject.SetActive(false);
        }
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