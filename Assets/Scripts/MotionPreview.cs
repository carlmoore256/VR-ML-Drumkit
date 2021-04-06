﻿using System.Collections;
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

    bool m_Active = false;


    void Start()
    {
        line = GetComponent<LineRenderer>();
        cm = GameObject.Find("Manager").GetComponent<CaptureMotion>();
        trailLength = (int)(cm.sampleRate * trailSecs);
        print("trail length : " + trailLength);
        linePoints = new Vector3[trailLength];
        line.enabled = false;
    }

    void Update()
    {
        //if(cm.capRunning)
        if(m_Active)
            {
            Vector3[] positions = cm.GetPositions(trailLength, deviceIndex);

            if(positions != null)
            {
                line.positionCount = trailLength;
                line.SetPositions(positions);
            } else
            {
                print("POSITIONS ARE NULL, START CAPTURE");
            }
        }
    }

    // call this to toggle motion preview
    public void ToggleActive()
    {
        if (m_Active)
        {
            m_Active = false;
            line.enabled = false;
        } else
        {
            m_Active = true;
            line.enabled = true;
        }

    }
}
