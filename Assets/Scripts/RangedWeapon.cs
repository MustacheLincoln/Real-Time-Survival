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
    public bool large;
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
        transform.parent = ES3.Load(goid + "parent", transform.parent);
        transform.localPosition = ES3.Load(goid + "position", transform.localPosition);
        transform.localRotation = ES3.Load(goid + "rotation", transform.localRotation);
    }

    public override void Save()
    {
        if (player)
        {
            ES3.Save(goid + "activeSelf", gameObject.activeSelf);
            ES3.Save(goid + "position", transform.localPosition);
            ES3.Save(goid + "rotation", transform.localRotation);
            ES3.Save(goid + "inMagazine", inMagazine);
            ES3.Save(goid + "parent", transform.parent);
        }
    }

    public void EquipRanged()
    {
        player = Player.Instance;
        if (player.rangedWeaponEquipped)
            if (player.rangedWeaponEquipped != this)
                player.rangedWeaponEquipped.Unequip();
        Equip();
        player.rangedWeaponEquipped = this;
        player.HolsterWeapon();
    }

    public override void PickUp()
    {
        player = Player.Instance;
        if (!player.rangedWeapons.Contains(this))
        {
            player.rangedWeapons.Add(this);
            if (player.rangedWeaponEquipped == null)
                EquipRanged();
            else
                AddToInventory();
        }
    }
}
