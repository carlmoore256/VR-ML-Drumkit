using System;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;
using UnityEngine;

public class NoteCallback : MonoBehaviour
{
    InputDevice m_InputDevice;

    Vector2Int m_LastNote;

    int mCurrentVel; 

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
    }

    private void OnApplicationQuit()
    {
        (m_InputDevice as IDisposable)?.Dispose();
    }

    // this should correlate note hit to the respective controller
    // capture thread accesses this and saves with motion data
    public Vector2Int GetLastNoteRecieved()
    {
        Vector2Int notePlayed = m_LastNote;

        m_LastNote = new Vector2Int(0, 0);
        
        return notePlayed;
    }

    //private void Update()
    //{
    //    print("IS LISTENING? " + m_InputDevice.IsListeningForEvents);
    //}

    private void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
    {
        var midiDevice = (MidiDevice)sender;
        print($"Event received from '{midiDevice.Name}' at {DateTime.Now}: {e.Event}");

        // consider pulling DeltaTime from event and logging it
        if(e.Event.EventType.Equals(MidiEventType.NoteOn))
        {
            NoteEvent noteEvent = (NoteEvent)e.Event;
            m_LastNote = new Vector2Int(noteEvent.NoteNumber, noteEvent.Velocity);
        }
    }
}