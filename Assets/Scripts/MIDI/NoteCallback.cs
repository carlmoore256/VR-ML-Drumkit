using System;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;
using UnityEngine;

public class NoteCallback : MonoBehaviour
{
    InputDevice m_InputDevice;

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

    //private void Update()
    //{
    //    print("IS LISTENING? " + m_InputDevice.IsListeningForEvents);
    //}

    private void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
    {
        var midiDevice = (MidiDevice)sender;
        print("TEST!");
        print($"Event received from '{midiDevice.Name}' at {DateTime.Now}: {e.Event}");
    }
}