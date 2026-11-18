using System;
using UnityEngine;

// Web build: progress lives in the browser (PlayerPrefs is IndexedDB on WebGL).
public class WebSaveService : ISaveService
{
    const string Key = "save";

    bool loaded;

    public void Load(Action<string> onLoaded)
    {
        loaded = true;
        onLoaded(PlayerPrefs.GetString(Key, ""));
    }

    public void Save(string json, Action<bool> onDone)
    {
        if (!loaded)
        {
            onDone(false);
            return;
        }

        PlayerPrefs.SetString(Key, json);
        PlayerPrefs.Save();
        onDone(true);
    }
}
