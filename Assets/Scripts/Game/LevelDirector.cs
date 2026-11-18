using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)] // Awake fits the view before Field.Awake builds its card from the camera
public class LevelDirector : MonoBehaviour
{
    enum State
    {
        Playing,
        Won,
        Lost
    }

    [SerializeField] Mothership mothership;
    [SerializeField] Swarm swarm;
    [SerializeField] MiteWaves mites;
    [SerializeField] Hive hive;
    [SerializeField] Champion champion;
    [SerializeField] Gate[] gates;
    [SerializeField] WarpRing[] rings;
    [SerializeField] Autopilot autopilot;
    [SerializeField] Field field;
    [SerializeField] Material haloMaterial;
    [SerializeField] TextMeshProUGUI resultText;
    [SerializeField] GameObject failedButtons;
    [SerializeField] Button reviveButton;
    [SerializeField] Button retryButton;
    [SerializeField] float contactRadius = 0.52f;
    [SerializeField] float restartDelay = 0.5f;
    [SerializeField] float maxStep = 0.05f;

    static readonly Bounds WorldBounds = new Bounds(new Vector3(0f, 1f, 18f), new Vector3(14f, 8f, 50f));
    const string ReviveRewardId = "revive";
    const float DesignAspect = 390f / 844f;
    const string FinalWinMessage = "HIVE CRACKED\n<size=60%>ALL LEVELS CLEARED</size>";

    static SaveData progress; // read from the first load of the session, then kept across scene loads

    State state;
    Halos halos;
    Camera view;
    float designFieldOfView;
    float resultTime;
    float simTime;
    bool loaded;
    bool reviveUsed;
    bool reviveWaiting;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetSession()
    {
        progress = null; // the Editor keeps statics between Play sessions
    }

    void Awake()
    {
        view = Camera.main;
        designFieldOfView = view.fieldOfView;
        FitView();
        halos = new Halos(haloMaterial, view.transform);
        reviveButton.onClick.AddListener(Revive);
        retryButton.onClick.AddListener(Retry);
    }

    void Start()
    {
        PlatformServices.Instance.Lifecycle.Paused += OnPaused;
        PlatformServices.Instance.Save.Load(OnSaveLoaded);
    }

    void OnDestroy()
    {
        if (PlatformServices.Instance != null)
        {
            PlatformServices.Instance.Lifecycle.Paused -= OnPaused;
        }
    }

    void OnSaveLoaded(string data)
    {
        if (progress == null)
        {
            progress = ReadProgress(data);
            int next = Mathf.Clamp(progress.highestLevelWon, 0, SceneManager.sceneCountInBuildSettings) % SceneManager.sceneCountInBuildSettings;
            Debug.Log($"[Level] Progress loaded: highest level won {progress.highestLevelWon}, next level {next + 1}");
            if (SceneManager.GetActiveScene().buildIndex == 0 && next != 0)
            {
                SceneManager.LoadScene(next); // a session starts in the first scene; continue at the next level not yet won
                return;
            }
        }

        loaded = true;
        PlatformServices.Instance.Lifecycle.GameReady();
    }

    static SaveData ReadProgress(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            return new SaveData();
        }

