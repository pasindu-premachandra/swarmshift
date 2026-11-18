using System;
using UnityEngine;
using YTGameSDK;

public class PlayablesSaveService : ISaveService
{
    readonly YTGameWrapper wrapper;
    Action<string> waiting;
    bool loadStarted;
    bool loadFinished;
    bool loadSucceeded;
    string data = "";

    public PlayablesSaveService(YTGameWrapper wrapper)
    {
        this.wrapper = wrapper;
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
            wrapper.LoadGameSaveData(OnLoaded, OnLoadFailed);
        }
    }

    public void Save(string json, Action<bool> onDone)
    {
        if (!loadSucceeded)
        {
            onDone(false); // YouTube rejects saveData until loadData has succeeded, so it is not sent
            return;
        }

        wrapper.SendGameSaveData(json, onDone);
    }

    void OnLoaded(string loaded)
    {
        loadSucceeded = true;
        Finish(loaded);
    }

    void OnLoadFailed()
    {
        Debug.LogWarning("[Platform] loadData failed, starting with default progress");
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
