using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionPreview : MonoBehaviour
{
    LineRenderer line;

    public float trailSecs = 1f;

    public int deviceIndex;

    int trailLength;

    CaptureMotion cm;

    Vector3[] linePoints;


    void Start()
    {
        line = GetComponent<LineRenderer>();
        cm = GameObject.Find("Manager").GetComponent<CaptureMotion>();
        trailLength = (int)(cm.sampleRate * trailSecs);
        print("trail length : " + trailLength);
        linePoints = new Vector3[trailLength];
    }

    void Update()
    {
        if(cm.capRunning)
        {
            Vector3[] positions = cm.GetPositions(trailLength, deviceIndex);

            if(positions != null)
            {
                line.positionCount = trailLength;
                line.SetPositions(positions);
            }
        }
    }
}
