using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mothership : MonoBehaviour
{
    static readonly int ColorId = Shader.PropertyToID("_Color");

    [SerializeField] float launchRate = 20f;
    [SerializeField] float slideUnitsPerScreenWidth = 10f;
    [SerializeField] float halfWidth = 1.6f;
    [SerializeField] float noseOffset = 1.4f;
    [SerializeField] ChargeMeter meter;
    [SerializeField] Transform spark;
    [SerializeField] float flashTime = 0.06f;
    [SerializeField] float shakeTime = 0.2f;
    [SerializeField] float shakeDistance = 0.08f;
    [SerializeField] Autopilot autopilot;

    float launchCredit;
    Vector2 lastPointer;
    bool wasPressed;
    bool wasFiring;
    MeshRenderer hull;
    MaterialPropertyBlock flash;
    MeshRenderer sparkRenderer;
    MaterialPropertyBlock sparkColour;
    Vector3 restPosition;
    float hitTime = float.NegativeInfinity;
    bool shaking;

    public float X => restPosition.x;
    public float NoseZ => restPosition.z + noseOffset;
    public ChargeMeter Meter => meter;
    public bool ChampionReleased { get; private set; }
    float Limit => Lane.HalfWidth - halfWidth;

    void Awake()
    {
        GetComponent<MeshFilter>().sharedMesh = BuildMesh();
        hull = GetComponent<MeshRenderer>();
        flash = new MaterialPropertyBlock();
        restPosition = transform.position;

        spark.GetComponent<MeshFilter>().sharedMesh = BuildSpark();
        sparkRenderer = spark.GetComponent<MeshRenderer>();
        sparkColour = new MaterialPropertyBlock();
        sparkRenderer.enabled = false;
    }

    // Returns the drones to launch this step; ChampionReleased is true for the step the finger lifts with the meter full.
    public int Tick(float deltaTime)
    {
        StopShake();
        float x = X;
        bool firing;

        if (autopilot != null && autopilot.enabled)
        {
            firing = autopilot.Firing(meter.Full);
            x = autopilot.SweepX(Limit);
        }
        else
        {
            Pointer pointer = Pointer.current;
            firing = pointer != null && pointer.press.isPressed;
            if (firing)
            {
                Vector2 position = pointer.position.ReadValue();
                if (wasPressed)
                {
                    x += (position.x - lastPointer.x) / Screen.width * slideUnitsPerScreenWidth;
                }
                lastPointer = position;
            }
            wasPressed = firing;
        }

        x = Mathf.Clamp(x, -Limit, Limit);
        restPosition = new Vector3(x, restPosition.y, restPosition.z);
        transform.position = restPosition;

        ChampionReleased = wasFiring && !firing && meter.Full;
        wasFiring = firing;
        if (ChampionReleased)
        {
            meter.Empty();
        }

        if (!firing)
        {
            launchCredit = 1f;
            return 0;
        }

        launchCredit += launchRate * deltaTime;
        int launches = (int)launchCredit;
        launchCredit -= launches;
        meter.Add(launches);
        return launches;
    }

    // DESIGN 17.3 mothership hit: spark, hull flash in minus-light, shake.
    public void PlayHit(float x, UnitColour colour)
    {
        hitTime = Time.time;
        shaking = true;
        spark.position = new Vector3(x, 1f, NoseZ);
        sparkColour.SetColor(ColorId, MeshBuilder.ToShaderColour(Palette.Unit(colour)));
        sparkRenderer.SetPropertyBlock(sparkColour);
        sparkRenderer.enabled = true;
        SetHullTint(Palette.MinusLight);
    }

    void Update()
    {
        if (!shaking)
        {
            return;
        }

        float age = Time.time - hitTime;
        if (age >= flashTime)
        {
            SetHullTint(Color.white);
        }
        if (age >= shakeTime)
        {
            StopShake();
            return;
        }

        float strength = shakeDistance * (1f - age / shakeTime);
        transform.position = restPosition + new Vector3(Random.Range(-strength, strength), 0f, Random.Range(-strength, strength));
        spark.localScale = Vector3.one * Mathf.Lerp(0.6f, 1.2f, age / shakeTime);
    }

    void StopShake()
    {
        if (!shaking)
        {
            return;
        }

        shaking = false;
        transform.position = restPosition;
        SetHullTint(Color.white);
        sparkRenderer.enabled = false;
    }

    void SetHullTint(Color tint)
    {
        flash.SetColor(ColorId, MeshBuilder.ToShaderColour(tint));
        hull.SetPropertyBlock(flash);
    }

    Mesh BuildMesh()
    {
        var builder = new MeshBuilder();
        builder.Box(new Vector3(0f, 0.55f, 0f), new Vector3(1.1f, 0.9f, 2f), Palette.Hull, Palette.HullShade);
        builder.Box(new Vector3(0f, 0.45f, 1.2f), new Vector3(0.8f, 0.7f, 0.4f), Palette.Hull, Palette.HullShade);
        builder.Box(new Vector3(0f, 1.01f, 0.3f), new Vector3(0.6f, 0.02f, 0.7f), Palette.Raised, Palette.Raised);

        Color[] tail = { Palette.Cyan, Palette.Yellow, Palette.Magenta };
        for (int side = -1; side <= 1; side += 2)
        {
            builder.Box(new Vector3(side * 1f, 0.3f, -0.2f), new Vector3(1.2f, 0.3f, 1.2f), Palette.Hull, Palette.HullShade);
            builder.Box(new Vector3(side * 1.45f, 0.45f, -0.25f), new Vector3(0.3f, 0.5f, 1f), Palette.Hull, Palette.HullShade);
            for (int i = 0; i < tail.Length; i++)
            {
                builder.Box(new Vector3(side * (0.7f + i * 0.17f), 0.46f, -0.2f), new Vector3(0.1f, 0.02f, 0.5f), tail[i], tail[i]);
            }
        }

        return builder.ToMesh("Mothership");
    }

    // A white eight-point star, 1 unit across, tinted per hit.
    static Mesh BuildSpark()
    {
        var builder = new MeshBuilder();
        builder.FlatPolygon(Vector3.zero, 0.5f, 4, 0f, Color.white);
        builder.FlatPolygon(Vector3.zero, 0.5f, 4, 45f, Color.white);
        return builder.ToMesh("Spark");
    }
}
