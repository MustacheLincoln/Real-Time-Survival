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
    public bool large;
    public int magazineSize;
    public int inMagazine;
    public float reloadTime;
    public float aimTime;

    public enum GunType { FullAuto, SemiAuto, BoltAction }
    public GunType gunType;

    private void Start()
    {
        name = displayName;
        goid = GetInstanceID().ToString();
        player = Player.Instance;
        descriptiveText = "Semi-automatic\nDamage: " + rangedAttackDamage + "\nTime to aim: " + aimTime + "\nNoise: " + rangedAttackNoise + "\nRange: " + rangedAttackRange + "\nLT to aim, RT to fire";
        Load();
    }

    public override void Load()
    {
        inMagazine = ES3.Load(goid + "inMagazine", inMagazine);
        gameObject.SetActive(ES3.Load(goid + "activeSelf", true));
        GetComponent<Collider>().enabled = ES3.Load(goid + "colliderEnabled", GetComponent<Collider>().enabled);
        transform.parent = ES3.Load(goid + "parent", transform.parent);
        transform.localPosition = ES3.Load(goid + "position", transform.localPosition);
        transform.localRotation = ES3.Load(goid + "rotation", transform.localRotation);
        if (transform.parent)
            if (transform.parent.GetComponent<Container>())
                gameObject.SetActive(false);
    }

    public override void Save()
    {
        if (player)
        {
            ES3.Save(goid + "inMagazine", inMagazine);
            ES3.Save(goid + "activeSelf", gameObject.activeSelf);
            ES3.Save(goid + "colliderEnabled", GetComponent<Collider>().enabled);
            ES3.Save(goid + "parent", transform.parent);
            ES3.Save(goid + "position", transform.localPosition);
            ES3.Save(goid + "rotation", transform.localRotation);
        }
    }

    public override void Equip(Player owner)
    {
        int indexModifier = 0;
        if (owner.rangedWeaponEquipped)
            if (owner.rangedWeaponEquipped != this)
            {
                int storage = 0;
                if (owner.backpackEquipped)
                    storage = owner.backpackEquipped.storage;
                if (owner.items.Count < owner.inventorySize + storage)
                {
                    owner.rangedWeaponEquipped.Unequip();
                    indexModifier = -1;
                }
                else
                {
                    owner.rangedWeaponEquipped.Drop(owner);
                }
            }
        gameObject.SetActive(true);
        GetComponent<Collider>().enabled = false;
        owner.RemoveItem(this, indexModifier);
        owner.rangedWeaponEquipped = this;
        owner.HolsterWeapon();
    }
}