        try
        {
            return JsonUtility.FromJson<SaveData>(data) ?? new SaveData();
        }
        catch (System.ArgumentException)
        {
            Debug.LogWarning("[Level] Saved progress unreadable, starting with default progress");
            return new SaveData();
        }
    }

    void SaveProgress(string reason)
    {
        PlatformServices.Instance.Save.Save(JsonUtility.ToJson(progress), saved => Debug.Log($"[Level] Progress {(saved ? "saved" : "not saved")} ({reason})"));
    }

    void OnPaused()
    {
        if (progress != null)
        {
            SaveProgress("pause");
        }
    }

    // A frame narrower than the 390 x 844 design keeps the design's width of view, so the whole lane stays on screen.
    void FitView()
    {
        float aspect = view.aspect;
        if (!(aspect > 0f) || float.IsInfinity(aspect))
        {
            return; // a 0 height frame while YouTube loads the game
        }

        float fieldOfView = designFieldOfView;
        if (aspect < DesignAspect)
        {
            fieldOfView = 2f * Mathf.Atan(Mathf.Tan(designFieldOfView * 0.5f * Mathf.Deg2Rad) * DesignAspect / aspect) * Mathf.Rad2Deg;
        }
        view.fieldOfView = fieldOfView;
    }

    void Update()
    {
        FitView();
        if (!loaded)
        {
            return;
        }

        if (state == State.Playing)
        {
            Simulate(Mathf.Min(Time.deltaTime, maxStep)); // a long Editor frame must not let units pass through each other
        }
        else if (state == State.Won && Time.time - resultTime > restartDelay && Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            SceneManager.LoadScene((SceneManager.GetActiveScene().buildIndex + 1) % SceneManager.sceneCountInBuildSettings);
        }

        swarm.Draw(WorldBounds);
        mites.Draw(WorldBounds);
        halos.Clear();
        field.AddHalos(halos);
        hive.AddHalos(halos);
        swarm.AddHalos(halos);
        halos.Draw(WorldBounds);
    }

    void Simulate(float deltaTime)
    {
        simTime += deltaTime;
        int launches = mothership.Tick(deltaTime);
        if (mothership.ChampionReleased)
        {
            champion.Launch(mothership.X, mothership.NoseZ);
        }
        swarm.Launch(mothership.X, mothership.NoseZ, launches);
        swarm.Tick(deltaTime, simTime, gates, rings, hive, champion);
        mites.Tick(deltaTime, hive.FrontZ - 0.5f, mothership.X);
        champion.Tick(deltaTime, mites, hive);
        ResolveContacts();

        if (autopilot != null && autopilot.enabled)
        {
            autopilot.Record(simTime, swarm, mites.Count, gates);
        }

        if (hive.HitPoints <= 0)
        {
            Finish(State.Won, "HIVE CRACKED");
            return;
        }

        int hit = mites.FirstReached(mothership.NoseZ);
        if (hit >= 0)
        {
            mothership.PlayHit(mites.Mites[hit].x, mites.Mites[hit].colour);
            Finish(State.Lost, "MOTHERSHIP HIT");
        }
    }

    void ResolveContacts()
    {
        Drone[] drones = swarm.Drones;
        Mite[] enemies = mites.Mites;
        float reachSquared = contactRadius * contactRadius;

        for (int m = mites.Count - 1; m >= 0; m--)
        {
            for (int d = 0; d < swarm.Count; d++)
            {
                float dz = drones[d].z - enemies[m].z;
                float dx = drones[d].x - enemies[m].x;
                if (dz > contactRadius || dz < -contactRadius || dx * dx + dz * dz > reachSquared)
                {
                    continue;
                }

                if (drones[d].colour != enemies[m].colour)
                {
                    swarm.RemoveAt(d);
                }
                mites.RemoveAt(m);
                break;
            }
        }
    }

    void Finish(State result, string message)
    {
        state = result;
        resultTime = Time.time;
        resultText.text = message;
        resultText.gameObject.SetActive(true);

        if (result == State.Won)
        {
            int level = SceneManager.GetActiveScene().buildIndex + 1;
            if (level == SceneManager.sceneCountInBuildSettings)
            {
                resultText.text = FinalWinMessage; // tells the player there is no more content
            }
            progress.highestLevelWon = Mathf.Max(progress.highestLevelWon, level);
            SaveProgress("win");
        }
        else
        {
            reviveButton.gameObject.SetActive(!reviveUsed && PlatformServices.Instance.Ads.RewardedAvailable);
            failedButtons.SetActive(true);
            Debug.Log($"[Level] Level failed: hive {hive.HitPoints}, drones {swarm.Count}, mites {mites.Count}, Revive offered {!reviveUsed}");
        }

        if (autopilot != null && autopilot.enabled)
        {
            autopilot.Report(message, gates, rings, champion.Launches);
        }
    }

    void Revive()
    {
        if (state != State.Lost || reviveUsed || reviveWaiting)
        {
            return;
        }

        reviveWaiting = true;
        reviveButton.interactable = false;
        PlatformServices.Instance.Ads.ShowRewarded(ReviveRewardId, OnReviveResult);
    }

    void OnReviveResult(bool rewarded)
    {
        if (this == null)
        {
            return; // the level was left while the ad was showing
        }

        reviveWaiting = false;
        reviveButton.interactable = true;
        Debug.Log($"[Level] Revive reward {(rewarded ? "granted" : "not granted")}");
        if (!rewarded || state != State.Lost)
        {
            return;
        }

        reviveUsed = true;
        int cleared = mites.Count;
        mites.Clear();
        failedButtons.SetActive(false);
        resultText.gameObject.SetActive(false);
        state = State.Playing;
        Debug.Log($"[Level] Revived: cleared {cleared} mites, play resumes with hive {hive.HitPoints} and {swarm.Count} drones");
    }

    void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
