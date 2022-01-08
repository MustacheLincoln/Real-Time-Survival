using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Item
{
    Player player;
    public float meleeAttackDamage;
    public float meleeAttackSpeed;
    public float meleeAttackNoise;
    public float meleeAttackRange;
    public float meleeKnockback;
    public int maxDurability;
    public int durability;

    private void Start()
    {
        name = displayName;
        goid = GetInstanceID().ToString();
        player = Player.Instance;
        durability = maxDurability / 2 + Random.Range(0, maxDurability / 2 + 1);
        descriptiveText = "Damage: " + meleeAttackDamage + "\nSpeed: " + meleeAttackSpeed + "\nNoise: " + meleeAttackNoise + "\nRange: " + meleeAttackRange + "\nDurability: " + durability + "/" + maxDurability + "\nRT to swing";

        Load();
    }
    private void Load()
    {
        durability = ES3.Load(goid + "durability", durability);
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
            ES3.Save(goid + "durability", durability);
            ES3.Save(goid + "parent", transform.parent);
        }
    }

    public void EquipMelee()
    {
        Player player = Player.Instance;
        if (player.meleeWeaponEquipped)
            if (player.meleeWeaponEquipped != this)
                player.meleeWeaponEquipped.Unequip();
        player.meleeWeaponEquipped = this;
        Equip();
    }

    public override void PickUp()
    {
        if (!player.meleeWeapons.Contains(this))
        {
            player.meleeWeapons.Add(this);
            transform.position = player.transform.position;
            transform.parent = player.transform;
            if (player.meleeWeaponEquipped == null)
                EquipMelee();
            else
                AddToInventory();
        }
    }
}
