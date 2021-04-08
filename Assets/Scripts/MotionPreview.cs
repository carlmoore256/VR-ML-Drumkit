using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionPreview : MonoBehaviour
{
    LineRenderer line;

    public float trailSecs = 1f;

    public int deviceIndex;

    public Vector3 trailOffset;

    int m_TrailLength;

    CaptureMotion cm;

    Vector3[] linePoints;

    bool m_Active = false;


    void Start()
    {
        line = GetComponent<LineRenderer>();
        cm = GameObject.Find("Manager").GetComponent<CaptureMotion>();
        m_TrailLength = CalculateTrailLength(trailSecs);
        print("trail length : " + m_TrailLength);
        linePoints = new Vector3[m_TrailLength];
        line.enabled = false;
    }

    void Update()
    {
        //if(cm.capRunning)
        if(m_Active)
        {
            Vector3[] positions = cm.GetPositions(m_TrailLength, deviceIndex, trailOffset);

            if(positions != null)
            { 
                line.positionCount = m_TrailLength;
                line.SetPositions(positions);
            } else
            {
                print("POSITIONS ARE NULL, START CAPTURE");
            }
        }
    }

    int CalculateTrailLength(float seconds)
    {
        return (int)(cm.sampleRate * seconds);
    }


    // call this to toggle motion preview
    public void ToggleActive(float trailLenSecs)
    {
        if (m_Active)
        {
            m_Active = false;
            line.enabled = false;
        } else
        {
            m_Active = true;
            line.enabled = true;
            m_TrailLength = CalculateTrailLength(trailLenSecs);
        }

    }
}
