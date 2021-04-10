// handles reports of virtual drum collisions and combines
// this with MIDI hits in order to properly correlate for capture motion
// could possibly even automatically re-arrange drums based on correlated hits
// in world space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class DrumCollisionManager : MonoBehaviour
{
    // how long system waits before clearing collision results
    public float m_CorrTimeout = 0.2f;

    public Transform m_DrumStickTipL;
    public Transform m_DrumStickTipR;

    Coroutine m_MidiTimeout;
    Coroutine m_CollisionTimeout;

    NoteCallback ncb;

    int m_LastNote = 0;
    // whatever
    int m_LastNoteCollision = 0;
    int m_LastNoteMidi = 0;
    int m_LastVelocity = 0;

    bool m_CalibratingPositions = false;

    float m_LastMidiTime;
    float m_LastCollisionTime;

    Vector3 m_LastCollisionPos;
    Vector2Int m_LastNoteHit;
    GameObject m_LastDrumObject;

    OVRInput.Controller m_LastController = OVRInput.Controller.None;

    // contains the mappings for note number to string tag
    Dictionary<string, int> MidiMappings = new Dictionary<string, int>()
    {
        { "kick",  36 },
        { "snare", 38 },
        { "hihat", 46 },
        { "tomhi", 48 },
        { "tommid", 45 },
        { "tomfloor", 43 },
        { "crash", 49 }, // 55 for rim
        { "ride", 51 } // 59 for rim
    };

    void Start()
    {
        m_LastNoteHit = new Vector2Int(0, 0);
        ncb = GetComponent<NoteCallback>();
    }

    // returns the most recent collision stat
    // and resets it to zero (for use by captureMotion)
    public Vector2Int GetLastNoteHit(OVRInput.Controller c)
    {
        if (c == m_LastController)
        {
            Vector2Int lastNote = m_LastNoteHit;
            m_LastNoteHit = new Vector2Int(0, 0);
            return lastNote;
        } else
        {
            return new Vector2Int(0, 0);
        }
    }

    // really dumb and unfortunate way of doing this
    // unfortunately I can't figure out how to get the external
    // midi library to sucessfully call an IEnumerator from its callback
    private void Update()
    {
        bool midiNoteThisFrame = false;

        Vector2Int midiNote = ncb.GetLastNoteRecieved();

        if (midiNote.x != 0)
        {
            midiNoteThisFrame = true;
            m_LastMidiTime = Time.time;
            if (m_MidiTimeout != null)
                StopCoroutine(m_MidiTimeout);
            m_MidiTimeout = StartCoroutine(MidiEventTimeout(midiNote.x, midiNote.y));
        }

        if (m_MidiTimeout != null && m_CollisionTimeout != null && m_LastNoteCollision == m_LastNoteMidi)
        {
            StopCoroutine(m_MidiTimeout);
            StopCoroutine(m_CollisionTimeout);
            m_MidiTimeout = null;
            m_CollisionTimeout = null;
            VerifiedCollisionCallback(m_LastNoteMidi, 
                                        m_LastVelocity, 
                                        m_LastController,
                                        m_LastDrumObject, 
                                        m_LastCollisionPos,
                                        midiNoteThisFrame);
        }
    }

    void VerifiedCollisionCallback(int note, 
                                    int velocity, 
                                    OVRInput.Controller c, 
                                    GameObject drum, 
                                    Vector3 collisionPos,
                                    bool collidedBeforeMidi)
    {
        string drumTag = MidiMappings.FirstOrDefault(x => x.Value == note).Key;
        print(drumTag + " collision. Note: " + note + " velocity: " + velocity + " controller: " + c);
        m_LastController = c;
        m_LastNoteHit = new Vector2Int(note, velocity);

        if (m_CalibratingPositions && collidedBeforeMidi)
            CalibratePosition(drum, collisionPos, c);
    }


    // we only want to adjust if unity collision happens before midi trigger
    // because presumably the stick is physically blocked from moving further
    // in the world space by the drum head IRL
    void CalibratePosition(GameObject drum, 
                            Vector3 collisionPos,
                            OVRInput.Controller c)
    {
        Transform stickTip = MatchControllerToStick(c);
        Vector3 newOffset = stickTip.position - collisionPos;

        // only move in the y direction
        newOffset.x = 0;
        newOffset.z = 0;

        drum.transform.position += newOffset;

    }

    IEnumerator MidiEventTimeout(int note, int velocity)
    {
        //m_LastNote = note;
        m_LastNoteMidi = note;
        m_LastVelocity = velocity;
        yield return new WaitForSeconds(m_CorrTimeout);
        m_MidiTimeout = null;
        print("MIDI TIMED OUT!");
    }

    IEnumerator CollisionEventTimeout(int note, OVRInput.Controller c)
    {
        //m_LastNote = note;
        m_LastNoteCollision = note;
        m_LastController = c;
        yield return new WaitForSeconds(m_CorrTimeout);
        m_CollisionTimeout = null;
    }

    // reports a midi event
    //public void ReportMidiNote(Vector2Int note)
    //{
    //    print("MIDI NOTE HIT! " + note);

    //    if (m_MidiTimeout != null)
    //        StopCoroutine(m_MidiTimeout);

    //    if (m_CollisionTimeout != null)
    //    {

    //        StopCoroutine(m_CollisionTimeout);
    //        print("MIDI - m_LastNote=" + m_LastNote + " this note=" + note.x);

    //        // in this case, the collision has already happened in unity
    //        if (m_LastNote == note.x)
    //        {
    //            VerifiedCollisionCallback(note.x, note.y, m_LastController);
    //        }
    //        else
    //        {
    //            // if last note doesn't equal, start a new coroutine
    //            m_MidiTimeout = StartCoroutine(MidiEventTimeout(note.x, note.y));
    //        }
    //    }
    //    else
    //    {
    //        m_MidiTimeout = StartCoroutine(MidiEventTimeout(note.x, note.y));
    //    }
    //}

    // reports when virtual drum stick collides with a drum
    public void ReportCollision(string parentName, 
                                string tag, 
                                GameObject drum, 
                                Vector3 collisionPosition)
    {
        m_LastDrumObject = drum;
        m_LastCollisionPos = collisionPosition;
        m_LastCollisionTime = Time.time;

        try
        {
            int note = MidiMappings[tag];
            OVRInput.Controller c = MatchController(parentName);

            if (m_CollisionTimeout != null)
                StopCoroutine(m_CollisionTimeout);
            m_CollisionTimeout = StartCoroutine(CollisionEventTimeout(note, c));

        } catch (Exception e) // in case tag doesn't match midi mappings
        {
            print("ERROR with collision report: " + e);
        }
    }

    OVRInput.Controller MatchController(string name)
    {
        switch (name)
        {
            case "LeftHandAnchor":
                return OVRInput.Controller.LTouch;
            case "RightHandAnchor":
                return OVRInput.Controller.RTouch;
            default:
                return OVRInput.Controller.None;
        }
    }

    Transform MatchControllerToStick(OVRInput.Controller c)
    {
        switch (c)
        {
            case OVRInput.Controller.LTouch:
                return m_DrumStickTipL;
            case OVRInput.Controller.RTouch:
                return m_DrumStickTipR;
            default: 
                return null;
        }
    }

    //Transform Match

    public void CalibrateDrumPlacement(bool toggle)
    {
        m_CalibratingPositions = toggle;
    }
}
