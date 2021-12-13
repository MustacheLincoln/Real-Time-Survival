using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    public GameObject zombie;

    private void Start()
    {
        StartCoroutine(SpawnZombie());
    }

    IEnumerator SpawnZombie()
    {
        Instantiate(zombie, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(10);
        StartCoroutine(SpawnZombie());
    }
}
