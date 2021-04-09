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

    Coroutine m_MidiTimeout;
    Coroutine m_CollisionTimeout;

    NoteCallback ncb;

    int m_LastNote = 0;
    // whatever
    int m_LastNoteCollision = 0;
    int m_LastNoteMidi = 0;
    int m_LastVelocity = 0;

    Vector2Int m_LastNoteHit;

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
            return m_LastNoteHit;
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
        Vector2Int midiNote = ncb.GetLastNoteRecieved();

        if (midiNote.x != 0)
        {
            if (m_MidiTimeout != null)
                StopCoroutine(m_MidiTimeout);
            m_MidiTimeout = StartCoroutine(MidiEventTimeout(midiNote.x, midiNote.y));
        }

        if (m_MidiTimeout != null && m_CollisionTimeout != null && m_LastNoteCollision == m_LastNoteMidi)
        {
            StopCoroutine(m_MidiTimeout);
            StopCoroutine(m_CollisionTimeout);
            VerifiedCollisionCallback(m_LastNoteMidi, m_LastVelocity, m_LastController);
        }
        //ReportMidiNote(midiNote);

        //if (midiNote.x != 0 && m_CollisionTimeout != null)
        //{
        //    print("MIDI NOTE NOT ZERO, TIMEOUT STILL GOING " + midiNote);
        //    StopCoroutine(m_CollisionTimeout);

        //    if(midiNote.x != 0 && m_LastNote == midiNote.x)
        //    {
        //        m_LastVelocity = midiNote.y;
        //        VerifiedCollisionCallback(midiNote.x, midiNote.y, m_LastController);
        //    }
        //}
    }

    void VerifiedCollisionCallback(int note, int velocity, OVRInput.Controller c)
    {
        string drumTag = MidiMappings.FirstOrDefault(x => x.Value == note).Key;
        print(drumTag + " collision. Note: " + note + " velocity: " + velocity + " controller: " + c);

        m_LastController = c;
        m_LastNoteHit = new Vector2Int(note, velocity);
    }

    IEnumerator MidiEventTimeout(int note, int velocity)
    {
        //m_LastNote = note;
        m_LastNoteMidi = note;
        m_LastVelocity = velocity;
        yield return new WaitForSeconds(m_CorrTimeout);
    }

    IEnumerator CollisionEventTimeout(int note, OVRInput.Controller c)
    {
        //m_LastNote = note;
        m_LastNoteCollision = note;
        m_LastController = c;
        yield return new WaitForSeconds(m_CorrTimeout);
    }

    // reports a midi event
    public void ReportMidiNote(Vector2Int note)
    {
        print("MIDI NOTE HIT! " + note);

        if (m_MidiTimeout != null)
            StopCoroutine(m_MidiTimeout);

        if (m_CollisionTimeout != null)
        {

            StopCoroutine(m_CollisionTimeout);
            print("MIDI - m_LastNote=" + m_LastNote + " this note=" + note.x);

            // in this case, the collision has already happened in unity
            if (m_LastNote == note.x)
            {
                VerifiedCollisionCallback(note.x, note.y, m_LastController);
            }
            else
            {
                // if last note doesn't equal, start a new coroutine
                m_MidiTimeout = StartCoroutine(MidiEventTimeout(note.x, note.y));
            }
        }
        else
        {
            m_MidiTimeout = StartCoroutine(MidiEventTimeout(note.x, note.y));
        }
    }

    // reports when virtual drum stick collides with a drum
    public void ReportCollision(string parentName, string tag)
    {
        try
        {
            int note = MidiMappings[tag];
            OVRInput.Controller c = MatchController(parentName);

            if (m_CollisionTimeout != null)
                StopCoroutine(m_CollisionTimeout);
            m_CollisionTimeout = StartCoroutine(CollisionEventTimeout(note, c));

            //if (m_MidiTimeout != null)
            //{
            //    StopCoroutine(m_MidiTimeout);
            //    print("COLLISION - m_LastNote=" + m_LastNote + " this note=" + note);
            //    // no way this could be true twice in a row, because we stop midi timeout
            //    if (m_LastNote == note)
            //    {
            //        VerifiedCollisionCallback(note, m_LastVelocity, c);
            //    }
            //    else
            //    {
            //        m_CollisionTimeout = StartCoroutine(CollisionEventTimeout(note, c));
            //    }
            //}
            //else
            //{
            //    m_CollisionTimeout = StartCoroutine(CollisionEventTimeout(note, c));
            //}

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

    public void CalibrateDrumPlacement(bool toggle)
    {

    }
}
