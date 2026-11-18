using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// The template calls FBInstant.startGameAsync once Unity has loaded, so the ready signals have nothing to send here.
public class MetaLifecycle : IPlatformLifecycle
{
    readonly List<InputDevice> disabledDevices = new List<InputDevice>();
    float timeScaleBeforePause = 1f;

    public bool IsPaused { get; private set; }
    public bool IsAudioEnabled => true;

    public event Action Paused;
    public event Action Resumed;
    public event Action<bool> AudioEnabledChanged { add { } remove { } }

    public MetaLifecycle(MetaBridge bridge)
    {
        bridge.PauseRequested += Pause;
        bridge.ResumeRequested += Resume;
        bridge.ListenForPause();
    }

    public void FirstFrameReady()
    {
    }

    public void GameReady()
    {
    }

    void Pause()
    {
        if (IsPaused)
        {
            return;
        }

        IsPaused = true;
        timeScaleBeforePause = Time.timeScale;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        foreach (InputDevice device in InputSystem.devices)
        {
            if (device.enabled)
            {
                InputSystem.DisableDevice(device); // a disabled device sits in its default state, so nothing reads as pressed
                disabledDevices.Add(device);
            }
        }

        Paused?.Invoke();
    }

    void Resume()
    {
        if (!IsPaused)
        {
            return;
        }

        IsPaused = false;
        foreach (InputDevice device in disabledDevices)
        {
            if (device.added)
            {
                InputSystem.EnableDevice(device);
            }
        }

        disabledDevices.Clear();
        AudioListener.pause = false;
        Time.timeScale = timeScaleBeforePause;
        Resumed?.Invoke();
    }
}
