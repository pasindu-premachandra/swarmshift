using System;

public interface IPlatformLifecycle
{
    bool IsPaused { get; }
    bool IsAudioEnabled { get; }

    event Action Paused;
    event Action Resumed;
    event Action<bool> AudioEnabledChanged;

    void FirstFrameReady();
    void GameReady();
}
