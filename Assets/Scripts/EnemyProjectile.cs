using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private GameObject playerHead;
    private PlayerData data;

    void Start()
    {
        playerHead = GameObject.Find("PlayerHead");
        data = GameObject.Find("PlayerData").GetComponent<PlayerData>();
    }
    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Obstacle")
        {
            Destroy(gameObject);
        }
        else if (collision.gameObject.tag == "Player")
        {
            data.TakeDamage(1);
            Destroy(gameObject);
        }
    }
}