using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleType : MonoBehaviour
{
    public int type = 1; // type of a puzzle piece, also decides the texture of it

    private Material material;
    void Start()
    {
        UpdateMaterial();
    }

    public void UpdateMaterial()
    {
        material = gameObject.GetComponent<Renderer>().material;
        material.SetFloat("Vector1_73CE0406", type);
    }
}
