using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

// Receives the Facebook Instant Games results that SwarmshiftMeta.jslib sends to this object.
public class MetaBridge : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    static extern void MetaLoadData();

    [DllImport("__Internal")]
    static extern void MetaSaveData(string json);

    [DllImport("__Internal")]
    static extern void MetaListenForPause();
#endif

    readonly Queue<Action<bool>> saveCallbacks = new Queue<Action<bool>>();
    Action<string> onLoaded;
    Action onLoadFailed;

    public event Action PauseRequested;
    public event Action ResumeRequested;

    public void ListenForPause()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        MetaListenForPause();
#endif
    }

    public void Load(Action<string> loaded, Action failed)
    {
        onLoaded = loaded;
        onLoadFailed = failed;
#if UNITY_WEBGL && !UNITY_EDITOR
        MetaLoadData();
#else
        ReceiveMetaLoadFailed();
#endif
    }

    public void Save(string json, Action<bool> onDone)
    {
        saveCallbacks.Enqueue(onDone);
#if UNITY_WEBGL && !UNITY_EDITOR
        MetaSaveData(json);
#else
        ReceiveMetaSaved("false");
#endif
    }

    void ReceiveMetaLoaded(string data)
    {
        onLoaded?.Invoke(data);
    }

    void ReceiveMetaLoadFailed()
    {
        onLoadFailed?.Invoke();
    }

    void ReceiveMetaSaved(string saved)
    {
        if (saveCallbacks.Count > 0)
        {
            saveCallbacks.Dequeue()(saved == "true");
        }
    }

    void ReceiveMetaPause()
    {
        PauseRequested?.Invoke();
    }

    void ReceiveMetaResume()
    {
        ResumeRequested?.Invoke();
    }
}
