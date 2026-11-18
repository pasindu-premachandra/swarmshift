using System;

public interface ISaveService
{
    // Loads once per session; later calls get the first result. Empty when nothing is saved or the load failed.
    void Load(Action<string> onLoaded);

    // Reports whether the platform stored the data; refused until a load has succeeded.
    void Save(string json, Action<bool> onDone);
}
