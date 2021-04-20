using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;
using System.Text;

public class CaptureMotion : MonoBehaviour
{
    struct CapturePoint
    {
        public OVRInput.Controller controller;
        public Vector3 position;
        public Vector3 worldSpacePosition; // has offset of the player in world space
        public Vector3 velocity;
        public Vector3 acceleration;
        public Vector3 angularVel;
        public Vector3 angularAccel;
        public Quaternion rotation;
        public Vector2Int midiNote;

        public CapturePoint(OVRInput.Controller controller, Vector3 position, Vector3 velocity, Vector3 acceleration, Vector3 angularVel, Vector3 angularAccel, Quaternion rotation, Vector2Int midi)
        {
            this.controller = controller;
            this.position = position;
            this.worldSpacePosition = position;
            this.velocity = velocity;
            this.acceleration = acceleration;
            this.angularVel = angularVel;
            this.angularAccel = angularAccel;
            this.rotation = rotation;
            this.midiNote = midi;
        }
    }

    // sample rate for capture of motion data from controllers
    public int sampleRate = 500;

    public string saveCaptureDir = "CapturedMotion/";

    //public bool saveJSON = true;

    public float forwardOffset = 0.1f;

    public Transform playerPosition;

    public bool m_CapRunning;

    public int m_FramesCaptured;

    public float m_CapDuration;

    float m_InitTime;

    public Transform m_StickTipL;
    public Transform m_StickTipR;

    private Vector3 m_CurrentTipL;
    private Vector3 m_CurrentTipR;

    private Vector3 currentPlayerPosition;

    private Vector3 playerForward;

    private double sampleInterval;

    Thread captureThread = null;
    List<CapturePoint[]> m_CapturePoints;
    List<CapturePoint[]> m_PreviewCapPoints; // do this differently eventually
    OVRInput.Controller[] trackedControllers;

    DrumCollisionManager dcm;
    CapStatsUI cs_ui;

    void Start()
    {
        m_CapturePoints = new List<CapturePoint[]>();
        sampleInterval = 1.0d / sampleRate;

        //trackedControllers = new OVRInput.Controller[] { OVRInput.Controller.LTrackedRemote, OVRInput.Controller.RTrackedRemote };
        trackedControllers = new OVRInput.Controller[] { OVRInput.Controller.LTouch, OVRInput.Controller.RTouch };
        dcm = GetComponent<DrumCollisionManager>();
        cs_ui = GameObject.Find("CapStats").GetComponent<CapStatsUI>();
    }

    public void ToggleCapture()
    {
        if (m_CapRunning)
        {
            EndCapture();
        } else
        {
            StartCapture();
        }
    }

    // can be called externally to begin the capture
    public void StartCapture()
    {
        m_FramesCaptured = 0;
        m_CapDuration = 0.0f;
        m_InitTime = Time.time;

        cs_ui.UpdateStaticStats(sampleRate);

        print($"Starting capture @ {sampleRate} Hz");

        m_CapturePoints = new List<CapturePoint[]>();
        captureThread = new Thread(RunCapture);
        captureThread.Start();
        m_CapRunning = true;
    }

    public void EndCapture()
    {
        captureThread.Abort();
        print("capture thread ended successfully, saving data...");
        WriteMotionData();
        m_CapRunning = false;
    }

    // main loop to sample controller data
    void RunCapture()
    {
        //capturePoints = new List<CapturePoint[]>();
        double nextCap = AudioSettings.dspTime + sampleInterval;

        while (true)
        {
            // maybe not the best way to sample but here for now
            double now = AudioSettings.dspTime;

            if (now >= nextCap)
            {
                CapturePoint[] ctrlPoints = GetCapturePoints();
                m_CapturePoints.Add(ctrlPoints);
                nextCap = now + sampleInterval;
                m_FramesCaptured += 1;
            }
        }
        // if(!tof && Time.time > timeout)
    }

    // gets a single capture point array of each controller points
    CapturePoint[] GetCapturePoints()
    {
        CapturePoint[] capturePoints = new CapturePoint[trackedControllers.Length];
        int index = 0;

        foreach (OVRInput.Controller c in trackedControllers)
        {
            capturePoints[index].controller = c;
            capturePoints[index].position = OVRInput.GetLocalControllerPosition(c);
            capturePoints[index].worldSpacePosition = OVRInput.GetLocalControllerPosition(c) + currentPlayerPosition + (playerForward * forwardOffset);
            capturePoints[index].velocity = OVRInput.GetLocalControllerVelocity(c);
            capturePoints[index].acceleration = OVRInput.GetLocalControllerAcceleration(c);
            capturePoints[index].angularVel = OVRInput.GetLocalControllerAngularVelocity(c);
            capturePoints[index].angularAccel = OVRInput.GetLocalControllerAngularAcceleration(c);
            capturePoints[index].rotation = OVRInput.GetLocalControllerRotation(c);
            capturePoints[index].midiNote = dcm.GetLastNoteHit(c); // REPLACE THIS WITH DRUM COLLISION MAN METHOD
            index++;
        }

        return capturePoints;
    }

