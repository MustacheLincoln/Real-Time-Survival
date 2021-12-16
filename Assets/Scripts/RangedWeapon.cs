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
    bool fullAuto;
    bool semiAuto;
    bool boltAction;
    int magazineSize;
    int inMagazine;
    float reloadTime;
    float aimTime;

    public enum Type { Pistol, Rifle, Random }
    public Type type = Type.Random;

    private void Start()
    {
        player = Player.Instance;
        Initialize();
    }

    private void Initialize()
    {
        switch (type)
        {
            case Type.Random:
                int rand = Random.Range(0, (int)Type.Random);
                type = (Type)rand;
                Initialize();
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
        fullAuto = false;
        magazineSize = 10;
        inMagazine = 10;
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
        fullAuto = false;
        magazineSize = 5;
        inMagazine = 5;
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
        player.fullAuto = fullAuto;
        player.magazineSize = magazineSize;
        player.inMagazine = inMagazine;
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
