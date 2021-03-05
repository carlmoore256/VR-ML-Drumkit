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
        public Vector3 velocity;
        public Vector3 acceleration;
        public Vector3 angularVel;
        public Vector3 angularAccel;
        public Quaternion rotation;

        public CapturePoint(OVRInput.Controller controller, Vector3 position, Vector3 velocity, Vector3 acceleration, Vector3 angularVel, Vector3 angularAccel, Quaternion rotation)
        {
            this.controller = controller;
            this.position = position;
            this.velocity = velocity;
            this.acceleration = acceleration;
            this.angularVel = angularVel;
            this.angularAccel = angularAccel;
            this.rotation = rotation;
        }
    }

    // sample rate for capture of motion data from controllers
    public int sampleRate = 500;

    public string saveCaptureDir = "Assets/Data";

    private double sampleInterval;
    Thread captureThread = null;
    List<CapturePoint[]> capturePoints;
    OVRInput.Controller[] trackedControllers;
    // public float timeout = 5f;
    // bool tof = false;
    public bool capRunning;

    void Start()
    {
        sampleInterval = 1.0d / sampleRate;
        
        trackedControllers = new OVRInput.Controller[] { OVRInput.Controller.LTrackedRemote, OVRInput.Controller.RTrackedRemote };
    }
    
    // can be called externally to begin the capture
    public void StartCapture()
    {
        captureThread = new Thread(RunCapture);
        captureThread.Start();
        capRunning = true;
    }

    public void EndCapture()
    {
        captureThread.Abort();
        print("capture thread ended successfully, saving data...");
        WriteMotionData();
        capRunning = false;
    }

    // main loop to sample controller data
    void RunCapture()
    {
        capturePoints = new List<CapturePoint[]>();
        double nextCap = AudioSettings.dspTime + sampleInterval;

        while(true)
        {
            // maybe not the best way to sample but here for now
            double now = AudioSettings.dspTime;

            if(now >= nextCap)
            {
                CapturePoint[] ctrlPoints = new CapturePoint[trackedControllers.Length];
                int index = 0;

                // controllerPoints[0].position = new Vector3(10f, 25f, 0.5f);
                // controllerPoints[1].position = new Vector3(50f, 55f, 5.5f);

                foreach(OVRInput.Controller c in trackedControllers)
                {
                    ctrlPoints[index].controller = c;
                    ctrlPoints[index].position = OVRInput.GetLocalControllerPosition(c);
                    ctrlPoints[index].velocity = OVRInput.GetLocalControllerVelocity(c);
                    ctrlPoints[index].acceleration = OVRInput.GetLocalControllerAcceleration(c);
                    ctrlPoints[index].angularVel = OVRInput.GetLocalControllerAngularVelocity(c);
                    ctrlPoints[index].angularAccel = OVRInput.GetLocalControllerAngularAcceleration(c);
                    ctrlPoints[index].rotation = OVRInput.GetLocalControllerRotation(c);
                    index++;
                }
                capturePoints.Add(ctrlPoints);
                nextCap = now + sampleInterval;
            }
        }
        // if(!tof && Time.time > timeout)
    }

    void Update()
    {
        //Vector3 pos = OVRInput.GetLocalControllerPosition(c);
        // {
        //     print("TIME RAN OUT!!!");
        //     EndCapture();
        // }
    }


    void OnDestroy()
    {
        if(captureThread != null)
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

        for(int i = 0; i < trackedControllers.Length; i++)
        {
            var pos = new StringBuilder();
            var vel = new StringBuilder();
            var acc = new StringBuilder();
            var a_vel = new StringBuilder();
            var a_acc = new StringBuilder();

            foreach(CapturePoint[] cp in capturePoints)
            {
                pos.AppendLine(ParseVector(cp[i].position));
                vel.AppendLine(ParseVector(cp[i].velocity));
                acc.AppendLine(ParseVector(cp[i].acceleration));
                a_vel.AppendLine(ParseVector(cp[i].angularVel));
                a_acc.AppendLine(ParseVector(cp[i].angularAccel));
            }
            
            File.WriteAllText(System.IO.Path.Combine(saveDir, $"pos_c{i}.csv"), pos.ToString());
            File.WriteAllText(System.IO.Path.Combine(saveDir, $"vel_c{i}.csv"), vel.ToString());
            File.WriteAllText(System.IO.Path.Combine(saveDir, $"acc_c{i}.csv"), acc.ToString());
            File.WriteAllText(System.IO.Path.Combine(saveDir, $"a_vel_c{i}.csv"), a_vel.ToString());
            File.WriteAllText(System.IO.Path.Combine(saveDir, $"a_acc_c{i}.csv"), a_acc.ToString());

            print("data written to " + saveDir);
        }
    }

    public string[] Vector3toString(Vector3 v)
    { 
        return new string[3] { v.x.ToString(), v.y.ToString(), v.z.ToString() }; 
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
    public Vector3[] GetPositions(int numPoints, int captureDevice)
    {
        Vector3[] positions = new Vector3[numPoints];
        if(capturePoints.Count >= numPoints)
        {
            int cpCount = capturePoints.Count;
            for(int i = 0; i < numPoints; i++)
                positions[i] = capturePoints[cpCount - numPoints + i][captureDevice].position;
                
            return positions;
        } else {
            return null;
        }
    }
}

// LOOK INTO OVRBOUNDARY - BoundaryTestResult -> seems to report world position of hands
// * NO, this actually returns the closest point of the boundary to the hand
// you could query OVRCameraRig trackingSpace, which returns a tranform of all anchors

// FOR MOTION CAPTURE WITH KINECT -> use OVRTracker, which provides offsets for tracker