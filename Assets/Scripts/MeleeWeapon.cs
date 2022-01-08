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
        if (player)
            if (player.meleeWeapons.Contains(this))
            {
                gameObject.layer = 0;
                transform.position = player.transform.position;
                transform.parent = player.transform;
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
            ES3.Save(goid + "durability", durability);
        }
    }

    public override void PickUp()
    {
        if (!player.meleeWeapons.Contains(this))
        {
            player.meleeWeapons.Add(this);
            gameObject.layer = 0;
            Unequip();
            transform.position = player.transform.position;
            transform.parent = player.transform;
            if (player.meleeWeaponEquipped == null)
            {
                player.meleeWeaponEquipped = this;
                Equip();
            }
        }
    }
}
