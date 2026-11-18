using System;

// Web build: no host pauses or mutes the game; a hidden tab already stops drawing.
public class WebLifecycle : IPlatformLifecycle
{
    public bool IsPaused => false;
    public bool IsAudioEnabled => true;

    public event Action Paused { add { } remove { } }
    public event Action Resumed { add { } remove { } }
    public event Action<bool> AudioEnabledChanged { add { } remove { } }

    public void FirstFrameReady()
    {
    }

    public void GameReady()
    {
    }
}
