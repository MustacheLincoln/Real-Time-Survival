using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    public string goid;
    public float searchTime = 3;
    public float timeElapsed;
    public List<Item> contents;
    public bool searched;
    private Player player;

    private void Start()
    {
        goid = GetInstanceID().ToString();
        player = Player.Instance;
        foreach (Item item in transform.GetComponentsInChildren<Item>(true))
        {
            contents.Add(item);
        }
        Load();
        if (searched && contents.Count <= 0)
            name = "Empty " + name;
    }

    private void Load()
    {
        searched = ES3.Load(goid + "searched", false);
        contents = ES3.Load(goid + "contents", contents);
    }

    void Save()
    {
        if (player)
        {
            ES3.Save(goid + "searched", searched);
            ES3.Save(goid + "contents", contents);
        }
    }

    public void Open()
    {
        contents.Clear();
        foreach (Item item in transform.GetComponentsInChildren<Item>(true))
        {
            item.gameObject.SetActive(false);
            contents.Add(item);
        }
        searched = true;
        if (contents.Count > 0)
        {
            StartCoroutine(player.Inspect(contents[0]));
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

    private void OnApplicationQuit()
    {
        Save();
    }
}
