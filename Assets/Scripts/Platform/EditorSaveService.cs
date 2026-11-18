using System;
using UnityEngine;

public class EditorSaveService : ISaveService
{
    bool loaded;

    public void Load(Action<string> onLoaded)
    {
        if (!loaded)
        {
            loaded = true;
            Debug.Log("[Platform] Editor: load, nothing stored, starting with default progress");
        }

        onLoaded("");
    }

    public void Save(string json, Action<bool> onDone)
    {
        if (!loaded)
        {
            Debug.Log("[Platform] Editor: save refused, load has not finished");
            onDone(false);
            return;
        }

        Debug.Log($"[Platform] Editor: save logged, not stored: {json}");
        onDone(true);
    }
}
