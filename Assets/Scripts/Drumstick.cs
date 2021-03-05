using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drumstick : MonoBehaviour
{
    public Transform controller;

    OVRCameraRig cameraRig;
    void Start()
    {
        transform.parent = controller;
        cameraRig = GameObject.Find("OVRCameraRig").GetComponent<OVRCameraRig>();
    }

    // Make methods to re-position stick using joystick after toggle command
    void Update()
    {
        
    }

    
}
