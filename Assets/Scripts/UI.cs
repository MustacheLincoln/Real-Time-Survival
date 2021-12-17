using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    Player player;

    public TMP_Text timeSurvivedLabel;
    public TMP_Text realTimeLabel;
    public TMP_Text rangedWeaponLabel;
    public TMP_Text meleeWeaponLabel;
    public TMP_Text itemLabel;
    public Image healthRadial;
    public Image staminaRadial;
    public Image hungerRadial;
    public Image thirstRadial;
    public Image burnedHealthRadial;
    public Image burnedStaminaRadial;

    public Image eatingProgressRadial;
    public Image reloadProgressRadial;
    public Image aimProgressRadial;
    public TMP_Text targetLabel;

    DateTime startTime;

    private void Start()
    {
        player = Player.Instance;
        startTime = DateTime.Now;
    }

    private void Update()
    {
        ScreenSpaceUI();
        WorldSpaceUI();
    }

    private void ScreenSpaceUI()
    {
        DateTime time = DateTime.Now;
        realTimeLabel.text = time.Hour.ToString().PadLeft(2, '0') + ":" + time.Minute.ToString().PadLeft(2, '0');

        if (player)
        {
            TimeSpan timeSurvived = DateTime.Now - startTime;
            timeSurvivedLabel.text = timeSurvived.Hours.ToString().PadLeft(2, '0') + ":" + timeSurvived.Minutes.ToString().PadLeft(2, '0') + ":" + timeSurvived.Seconds.ToString().PadLeft(2, '0');

            healthRadial.fillAmount = player.vitals.health / player.vitals.maxMaxHealth;
            staminaRadial.fillAmount = player.vitals.stamina / player.vitals.maxMaxStamina;
            hungerRadial.fillAmount = player.vitals.calories / player.vitals.maxCalories;
            thirstRadial.fillAmount = player.vitals.milliliters / player.vitals.maxMilliliters;

            burnedHealthRadial.fillAmount = (player.vitals.maxMaxHealth - player.vitals.maxHealth) / player.vitals.maxMaxHealth;
            burnedStaminaRadial.fillAmount = (player.vitals.maxMaxStamina - player.vitals.maxStamina) / player.vitals.maxMaxStamina;

            if (player.rangedWeaponEquipped)
            {
                if (player.rangedWeaponEquipped.name == "Pistol")
                    rangedWeaponLabel.text = "Pistol " + player.rangedWeaponEquipped.GetComponent<RangedWeapon>().inMagazine + "/" + player.pistolAmmo;
                if (player.rangedWeaponEquipped.name == "Rifle")
                    rangedWeaponLabel.text = "Rifle " + player.rangedWeaponEquipped.GetComponent<RangedWeapon>().inMagazine + "/" + player.rifleAmmo;
            }
            else
                rangedWeaponLabel.text = "-----";
            if (player.meleeWeaponEquipped)
                meleeWeaponLabel.text = player.meleeWeaponEquipped.name + player.meleeWeaponEquipped.GetComponent<MeleeWeapon>().durability + "/" + player.meleeWeaponEquipped.GetComponent<MeleeWeapon>().maxDurability;
            else
                meleeWeaponLabel.text = "-----";
            if (player.itemSelected)
                itemLabel.text = player.itemSelected.name;
            else
                itemLabel.text = "-----";
        }

    }

    private void WorldSpaceUI()
    {
        if (player)
        {
            reloadProgressRadial.transform.position = player.transform.position;
            reloadProgressRadial.fillAmount = player.reloadTimeElapsed / player.reloadTime;

            eatingProgressRadial.transform.position = player.transform.position;
            eatingProgressRadial.fillAmount = player.eatingTimeElapsed / player.eatingTime;

            if (player.fov.target)
            {
                targetLabel.text = player.fov.target.name;
                targetLabel.transform.position = player.fov.target.transform.position;
                if (player.fov.target.name == "Zombie")
                    targetLabel.text = null;
            }
            else
                targetLabel.text = null;

            if (player.actionState == Player.ActionState.Aiming)
            {
                if (player.target)
                {
                    aimProgressRadial.transform.position = player.target.transform.position;
                    aimProgressRadial.fillAmount = player.aimTimeElapsed / player.aimTime;
                }
                else
                    aimProgressRadial.fillAmount = 0;
            }
            else
                aimProgressRadial.fillAmount = 0;
        }
        else
        {
            reloadProgressRadial.enabled = false;
            eatingProgressRadial.enabled = false;
            targetLabel.enabled = false;
            aimProgressRadial.enabled = false;
        }
    }
}
