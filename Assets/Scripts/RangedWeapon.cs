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
    public bool primary;
    public bool sideArm;
    public int magazineSize;
    public int inMagazine;
    public float reloadTime;
    public float aimTime;

    private void Start()
    {
        name = displayName;
        goid = GetInstanceID().ToString();
        player = Player.Instance;
        descriptiveText = "Semi-automatic\nDamage: " + rangedAttackDamage + "\nTime to aim: " + aimTime + "\nNoise: " + rangedAttackNoise + "\nRange: " + rangedAttackRange + "\nLT to aim, RT to fire";
        Load();
    }

    private void Load()
    {
        inMagazine = ES3.Load(goid + "inMagazine", inMagazine);
        gameObject.SetActive(ES3.Load(goid + "activeSelf", true));
        if (player)
            if (player.rangedWeapons.Contains(this))
            {
                gameObject.layer = 0;
                transform.position = player.transform.position;
                transform.parent = player.transform;
                if (player.rangedWeaponEquipped == this)
                {
                    transform.position = player.holdPoint.position;
                    transform.rotation = player.holdPoint.rotation;
                    transform.parent = player.holdPoint;
                }
            }
            else
            {
                transform.position = ES3.Load(goid + "position", transform.position);
                transform.rotation = ES3.Load(goid + "rotation", transform.rotation);
            }
    }

    public override void Save()
    {
        if (player)
        {
            ES3.Save(goid + "activeSelf", gameObject.activeSelf);
            ES3.Save(goid + "position", transform.position);
            ES3.Save(goid + "rotation", transform.rotation);
            ES3.Save(goid + "inMagazine", inMagazine);
        }
    }

    public void EquipRanged()
    {
        if (player.rangedWeaponEquipped)
            player.rangedWeaponEquipped.Unequip();
        player.rangedWeaponEquipped = this;
        Equip();
    }

    public override void PickUp()
    {
        if (!player.rangedWeapons.Contains(this))
        {
            player.rangedWeapons.Add(this);
            gameObject.layer = 0;
            transform.position = player.holdPoint.position;
            transform.rotation = player.holdPoint.rotation;
            transform.parent = player.holdPoint;
            if (player.rangedWeaponEquipped == null)
                EquipRanged();
            else
                Unequip();
        }
    }
}
