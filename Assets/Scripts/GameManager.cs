using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    Player player;

    public TimeSpan timeSurvived;

    private void Awake() { Instance = this; }

    private void Start()
    {
        player = Player.Instance;
    }

    private void Update()
    {
        if (player)
        {
            timeSurvived = player.vitals.timeSurvived;
        }
    }

    private void OnApplicationQuit()
    {
        if (player == null)
        {
            ES3.DeleteFile();
        }
    }
}
