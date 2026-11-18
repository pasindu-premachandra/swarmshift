using System.Collections;
using UnityEngine;
using YTGameSDK;

[RequireComponent(typeof(YTGameWrapper))]
public class PlatformServices : MonoBehaviour
{
    public static PlatformServices Instance { get; private set; }

    public IAdService Ads { get; private set; }
    public ISaveService Save { get; private set; }
    public IPlatformLifecycle Lifecycle { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject); // a level restart reloads the scene, which brings a second copy of this object
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
#if UNITY_WEBGL && !UNITY_EDITOR && SWARMSHIFT_META
        MetaBridge bridge = gameObject.AddComponent<MetaBridge>();
        Ads = new NoAdService();
        Save = new MetaSaveService(bridge);
        Lifecycle = new MetaLifecycle(bridge);
#elif UNITY_WEBGL && !UNITY_EDITOR && SWARMSHIFT_WEB
        Ads = new NoAdService();
        Save = new WebSaveService();
        Lifecycle = new WebLifecycle();
#elif UNITY_WEBGL && !UNITY_EDITOR
        YTGameWrapper wrapper = GetComponent<YTGameWrapper>();
        Ads = new PlayablesAdService(wrapper);
        Save = new PlayablesSaveService(wrapper);
        Lifecycle = new PlayablesLifecycle(wrapper);
#else
        Ads = new EditorAdService();
        Save = new EditorSaveService();
        Lifecycle = new EditorLifecycle();
#endif
        StartCoroutine(SignalFirstFrame());
    }

    IEnumerator SignalFirstFrame()
    {
        yield return new WaitForEndOfFrame();
        Lifecycle.FirstFrameReady();
    }
}
