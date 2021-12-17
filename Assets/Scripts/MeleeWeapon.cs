using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour, IPickUpable
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
            case Type.Crobar:
                CrobarSetup();
                break;
            case Type.Knife:
                KnifeSetup();
                break;
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
    }

    public void PickUp()
    {
        if (!player.meleeWeapons.Contains(this))
        {
            player.meleeWeapons.Add(this);
            gameObject.SetActive(false);
            transform.parent = player.transform;
        }
        if (player.meleeWeaponEquipped == null)
            player.meleeWeaponEquipped = this;
    }
}
