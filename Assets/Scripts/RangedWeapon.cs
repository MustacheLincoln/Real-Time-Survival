using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedWeapon : Item
{
    Player player;

    public float rangedAttackDamage;
    public float rangedAttackSpeed;
    public float rangedAttackNoise;
    public float rangedAttackRange;
    public float rangedKnockback;
    public bool fullAuto;
    public bool semiAuto;
    public bool boltAction;
    public int magazineSize;
    public int inMagazine;
    public float reloadTime;
    public float aimTime;

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
        rangedAttackDamage = 34;
        rangedAttackSpeed = .01f;
        rangedAttackNoise = 10;
        rangedAttackRange = 10;
        rangedKnockback = .1f;
        fullAuto = false;
        semiAuto = true;
        boltAction = false;
        magazineSize = 10;
        inMagazine = 10;
        reloadTime = 1;
        aimTime = .5f;
        SetMesh();
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
        semiAuto = false;
        boltAction = true;
        magazineSize = 5;
        inMagazine = 5;
        reloadTime = 2;
        aimTime = 1;
        SetMesh();
    }

    private void SetMesh()
    {
        foreach (Transform child in transform)
        {
            if (child.name == name)
                child.gameObject.SetActive(true);
            else
                child.gameObject.SetActive(false);
        }
    }

    public override void PickUp()
    {
        if (!player.rangedWeapons.Contains(this))
        {
            player.rangedWeapons.Add(this);
            gameObject.SetActive(false);
            transform.parent = player.transform;
        }
        if (player.rangedWeaponEquipped == null)
            player.rangedWeaponEquipped = this;
    }
}
