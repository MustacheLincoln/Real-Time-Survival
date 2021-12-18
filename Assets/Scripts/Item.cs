using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour, IPickUpable
{
    public virtual void PickUp()
    {

    }
}
