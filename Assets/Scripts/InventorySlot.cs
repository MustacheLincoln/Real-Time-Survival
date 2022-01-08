using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image image;
    public Image selected;

    private void Update()
    {
        if (image.sprite == null)
            image.gameObject.SetActive(false);
        if (image.sprite)
            image.gameObject.SetActive(true);
    }
}
