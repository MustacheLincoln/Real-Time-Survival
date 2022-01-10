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
    public bool large;

    private void Start()
    {
        name = displayName;
        goid = GetInstanceID().ToString();
        player = Player.Instance;
        durability = maxDurability / 2 + Random.Range(0, maxDurability / 2 + 1);
        descriptiveText = "Damage: " + meleeAttackDamage + "\nSpeed: " + meleeAttackSpeed + "\nNoise: " + meleeAttackNoise + "\nRange: " + meleeAttackRange + "\nDurability: " + durability + "/" + maxDurability + "\nRT to swing";

        Load();
    }
    public override void Load()
    {
        durability = ES3.Load(goid + "durability", durability);
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
            ES3.Save(goid + "durability", durability);
            ES3.Save(goid + "activeSelf", gameObject.activeSelf);
            ES3.Save(goid + "colliderEnabled", GetComponent<Collider>().enabled);
            ES3.Save(goid + "parent", transform.parent);
            ES3.Save(goid + "position", transform.localPosition);
            ES3.Save(goid + "rotation", transform.localRotation);
        }
    }

    public override void Equip()
    {
        int indexModifier = 0;
        player = Player.Instance;
        if (player.meleeWeaponEquipped)
            if (player.meleeWeaponEquipped != this)
            {
                player.meleeWeaponEquipped.Unequip();
                indexModifier = -1;
            }
        gameObject.SetActive(true);
        GetComponent<Collider>().enabled = false;
        player.RemoveItem(this, indexModifier);
        player.meleeWeaponEquipped = this;
        player.HolsterWeapon();
    }

    public void Break()
    {
        gameObject.SetActive(false);
        transform.parent = null;
        player.meleeWeaponEquipped = null;
        Save();
    }
}
