using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SpellProperties : MonoBehaviour
{
    public enum Spell
    {
        Fire,
        Void,
        Shield,
        Energy
    };
    public Spell spell;
    public List<ParticleSystem> emit;

    private int hp = 0;
    private PlayerData data;
    void Start()
    {
        data = GameObject.Find("PlayerData").GetComponent<PlayerData>();
        if (spell == Spell.Fire)
        {
            hp = 1;
        }
        else if (spell == Spell.Void)
        {
            hp = 1;
        }
        else if (spell == Spell.Shield)
        {
            hp = 5;
        }
        else if (spell == Spell.Energy)
        {
            hp = 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Obstacle" || collision.gameObject.tag == "Puzzle")
        {
            hp = 0;
            if (spell == Spell.Shield)
            {
                gameObject.GetComponent<XRGrabInteractable>().colliders.Clear();
                GetComponent<BreakObject>().DestroyMesh();
            }
            else if (spell == Spell.Fire)
            {
                gameObject.GetComponent<OffsetTomatoPresence>().colliders.Clear();
                StartCoroutine(DelayedDestroy());
            }
            else
            {
                gameObject.GetComponent<XRGrabInteractable>().colliders.Clear();
                Destroy(gameObject);
            }
        }
        else if (collision.gameObject.tag == "EnemyProjectile" && spell == Spell.Shield)
        {
            Destroy(collision.gameObject);
            hp--;
            if (hp == 0)
            {
                gameObject.GetComponent<XRGrabInteractable>().colliders.Clear();
                GetComponent<BreakObject>().DestroyMesh();
            }
        }
        else if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Spell" || collision.gameObject.tag == "EnemyProjectile")
        {

        }
        else
        {
            try
            {
                Destroy(collision.gameObject.transform.parent.gameObject);
            }
            catch
            {
                Destroy(collision.gameObject);
            }
            data.GetPoints(1);
            hp--;
            if (hp == 0)
            {
                if (spell == Spell.Shield)
                {
                    gameObject.GetComponent<XRGrabInteractable>().colliders.Clear();
                    GetComponent<BreakObject>().DestroyMesh();
                }
                else if (spell == Spell.Fire)
                {
                    gameObject.GetComponent<OffsetTomatoPresence>().colliders.Clear();
                    StartCoroutine(DelayedDestroy());
                }
                else
                {
                    gameObject.GetComponent<XRGrabInteractable>().colliders.Clear();
                    Destroy(gameObject);
                }
            }
        }
    }
    private void OnDestroy()
    {
        if (spell != Spell.Fire)
        {
            DetachParticles();
        }
    }
    public void DetachParticles()
    {
        for (int i = 0; i < emit.Count; i++)
        {
            emit[i].transform.position = emit[i].transform.parent.position;
            emit[i].transform.rotation = Quaternion.identity;
            emit[i].transform.parent = null;
            if (spell == Spell.Fire)
            {
                emit[i].transform.localScale *= 1;
            }
            else if (spell == Spell.Void)
            {
                emit[i].transform.localScale *= 5;
            }
            var main = emit[i].main;
            main.stopAction = ParticleSystemStopAction.Destroy;
            emit[i].Stop();
        }
    }
    IEnumerator DelayedDestroy()
    {
        gameObject.GetComponent<Collider>().enabled = false;
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        Destroy(gameObject.GetComponent<Rigidbody>());
        Destroy(gameObject.GetComponent<Renderer>());
        gameObject.GetComponentInChildren<TrailRenderer>().autodestruct = true;
        Destroy(gameObject.transform.GetChild(0).GetComponent<Renderer>());
        for (int i = 0; i < emit.Count; i++)
        {
            var main = emit[i].main;
            main.stopAction = ParticleSystemStopAction.Destroy;
            emit[i].Stop();
        }
        yield return new WaitForSecondsRealtime(2.0f);
        Destroy(gameObject);
    }
}
