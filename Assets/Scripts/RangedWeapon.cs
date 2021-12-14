using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedWeapon : MonoBehaviour, IPickUpable
{
    Player player;

    float rangedAttackDamage;
    float rangedAttackSpeed;
    float rangedAttackNoise;
    float rangedAttackRange;
    float rangedKnockback;
    bool rangedAttackAutomatic;
    int magazineSize;
    float reloadTime;
    float aimTime;

    public enum Type { Random, Pistol, Rifle }
    public Type type;

    private void Start()
    {
        player = Player.Instance;

        switch (type)
        {
            case Type.Random:
                int rand = Random.Range((int)Type.Pistol, (int)Type.Rifle + 1);
                if (rand == (int)Type.Pistol)
                    PistolSetup();
                if (rand == (int)Type.Rifle)
                    RifleSetup();
                break;
            case Type.Pistol:
                PistolSetup();
                break;
            case Type.Rifle:
                RifleSetup();
                break;
        }

    }

    private void PistolSetup()
    {
        name = "Pistol";
        rangedAttackDamage = 50;
        rangedAttackSpeed = .01f;
        rangedAttackNoise = 10;
        rangedAttackRange = 10;
        rangedKnockback = .1f;
        rangedAttackAutomatic = false;
        magazineSize = 10;
        reloadTime = 1;
        aimTime = .5f;
    }

    private void RifleSetup()
    {
        name = "Rifle";
        rangedAttackDamage = 100;
        rangedAttackSpeed = .01f;
        rangedAttackNoise = 20;
        rangedAttackRange = 20;
        rangedKnockback = .25f;
        rangedAttackAutomatic = false;
        magazineSize = 5;
        reloadTime = 2;
        aimTime = 1;
    }

    public void Equip()
    {
        player.rangedWeaponEquipped = gameObject;
        player.hasRangedWeapon = true;
        player.rangedAttackDamage = rangedAttackDamage;
        player.rangedAttackSpeed = rangedAttackSpeed;
        player.rangedAttackNoise = rangedAttackNoise;
        player.rangedAttackRange = rangedAttackRange;
        player.rangedKnockback = rangedKnockback;
        player.rangedAttackAutomatic = rangedAttackAutomatic;
        player.magazineSize = magazineSize;
        player.reloadTime = reloadTime;
        player.aimTime = aimTime;
    }

    public void PickUp()
    {
        bool dupe = false;
        if (!player.rangedWeapons.Contains(gameObject))
        {
            player.rangedWeapons.Add(gameObject);
            gameObject.SetActive(false);
            transform.parent = player.transform;
        }

        foreach (GameObject weapon in player.rangedWeapons)
        {
            if (weapon.name == name)
            {
                if (weapon != gameObject)
                {
                    player.rangedWeapons.Remove(gameObject);
                    Destroy(gameObject);
                    dupe = true;
                    break;
                }
            }
        }
        if (dupe == false)
            Equip();
    }
}
