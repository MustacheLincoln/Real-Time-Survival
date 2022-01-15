using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedWeapon : Item
{
    public float rangedAttackDamage;
    public float rangedAttackSpeed;
    public float rangedAttackNoise;
    public float rangedAttackRange;
    public float rangedKnockback;
    public bool large;
    public int magazineSize;
    public int inMagazine;
    public float reloadTime;
    public float aimTime;
    public Ammo.AmmoType ammoType;

    public enum GunType { FullAuto, SemiAuto, BoltAction }
    public GunType gunType;

    private void Start()
    {
        name = displayName;
        goid = GetInstanceID().ToString();
        descriptiveText = "Damage: " + rangedAttackDamage + "\nTime to aim: " + aimTime + "\nRange: " + rangedAttackRange + "\nLT to aim, RT to fire";
        Load();
    }

    public override void Load()
    {
        inMagazine = ES3.Load(goid + "inMagazine", inMagazine);
        base.Load();
    }

    public override void Save()
    {
        Player player = Player.Instance;
        if (player)
        {
            ES3.Save(goid + "inMagazine", inMagazine);
            base.Save();
        }
    }

    public override void Equip(Player owner)
    {
        bool isReplacingEquipment = false;
        if (owner.rangedWeaponEquipped)
            if (owner.rangedWeaponEquipped != this)
            {
                int storage = 0;
                if (owner.backpackEquipped)
                    storage = owner.backpackEquipped.storage;
                if (owner.items.Count < owner.inventorySize + storage || owner.items.Contains(this))
                {
                    owner.rangedWeaponEquipped.Unequip(owner);
                    isReplacingEquipment = true;
                }
                else
                {
                    owner.Drop(owner.rangedWeaponEquipped);
                }
            }
        GetComponent<Renderer>().enabled = true;
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        owner.RemoveItem(this, isReplacingEquipment);
        owner.rangedWeaponEquipped = this;
        owner.HolsterWeapon();
    }
}
