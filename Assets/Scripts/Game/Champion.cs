using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Champion : MonoBehaviour
{
    public const float HiveDamageFraction = 0.25f; // of the hive's starting number, rounded up

    [SerializeField] float speed = 9f;
    [SerializeField] float flightHeight = 1.2f;
    [SerializeField] float growTime = 0.2f;
    [SerializeField] float launchScale = 0.6f;
    [SerializeField] float popRadius = 0.9f;
    [SerializeField] float funnelDistance = 8f;
    [SerializeField] float funnelSpeed = 2f;

    MeshRenderer body;
    float x;
    float z;
    float age;
    int popped;

    public bool InFlight { get; private set; }
    public int Launches { get; private set; }

    void Awake()
    {
        GetComponent<MeshFilter>().sharedMesh = BuildMesh();
        body = GetComponent<MeshRenderer>();
        body.enabled = false;
    }

    public void Launch(float fromX, float fromZ)
    {
        x = fromX;
        z = fromZ;
        age = 0f;
        popped = 0;
        InFlight = true;
        Launches++;
        body.enabled = true;
        Place();
    }

    public void Tick(float deltaTime, MiteWaves mites, Hive hive)
    {
        if (!InFlight)
        {
            return;
        }

        age += deltaTime;
        z += speed * deltaTime;
        if (z > hive.FrontZ - funnelDistance)
        {
            float edge = hive.HalfWidth * 0.8f;
            x = Mathf.MoveTowards(x, Mathf.Clamp(x, hive.X - edge, hive.X + edge), funnelSpeed * deltaTime);
        }

        Mite[] enemies = mites.Mites;
        float reachSquared = popRadius * popRadius;
        for (int m = mites.Count - 1; m >= 0; m--)
        {
            float dx = enemies[m].x - x;
            float dz = enemies[m].z - z;
            if (dx * dx + dz * dz <= reachSquared)
            {
                mites.RemoveAt(m);
                popped++;
            }
        }

        if (z >= hive.FrontZ)
        {
            int damage = Mathf.CeilToInt(hive.StartHitPoints * HiveDamageFraction);
            int before = hive.HitPoints;
            hive.Damage(damage);
            Debug.Log($"[Champion] hit the hive: {before} -> {hive.HitPoints} " +
                $"({HiveDamageFraction * 100f:F0}% of {hive.StartHitPoints} rounded up = {damage}), popped {popped} mites on the way");
            InFlight = false;
            body.enabled = false;
            return;
        }

        Place();
    }

    void Place()
    {
        transform.position = new Vector3(x, flightHeight, z);
        transform.localScale = Vector3.one * Mathf.Lerp(launchScale, 1f, age / growTime);
    }

    // Placeholder Bulwark: wide white hull, navy deck stripe, shield arc in front; about 1.15 lane units wide.
    Mesh BuildMesh()
    {
        var builder = new MeshBuilder();
        builder.Box(new Vector3(0f, 0f, -0.1f), new Vector3(0.95f, 0.3f, 0.8f), Palette.Hull, Palette.HullShade);
        builder.Box(new Vector3(0f, 0.16f, -0.15f), new Vector3(0.28f, 0.02f, 0.6f), Palette.Raised, Palette.Raised);
        builder.Box(new Vector3(-0.53f, -0.02f, 0.25f), new Vector3(0.09f, 0.28f, 0.28f), Palette.Hull, Palette.HullShade);
        builder.Box(new Vector3(0.53f, -0.02f, 0.25f), new Vector3(0.09f, 0.28f, 0.28f), Palette.Hull, Palette.HullShade);
        builder.Box(new Vector3(0f, -0.02f, 0.42f), new Vector3(1.15f, 0.26f, 0.08f), Palette.Hull, Palette.HullShade);
        return builder.ToMesh("Champion");
    }
}
