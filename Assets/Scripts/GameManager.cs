using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    Player player;

    public TimeSpan timeSurvived;
    public List<String> inspected;


    public enum GameState { Playing, Inspecting }
    public GameState gameState;

    private void Awake() { Instance = this; }

    private void Start()
    {
        player = Player.Instance;
        gameState = GameState.Playing;
        inspected = ES3.Load("inspected", inspected);
    }

    private void Update()
    {
        if (player)
            timeSurvived = player.vitals.timeSurvived;
    }

    private void OnApplicationQuit()
    {
        if (player)
        {
            ES3.Save("inspected", inspected);
        }
        else
        {
            ES3.DeleteFile();
        }
    }
}
