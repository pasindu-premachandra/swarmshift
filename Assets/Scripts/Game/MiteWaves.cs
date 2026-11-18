using UnityEngine;

public class MiteWaves : MonoBehaviour
{
    public const int Capacity = 120;

    static readonly UnitColour[] WaveColours = { UnitColour.Cyan, UnitColour.Yellow, UnitColour.Magenta };

    [SerializeField] Material material;
    [SerializeField] Material rimMaterial;
    [SerializeField] float firstWaveDelay = 2f;
    [SerializeField] float waveInterval = 2f;
    [SerializeField] int mitesPerWave = 5;
    [SerializeField] float speed = 2.5f;
    [SerializeField] float homingSpeed = 1.2f;
    [SerializeField] float flightHeight = 0.1f; // the body base rides 0.1 above the lane on its legs

    readonly Mite[] mites = new Mite[Capacity];
    readonly InstanceBatch[] batches = new InstanceBatch[4];
    float waveTimer;
    int waveIndex;

    public int Count { get; private set; }
    public Mite[] Mites => mites;

    // One spec mesh per wave colour: submesh 0 (body, two-sided legs) uses material, submesh 1 (the ink rim, an inverted hull) uses rimMaterial
    void Awake()
    {
        foreach (UnitColour colour in WaveColours)
        {
            batches[(int)colour] = new InstanceBatch(SpecMeshData.Mites[(int)colour].ToMesh(), new[] { material, rimMaterial }, Capacity);
        }
        waveTimer = firstWaveDelay;
    }

    public void Tick(float deltaTime, float spawnZ, float targetX)
    {
        waveTimer -= deltaTime;
        if (waveTimer <= 0f)
        {
            SpawnWave(spawnZ);
            waveTimer += waveInterval;
        }

        float limit = Lane.HalfWidth - 0.35f;
        for (int i = 0; i < Count; i++)
        {
            mites[i].z -= speed * deltaTime;
            float goal = Mathf.Clamp(targetX + mites[i].offsetX, -limit, limit);
            mites[i].x = Mathf.MoveTowards(mites[i].x, goal, homingSpeed * deltaTime);
        }
    }

    // Index of the first mite at or past the line, or -1.
    public int FirstReached(float lineZ)
    {
        for (int i = 0; i < Count; i++)
        {
            if (mites[i].z <= lineZ)
            {
                return i;
            }
        }
        return -1;
    }

    public void RemoveAt(int index)
    {
        Count--;
        mites[index] = mites[Count];
    }

    public void Clear()
    {
        Count = 0;
    }

    public void Draw(Bounds worldBounds)
    {
        foreach (UnitColour colour in WaveColours)
        {
            batches[(int)colour].Clear();
        }
        for (int i = 0; i < Count; i++)
        {
            batches[(int)mites[i].colour].Add(Matrix4x4.Translate(new Vector3(mites[i].x, flightHeight, mites[i].z)), Vector4.one);
        }
        foreach (UnitColour colour in WaveColours)
        {
            batches[(int)colour].Draw(worldBounds);
        }
    }

    void SpawnWave(float spawnZ)
    {
        UnitColour colour = WaveColours[waveIndex % WaveColours.Length];
        waveIndex++;
        float centreX = Random.Range(-3.5f, 3.5f);
        for (int i = 0; i < mitesPerWave && Count < Capacity; i++)
        {
            float offsetX = (i - (mitesPerWave - 1) * 0.5f) * 0.5f;
            mites[Count] = new Mite
            {
                x = centreX + offsetX,
                z = spawnZ + (i % 2) * 0.6f,
                offsetX = offsetX,
                colour = colour
            };
            Count++;
        }
    }
}
