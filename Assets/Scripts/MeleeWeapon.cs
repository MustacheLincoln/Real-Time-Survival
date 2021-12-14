using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    PlayerController player;

    float meleeAttackDamage = 50;
    float meleeAttackSpeed = .5f;
    float meleeAttackNoise = 6;
    float meleeAttackRange = .5f;
    float meleeKnockback = .5f;

    public enum Type { Random, Crobar, Knife }
    public Type type;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        switch (type)
        {
            case Type.Random:
                int rand = Random.Range((int)Type.Crobar, (int)Type.Knife + 1);
                if (rand == (int)Type.Crobar)
                    CrobarSetup();
                if (rand == (int)Type.Knife)
                    KnifeSetup();
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
    }

    private void KnifeSetup()
    {
        name = "Knife";
        meleeAttackDamage = 34;
        meleeAttackSpeed = .25f;
        meleeAttackNoise = 3;
        meleeAttackRange = .4f;
        meleeKnockback = .1f;
    }

    private void OnDestroy()
    {
        if (!player.meleeWeapons.Contains(this))
        {
            player.meleeWeapons.Add(this);

        player.hasMeleeWeapon = true;
        player.meleeAttackDamage = meleeAttackDamage;
        player.meleeAttackSpeed = meleeAttackSpeed;
        player.meleeAttackNoise = meleeAttackNoise;
        player.meleeAttackRange = meleeAttackRange;
        player.meleeKnockback = meleeKnockback;
        }
    }
}
