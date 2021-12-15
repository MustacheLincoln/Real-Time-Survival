using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    Player player;

    public Image eatingProgressBar;
    public Image reloadProgressBar;
    public Image aimProgressBar;
    public TMP_Text targetLabel;

    private void Start()
    {
        player = Player.Instance;
    }

    private void Update()
    {
        WorldSpaceUI();
    }

    private void WorldSpaceUI()
    {
        reloadProgressBar.transform.position = player.transform.position;
        reloadProgressBar.fillAmount = player.reloadTimeElapsed / player.reloadTime;

        eatingProgressBar.transform.position = player.transform.position;
        eatingProgressBar.fillAmount = player.eatingTimeElapsed / player.eatingTime;

        if (player.fov.target)
        {
            targetLabel.text = player.fov.target.name;
            targetLabel.transform.position = player.fov.target.transform.position;
        }
        else
            targetLabel.text = null;

        if (player.actionState == Player.ActionState.Aiming)
        {
            if (player.target)
            {
                aimProgressBar.transform.position = player.target.transform.position;
                aimProgressBar.fillAmount = player.aimTimeElapsed / player.aimTime;
            }
            else
                aimProgressBar.fillAmount = 0;
        }
        else
            aimProgressBar.fillAmount = 0;
    }
}
