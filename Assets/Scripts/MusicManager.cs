using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private List<AudioSource> sources;
    private int prevCount = 0;
    private int newCount = 0;
    void Start()
    {
        sources = new List<AudioSource>(gameObject.GetComponents<AudioSource>());
    }
    void Update()
    {
        List<GameObject> enemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
        newCount = enemies.Count;
        if (newCount != prevCount)
        {
            StopAllCoroutines();
            prevCount = newCount;
            StartCoroutine(SetAsPlaying(newCount));
        }
    }
    IEnumerator SetAsPlaying(int id)
    {
        if (id > sources.Count - 1)
        {
            id = sources.Count - 1;
        }
        bool ok = false;
        while (!ok)
        {
            ok = true;
            for (int i = 0; i < sources.Count; i++)
            {
                if (i == id)
                {
                    if (sources[id].volume < 1.0f)
                    {
                        ok = false;
                    }
                    sources[id].volume += 0.02f;
                    if (sources[id].volume > 1.0f)
                    {
                        sources[id].volume = 1.0f;
                    }
                }
                else if (sources[i].volume > 0.0f)
                {
                    if (sources[i].volume > 0.0f)
                    {
                        ok = false;
                    }
                    sources[i].volume -= 0.02f;
                    if (sources[i].volume < 0.0f)
                    {
                        sources[i].volume = 0.0f;
                    }
                }
            }
            yield return new WaitForSecondsRealtime(0.05f);
        }
    }
}