    void Update()
    {

        //Vector3 pos = OVRInput.GetLocalControllerPosition(c);
        // {
        //     print("TIME RAN OUT!!!");
        //     EndCapture();
        // }

        if (m_CapRunning)
        {
            // we can't access this from a separate thread in the capture, 
            // so we assign it during update
            currentPlayerPosition = playerPosition.position;
            playerForward = playerPosition.forward;

            m_CurrentTipL = m_StickTipL.position;
            m_CurrentTipR = m_StickTipR.position;

            m_CapDuration = Time.time - m_InitTime;
            cs_ui.UpdateStats(m_FramesCaptured, m_CapDuration);
        }
    }


    void OnDestroy()
    {
        if (captureThread != null)
            EndCapture();
    }

    void OnApplicationQuit()
    {
        if (captureThread != null)
            EndCapture();
    }

    void WriteMotionData()
    {
        System.DateTime dt = System.DateTime.Now;
        string timestamp = dt.ToString("yyyy-MM-dd-hhmmss");
        string saveDir = System.IO.Path.Combine(saveCaptureDir, timestamp);
        System.IO.Directory.CreateDirectory(saveDir);

        for (int i = 0; i < trackedControllers.Length; i++)
        {
            var pos = new StringBuilder();
            var world_pos = new StringBuilder();
            var vel = new StringBuilder();
            var acc = new StringBuilder();
            var a_vel = new StringBuilder();
            var a_acc = new StringBuilder();
            var midi = new StringBuilder();

            foreach (CapturePoint[] cp in m_CapturePoints)
            {
                pos.AppendLine(ParseVector(cp[i].position));
                world_pos.AppendLine(ParseVector(cp[i].worldSpacePosition));
                vel.AppendLine(ParseVector(cp[i].velocity));
                acc.AppendLine(ParseVector(cp[i].acceleration));
                a_vel.AppendLine(ParseVector(cp[i].angularVel));
                a_acc.AppendLine(ParseVector(cp[i].angularAccel));
                midi.AppendLine(ParseVector2Int(cp[i].midiNote));
            }

            File.WriteAllText(System.IO.Path.Combine(saveDir, $"pos_c{i}.csv"), pos.ToString());
            File.WriteAllText(System.IO.Path.Combine(saveDir, $"pos_world_c{i}.csv"), world_pos.ToString());
            File.WriteAllText(System.IO.Path.Combine(saveDir, $"vel_c{i}.csv"), vel.ToString());
            File.WriteAllText(System.IO.Path.Combine(saveDir, $"acc_c{i}.csv"), acc.ToString());
            File.WriteAllText(System.IO.Path.Combine(saveDir, $"ang_vel_c{i}.csv"), a_vel.ToString());
            File.WriteAllText(System.IO.Path.Combine(saveDir, $"ang_acc_c{i}.csv"), a_acc.ToString());
            File.WriteAllText(System.IO.Path.Combine(saveDir, $"midi_note_c{i}.csv"), midi.ToString());

            print("data written to " + saveDir);
        }
    }

    public string[] Vector3toString(Vector3 v)
    {
        return new string[3] { v.x.ToString(), v.y.ToString(), v.z.ToString() };
    }

    public string[] Vector2InttoString(Vector2Int v)
    {
        return new string[2] { v.x.ToString(), v.y.ToString() };
    }

    public string[] QuaternionToString(Quaternion q)
    {
        return new string[4] { q.w.ToString(), q.x.ToString(), q.y.ToString(), q.z.ToString() };
    }

    // converts vector3 to CSV formatted
    public string ParseVector(Vector3 v)
    {
        string[] vecStr = Vector3toString(v);
        return ParseLine(vecStr);
    }

    public string ParseVector2Int(Vector2Int v)
    {
        string[] vecStr = Vector2InttoString(v);
        return ParseLine(vecStr);
    }

    string ParseLine(string[] strArr)
    {
        var line = new StringBuilder();
        for(int i = 0; i < strArr.Length; i++)
        {
            line.Append(strArr[i]);
            if(i < strArr.Length-1)
                line.Append(",");
        }
        return line.ToString();
    }

    // returns a vector3[] of positions from the last numPoints samples, from the captureDevice (index)
    public Vector3[] GetPositions(int numPoints, int captureDevice, Vector3 offset)
    {
        if (m_CapturePoints.Count < numPoints)
            numPoints = m_CapturePoints.Count;

        if (numPoints == 0)
            return null;

        Vector3[] positions = new Vector3[numPoints];

        CapturePoint[] caps;

        if (m_CapturePoints.Count < numPoints)
            numPoints = m_CapturePoints.Count;

        for (int i = 0; i < numPoints; i++)
        {
            caps = m_CapturePoints[m_CapturePoints.Count - numPoints + i];
            positions[i] = caps[captureDevice].worldSpacePosition + offset;
        }

        return positions;
    }
}



// LOOK INTO OVRBOUNDARY - BoundaryTestResult -> seems to report world position of hands
// * NO, this actually returns the closest point of the boundary to the hand
// you could query OVRCameraRig trackingSpace, which returns a tranform of all anchors

// FOR MOTION CAPTURE WITH KINECT -> use OVRTracker, which provides offsets for tracker