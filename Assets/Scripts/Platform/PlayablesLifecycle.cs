using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using YTGameSDK;

public class PlayablesLifecycle : IPlatformLifecycle
{
    readonly YTGameWrapper wrapper;
    readonly List<InputDevice> disabledDevices = new List<InputDevice>();
    bool firstFrameSent;
    bool gameReadySent;
    float timeScaleBeforePause = 1f;

    public bool IsPaused { get; private set; }
    public bool IsAudioEnabled { get; private set; }

    public event Action Paused;
    public event Action Resumed;
    public event Action<bool> AudioEnabledChanged;

#if UNITY_WEBGL && !UNITY_EDITOR
    const int PausedRenderInterval = 100000; // render one frame in this many, so none while paused
    int renderIntervalBeforePause = 1;

    [DllImport("__Internal")]
    static extern void PauseMainLoop();

    [DllImport("__Internal")]
    static extern void ResumeMainLoop();
#endif

    public PlayablesLifecycle(YTGameWrapper wrapper)
    {
        this.wrapper = wrapper;
        wrapper.SetOnPauseCallback(Pause);
        wrapper.SetOnResumeCallback(Resume);
        wrapper.SetOnAudioEnabledChangeCallback(SetAudioEnabled);
        SetAudioEnabled(wrapper.IsYTGameAudioEnabled());
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus; // only the SDK pauses; a focus change while paused cost the next tap
    }

    public void FirstFrameReady()
    {
        if (firstFrameSent)
        {
            return;
        }

        firstFrameSent = true;
        wrapper.SendGameFirstFrameReady();
    }

    public void GameReady()
    {
        if (gameReadySent)
        {
            return;
        }

        gameReadySent = true;
        FirstFrameReady(); // YouTube requires firstFrameReady before gameReady
        wrapper.SendGameIsReady();
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
#if UNITY_WEBGL && !UNITY_EDITOR
        renderIntervalBeforePause = OnDemandRendering.renderFrameInterval;
        OnDemandRendering.renderFrameInterval = PausedRenderInterval; // Unity's background timer still ticks about once a second
        PauseMainLoop(); // the paused frame stays on screen and nothing is redrawn
#endif
    }

    void Resume()
    {
        if (!IsPaused)
        {
            return;
        }

        IsPaused = false;
#if UNITY_WEBGL && !UNITY_EDITOR
        OnDemandRendering.renderFrameInterval = renderIntervalBeforePause;
        ResumeMainLoop();
#endif
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

    void SetAudioEnabled(bool enabled)
    {
        IsAudioEnabled = enabled;
        AudioListener.volume = enabled ? 1f : 0f;
        AudioEnabledChanged?.Invoke(enabled);
    }
}
