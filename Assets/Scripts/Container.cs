using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    public string goid;
    float searchTime;
    public float timeElapsed;
    public List<Item> contents;
    public bool searched;
    private GameManager gameManager;
    private Player player;

    private void Start()
    {
        searchTime = 3;
        searched = false;
        gameManager = GameManager.Instance;
        player = Player.Instance;
        foreach (Item item in transform.GetComponentsInChildren<Item>())
        {
            item.gameObject.layer = 0;
            contents.Add(item);
        }
    }

    public void Search()
    {
        if (searched == false)
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed >= searchTime)
            {
                Open();
                timeElapsed = 0;
            }
        }
        if (searched == true && contents.Count > 0)
        {
            Open();
        }
    }

    public void Open()
    {
        contents.Clear();
        foreach (Item item in transform.GetComponentsInChildren<Item>())
        {
            item.gameObject.layer = 0;
            contents.Add(item);
        }
        searched = true;
        if (contents.Count > 0)
        {
            player.pickUpTarget = contents[0];
            player.Inspect();
        }
        else
            name = "Empty " + name;
    }

    public void NextItem()
    {
        if (contents.Count > 0)
        {
            contents.Remove(contents[0]);
            Open();
        }
    }
}
