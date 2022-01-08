using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandPresence : MonoBehaviour
{
    public List<GameObject> controllerPrefab; // hand prefabs
    public InputDeviceCharacteristics controllerChatacteristics; // characteristics of the controller
    
    private InputDevice targetDevice; // device of chosen characteristics
    private GameObject spawnedController; // spawned controller object
    private Animator handAnimator; // hand animator
    void Start()
    {
        Initialize();
    }
    void Initialize()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(controllerChatacteristics, devices);
        if (devices.Count > 0)
        {
            targetDevice = devices[0];
            GameObject prefab = controllerPrefab[0];
            if (prefab)
            {
                spawnedController = Instantiate(prefab, transform);
            }
            handAnimator = spawnedController.GetComponent<Animator>();
        }
    }
    void UpdateAnimation()
    {
        if (targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
        {
            handAnimator.SetFloat("Trigger", triggerValue);
        }
        else
        {
            handAnimator.SetFloat("Trigger", 0);
        }
        
        if (targetDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
        {
            handAnimator.SetFloat("Grip", gripValue);
        }
        else
        {
            handAnimator.SetFloat("Grip", 0);
        }
    }
    void Update()
    {
        if (!targetDevice.isValid)
        {
            Initialize();
        }
        else
        {
            UpdateAnimation();
        }
    }
}
