using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    CameraController cam;

    public virtual void PickUp()
    {

    }

    public virtual void Save()
    {

    }

    public void Equip()
    {
        gameObject.SetActive(true);
        cam = CameraController.Instance;
        //transform.position = cam.inspectPoint.position;
        //transform.rotation = cam.inspectPoint.rotation;
        //transform.parent = cam.inspectPoint;
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