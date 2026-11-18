#if UNITY_WEBGL && !UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

// Development Web builds only: lets a proof script switch lane 1's Autopilot on from the page.
public class DevAutopilotHook : MonoBehaviour
{
    bool keepOn;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
    {
        if (!Debug.isDebugBuild)
        {
            return;
        }

        var hook = new GameObject("DevAutopilotHook");
        DontDestroyOnLoad(hook);
        hook.AddComponent<DevAutopilotHook>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    [Preserve] // called from the page through SendMessage
    public void EnableAutopilot()
    {
        keepOn = true;
        SwitchOn();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (keepOn)
        {
            SwitchOn();
        }
    }

    static void SwitchOn()
    {
        Autopilot autopilot = FindAnyObjectByType<Autopilot>();
        if (autopilot != null)
        {
            autopilot.enabled = true;
        }
    }
}
#endif
