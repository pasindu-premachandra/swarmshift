using System;
using UnityEngine;

public class EditorLifecycle : IPlatformLifecycle
{
    bool firstFrameSent;
    bool gameReadySent;

    public bool IsPaused => false;
    public bool IsAudioEnabled => true;

    // Nothing outside the Editor pauses or mutes it, so these never fire.
    public event Action Paused { add { } remove { } }
    public event Action Resumed { add { } remove { } }
    public event Action<bool> AudioEnabledChanged { add { } remove { } }

    public void FirstFrameReady()
    {
        if (firstFrameSent)
        {
            return;
        }

        firstFrameSent = true;
        Debug.Log("[Platform] Editor: first frame ready");
    }

    public void GameReady()
    {
        if (gameReadySent)
        {
            return;
        }

        gameReadySent = true;
        FirstFrameReady(); // YouTube requires firstFrameReady before gameReady
        Debug.Log("[Platform] Editor: game ready");
    }
}
