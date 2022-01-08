using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    void Start()
    {
        StartCoroutine(SpawnOnLoop());
    }

    IEnumerator SpawnOnLoop()
    {
        while(true)
        {
            yield return new WaitForSecondsRealtime(20.0f);
            if (new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy")).Count < 3)
            {
                var obj = Instantiate(enemyPrefab, gameObject.transform.position, gameObject.transform.rotation);
                obj.transform.parent = null;
            }
        }
    }
}
