using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesktopMoCap : MonoBehaviour
{

    public int drawPoints = 60;

    public LineRenderer motionTrail;
    
    List<Vector3[]> points;

    OVRInput.Controller[] trackedControllers;

    void Start()
    {
        points = new List<Vector3[]>();

        motionTrail = new LineRenderer();

        trackedControllers = new OVRInput.Controller[] { OVRInput.Controller.LTrackedRemote, OVRInput.Controller.RTrackedRemote };
    }

    void Update()
    {
/*        Vector3[] capturePoints = new Vector3[trackedControllers.Length]();
        foreach (OVRInput.Controller c in trackedControllers)
        {
            points.Add(OVRInput.GetLocalControllerPosition(c));
        }*/
    }
}
