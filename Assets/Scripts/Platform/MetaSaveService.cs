using System;
using UnityEngine;

public class MetaSaveService : ISaveService
{
    readonly MetaBridge bridge;
    Action<string> waiting;
    bool loadStarted;
    bool loadFinished;
    bool loadSucceeded;
    string data = "";

    public MetaSaveService(MetaBridge bridge)
    {
        this.bridge = bridge;
    }

    public void Load(Action<string> onLoaded)
    {
        if (loadFinished)
        {
            onLoaded(data);
            return;
        }

        waiting += onLoaded;
        if (!loadStarted)
        {
            loadStarted = true;
            bridge.Load(OnLoaded, OnLoadFailed);
        }
    }

    public void Save(string json, Action<bool> onDone)
    {
        if (!loadSucceeded)
        {
            onDone(false); // a failed load must not be overwritten by default progress
            return;
        }

        bridge.Save(json, onDone);
    }

    void OnLoaded(string loaded)
    {
        loadSucceeded = true;
        Finish(loaded);
    }

    void OnLoadFailed()
    {
        Debug.LogWarning("[Platform] getDataAsync failed, starting with default progress");
        Finish("");
    }

    void Finish(string loaded)
    {
        data = loaded;
        loadFinished = true;
        Action<string> callbacks = waiting;
        waiting = null;
        callbacks?.Invoke(data);
    }
}
