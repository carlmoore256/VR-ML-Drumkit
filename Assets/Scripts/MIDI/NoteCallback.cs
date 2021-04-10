using System;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class NoteCallback : MonoBehaviour
{
    InputDevice m_InputDevice;

    Vector2Int m_LastNote;

    int mCurrentVel;

    DrumCollisionManager dcm;

    public float m_HiHatPedal;

    float m_LastNotePlayTime;

    private void Start()
    {
        print("STARTING MIDI SERVICE!");
        var devices = InputDevice.GetAll();

        foreach (var d in devices)
        {
            print("DEVICE : " + d);
            var inputDevice = d;
            inputDevice.EventReceived += OnEventReceived;
            inputDevice.StartEventsListening();
            m_InputDevice = inputDevice;
            break;
        }

        dcm = GetComponent<DrumCollisionManager>();
    }

    private void OnApplicationQuit()
    {
        (m_InputDevice as IDisposable)?.Dispose();
    }

    // this should correlate note hit to the respective controller
    // capture thread accesses this and saves with motion data
    public Vector2Int GetLastNoteRecieved(bool reset = true)
    {
        Vector2Int notePlayed = m_LastNote;

        if (reset)
            m_LastNote = new Vector2Int(0, 0);
        
        return notePlayed;
    }

    // doesn't work
    public float GetLastNotePlaytime()
    {
        return m_LastNotePlayTime;
    }

    private void Update()
    {
        //print("IS LISTENING? " + m_InputDevice.IsListeningForEvents);
    }

    private void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
    {
        var midiDevice = (MidiDevice)sender;
        //print($"Event received from '{midiDevice.Name}' at {DateTime.Now}: {e.Event}");

        // consider pulling DeltaTime from event and logging it
        if(e.Event.EventType.Equals(MidiEventType.NoteOn))
        {
            NoteEvent noteEvent = (NoteEvent)e.Event;
            //m_LastNotePlayTime = Time.time;
            m_LastNote = new Vector2Int(noteEvent.NoteNumber, noteEvent.Velocity);
            // this won't work unfortunately because of the external library not having a definition of ienumerator
            //dcm.ReportMidiNote(new Vector2Int(noteEvent.NoteNumber, noteEvent.Velocity));
        }

        if(e.Event.EventType.Equals(MidiEventType.ControlChange))
        {
            ControlChangeEvent controlChange = (ControlChangeEvent)e.Event;
            // implement hihat movement here
            print(controlChange);
            if (controlChange.ControlNumber == 4)
            {
                m_HiHatPedal = (float)controlChange.ControlValue / 128f;
            }
        }
    }
}