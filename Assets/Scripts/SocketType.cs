using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

public class SocketType : XRSocketInteractor
{
    public int from = 0; // from which node the spark goes
    public int to = 0; // to which node the spark goes
    public int val = -1; // what letter it is

    private DrawGraph parentGraph; // parent board

    public void getType(XRBaseInteractable interactable)
    {
        PuzzleType pt = interactable.GetComponentInChildren<PuzzleType>();
        val = pt.type;
    }
    protected override void OnSelectEntered(XRBaseInteractable interactable) // on putting the object in and leaving it there
    {
        base.OnSelectEntered(interactable);
        getType(interactable);
    }
    protected override void OnSelectExited(XRBaseInteractable interactable) // on clicking with the intention of taking the object out
    {
        base.OnSelectExited(interactable);
        val = -1;
    }
    protected override void OnEnable()
    {
        base.OnEnable();
    }
    new void Start()
    {
        parentGraph = transform.GetComponentInParent<DrawGraph>();
    }

}
