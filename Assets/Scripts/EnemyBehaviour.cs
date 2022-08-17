using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    public float[,] movementRestriction = new float[3, 2] {{-4.0f, 4.0f}, {3.0f, 5.0f}, {0.0f, 3.0f}};
    public GameObject enemyMissilePrefab;
    public AudioClip charging;
    public AudioClip shooting;
    public GameObject deathParticles;

    private GameObject playerHead;
    private ParticleSystem shootingPrepAnimation;
    private GameObject model;
    void Start()
    {
        playerHead = GameObject.Find("PlayerHead");
        shootingPrepAnimation = gameObject.GetComponent<ParticleSystem>();
        model = gameObject.transform.GetChild(1).gameObject;
        StartCoroutine(MoveAround());
        StartCoroutine(PrepareAttack());
        StartCoroutine(Bob());
    }
    void Update()
    {
        gameObject.transform.LookAt(playerHead.transform);
    }
    public void DeathParticle()
    {
        var obj = Instantiate(deathParticles);
        obj.transform.position = gameObject.transform.position;
        obj.transform.rotation = gameObject.transform.rotation;
        obj.transform.parent = null;
    }
    IEnumerator MoveAround()
    {
        float t;
        while (true)
        {
            t = 0.0f;
            Vector3 newPos = new Vector3(Random.Range(movementRestriction[0, 0], movementRestriction[0, 1]),
                Random.Range(movementRestriction[1, 0], movementRestriction[1, 1]),
                Random.Range(movementRestriction[2, 0], movementRestriction[2, 1]));
            float randomizedSpeed = Random.Range(3.0f, 4.0f);
            while (t < randomizedSpeed)
            {
                t += Time.deltaTime;
                Vector3 nextPos = Vector3.Lerp(gameObject.transform.position, newPos, t / randomizedSpeed);
                gameObject.transform.localPosition = nextPos;
                yield return new WaitForSecondsRealtime(0.011f);
            }
            float randomizedDelay = Random.Range(1.25f, 1.75f);
            yield return new WaitForSecondsRealtime(randomizedDelay);
        }
    }
    IEnumerator PrepareAttack()
    {
        float randomizedDelay;
        while(true)
        {
            randomizedDelay = Random.Range(7.5f, 10.0f);
            yield return new WaitForSecondsRealtime(randomizedDelay);
            shootingPrepAnimation.Play();
            gameObject.GetComponent<AudioSource>().PlayOneShot(charging, 1.0f);
            yield return new WaitForSecondsRealtime(3.0f);
            gameObject.GetComponent<AudioSource>().PlayOneShot(shooting, 1.0f);
            shootingPrepAnimation.Stop();
            var obj = Instantiate(enemyMissilePrefab, model.transform.position, model.transform.rotation);
            obj.transform.parent = null;
            obj.transform.LookAt(playerHead.transform);
            obj.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * 50.0f, ForceMode.Force);
        }
    }
    IEnumerator Bob()
    {
        var model = gameObject.transform.GetChild(0).gameObject;
        while (true)
        {
            float t = 0.0f;
            while (t < 2.0f)
            {
                t += Time.deltaTime;
                Vector3 prev = gameObject.transform.localPosition;
                Vector3 nextPos = Vector3.Lerp(prev, prev + new Vector3(0.0f, 0.002f, 0.0f), t / 2.0f);
                gameObject.transform.localPosition = nextPos;
                yield return 0;
            }
            t = 0.0f;
            while (t < 2.0f)
            {
                t += Time.deltaTime;
                Vector3 prev = gameObject.transform.localPosition;
                Vector3 nextPos = Vector3.Lerp(prev, prev - new Vector3(0.0f, 0.002f, 0.0f), t / 2.0f);
                gameObject.transform.localPosition = nextPos;
                yield return 0;
            }
        }
    }
    private void OnDestroy()
    {
        DeathParticle();
    }
}
