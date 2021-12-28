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

    public enum Type { Crobar, Knife, Random }
    public Type type = Type.Random;

    private void Start()
    {
        goid = GetInstanceID().ToString();
        player = Player.Instance;
        type = ES3.Load(goid + "type", type);
        Initialize();
        Load();
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
            case Type.Crobar:
                CrobarSetup();
                break;
            case Type.Knife:
                KnifeSetup();
                break;
        }
    }
    private void Load()
    {
        durability = ES3.Load(goid + "durability", durability);
        gameObject.SetActive(ES3.Load(goid + "activeSelf", true));
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
            ES3.Save(goid + "type", type);
            ES3.Save(goid + "activeSelf", gameObject.activeSelf);
            ES3.Save(goid + "position", transform.position);
            ES3.Save(goid + "rotation", transform.rotation);
            ES3.Save(goid + "durability", durability);
        }
    }

    private void CrobarSetup()
    {
        name = "Crobar";
        meleeAttackDamage = 50;
        meleeAttackSpeed = .5f;
        meleeAttackNoise = 6;
        meleeAttackRange = .5f;
        meleeKnockback = .5f;
        maxDurability = 200;
        durability = maxDurability / 2 + Random.Range(0, maxDurability / 2);
        descriptiveText = "Damage: " + meleeAttackDamage + "\nSpeed: " + meleeAttackSpeed + "\nNoise: " + meleeAttackNoise + "\nRange: " + meleeAttackRange + "\nDurability: " + durability + "/" + maxDurability + "\nRT to swing";
    }

    private void KnifeSetup()
    {
        name = "Knife";
        meleeAttackDamage = 34;
        meleeAttackSpeed = .25f;
        meleeAttackNoise = 3;
        meleeAttackRange = .4f;
        meleeKnockback = .1f;
        maxDurability = 100;
        durability = maxDurability / 2 + Random.Range(0, maxDurability / 2 + 1);
        descriptiveText = "Damage: " + meleeAttackDamage + "\nSpeed: " + meleeAttackSpeed + "\nNoise: " + meleeAttackNoise + "\nRange: " + meleeAttackRange + "\nDurability: " + durability + "/" + maxDurability + "\nRT to swing";
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
