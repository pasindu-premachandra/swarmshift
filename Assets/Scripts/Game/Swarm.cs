using UnityEngine;

// Each drone flies straight up the lane from where it was placed: its bay slot beside the ship at launch, or its place in the
// ranks a multiply gate lays out. Only the hive funnel and the champion's bow wave move it sideways after that.
public class Swarm : MonoBehaviour
{
    public const int Capacity = 1000;
    const float UnitBox = 0.5f; // DESIGN 16.4
    const float HaloAlpha = 0.55f;
    const float HaloLift = -0.3f; // just behind the drone

    // Formation tuning (DESIGN 16.4), lane units and seconds
    public const int BayAbreast = 3;
    public const float BaySpacing = 0.5f;
    const float BayFillDepth = 1f; // an open rank takes new launches until it is this far out of the bay
    const int TideWideRank = 5;
    const int TideNarrowRank = 4;
    const float TideSpacing = 0.3f;
    const float TideRankInterval = 0.2f;
    const float FunnelSpeed = 4f; // a drone toward the hive's band in the last funnelDistance
    const float LeadRankDepth = TideRankInterval * 0.5f; // the front rank only
    const float BowSide = 0.95f;
    const float BowBehind = 1f;
    const float BowAhead = 0.75f;

    const float SameRank = 0.01f;
    static readonly float[] BaySlots = { 0f, -1f, 1f }; // in launch order: centre, left, right

    [SerializeField] Material material;
    [SerializeField] float speed = 5.5f;
    [SerializeField] float flightHeight = 1.2f;
    [SerializeField] float funnelDistance = 8f;

    readonly Drone[] drones = new Drone[Capacity];
    readonly InstanceBatch[] batches = new InstanceBatch[4];
    readonly float[] leadZ = new float[4];

    readonly Drone[] crossers = new Drone[Capacity];
    readonly int[] crossGate = new int[Capacity];
    readonly int[] crossCopies = new int[Capacity];
    readonly int[] crossOrder = new int[Capacity];
    readonly float[] layoutSlot = new float[Capacity];
    readonly int[] layoutRank = new int[Capacity];
    int crossCount;

    float bayZ;
    int bayFill = BayAbreast;

    public int Count { get; private set; }
    public Drone[] Drones => drones;
    public float Speed => speed;

    // Arrow Darts, one spec mesh per colour; a drone swaps mesh when a ring charges it
    void Awake()
    {
        for (int i = 0; i < batches.Length; i++)
        {
            batches[i] = new InstanceBatch(SpecMeshData.ArrowDarts[i].ToMesh(), material, Capacity);
        }
    }

    // Launches leave the bay in ranks of 3 abreast.
    public void Launch(float x, float z, int launches)
    {
        for (int i = 0; i < launches; i++)
        {
            if (bayFill >= BayAbreast || bayZ < z || bayZ - z > BayFillDepth)
            {
                bayFill = 0;
                bayZ = z;
            }
            float slot = BaySlots[bayFill] * BaySpacing;
            Add(new Drone { x = x + slot, z = bayZ, slot = slot, colour = UnitColour.White });
            bayFill++;
        }
    }

    public void Tick(float deltaTime, float time, Gate[] gates, WarpRing[] rings, Hive hive, Champion champion)
    {
        bayZ += speed * deltaTime;
        crossCount = 0;

        for (int i = 0; i < Count; i++)
        {
            Drone drone = drones[i];
            float previousZ = drone.z;
            drone.z += speed * deltaTime;
            bool removed = false;

            for (int g = 0; g < gates.Length; g++)
            {
                Gate gate = gates[g];
                if (previousZ < gate.Z && drone.z >= gate.Z && gate.Contains(drone.x))
                {
                    int leaving = gate.Cross(drone.colour, time);
                    if (leaving > 1 && !removed)
                    {
                        crossers[crossCount] = drone; // replaced by its copies once the whole rank has crossed
                        crossGate[crossCount] = g;
                        crossCopies[crossCount] = leaving;
                        crossCount++;
                    }
                    if (leaving != 1)
                    {
                        removed = true;
                    }
                }
            }

            if (!removed)
            {
                foreach (WarpRing ring in rings)
                {
                    if (previousZ < ring.Z && drone.z >= ring.Z && ring.Contains(drone.x))
                    {
                        drone.colour = ring.Charge();
                    }
                }
            }

            if (!removed && drone.z >= hive.FrontZ)
            {
                hive.Hit();
                removed = true;
            }

            if (removed)
            {
                RemoveAt(i);
                i--;
                continue;
            }

            drones[i] = drone;
        }

        LayOutCopies();
        Form(deltaTime, hive, champion);
    }

    public void RemoveAt(int index)
    {
        Count--;
        drones[index] = drones[Count];
    }

    public void Draw(Bounds worldBounds)
    {
        foreach (InstanceBatch batch in batches)
        {
            batch.Clear();
        }
        for (int i = 0; i < Count; i++)
        {
            batches[(int)drones[i].colour].Add(Matrix4x4.Translate(Position(i)), Vector4.one);
        }
        foreach (InstanceBatch batch in batches)
        {
            batch.Draw(worldBounds);
        }
    }

    // Only the lead rank of each colour carries a halo.
    public void AddHalos(Halos halos)
    {
        for (int c = 0; c < leadZ.Length; c++)
        {
            leadZ[c] = float.MinValue;
        }
        for (int i = 0; i < Count; i++)
        {
            leadZ[(int)drones[i].colour] = Mathf.Max(leadZ[(int)drones[i].colour], drones[i].z);
        }
        for (int i = 0; i < Count; i++)
        {
            if (drones[i].z >= leadZ[(int)drones[i].colour] - LeadRankDepth)
            {
                halos.Add(Position(i), Vector2.one * (UnitBox * Halos.Scale), Palette.UnitLight(drones[i].colour), HaloAlpha, HaloLift);
            }
        }
    }

    // Each rank that crossed a multiply gate this step becomes ranks of 5 and 4 abreast, each in the gaps of the last;
    // every drone's copies take the places nearest its own, around the rank's x.
    void LayOutCopies()
    {
        for (int i = 0; i < crossCount; i++)
        {
            int key = i;
            int j = i - 1;
            while (j >= 0 && CrossesAfter(crossOrder[j], key))
            {
                crossOrder[j + 1] = crossOrder[j];
                j--;
            }
            crossOrder[j + 1] = key;
        }

        int start = 0;
        while (start < crossCount)
        {
            int end = start + 1;
            while (end < crossCount && InSameRank(crossOrder[start], crossOrder[end]))
            {
                end++;
            }
            LayOutRank(start, end);
            start = end;
        }
    }

    bool CrossesAfter(int a, int b)
    {
        if (crossGate[a] != crossGate[b])
        {
            return crossGate[a] > crossGate[b];
        }
        if (crossers[a].colour != crossers[b].colour)
        {
            return crossers[a].colour > crossers[b].colour;
        }
        if (Mathf.Abs(crossers[a].z - crossers[b].z) > SameRank)
        {
            return crossers[a].z < crossers[b].z;
        }
        return crossers[a].slot > crossers[b].slot;
    }

    bool InSameRank(int a, int b)
    {
        return crossGate[a] == crossGate[b] && crossers[a].colour == crossers[b].colour && Mathf.Abs(crossers[a].z - crossers[b].z) <= SameRank;
    }

    void LayOutRank(int start, int end)
    {
        int copies = crossCopies[crossOrder[start]];
        int total = (end - start) * copies;

        float rankX = 0f;
        for (int n = start; n < end; n++)
        {
            rankX += crossers[crossOrder[n]].x;
        }
        rankX /= end - start;

        int ranks = 0;
        int placed = 0;
        while (placed < total)
        {
            int size = Mathf.Min(ranks % 2 == 0 ? TideWideRank : TideNarrowRank, total - placed);
            for (int k = 0; k < size; k++)
            {
                InsertPlace(placed + k, (k - (size - 1) * 0.5f) * TideSpacing, ranks);
            }
            placed += size;
            ranks++;
        }

        for (int n = 0; n < total; n++)
        {
            Drone copy = crossers[crossOrder[start + n / copies]];
            copy.z += (ranks - 1 - layoutRank[n]) * TideRankInterval; // the last rank stays on the crossing, so no copy crosses again
            copy.slot = layoutSlot[n];
            copy.x = rankX + layoutSlot[n];
            Add(copy);
        }
    }

    // Keeps the places sorted left to right.
    void InsertPlace(int count, float slot, int rank)
    {
        int j = count - 1;
        while (j >= 0 && layoutSlot[j] > slot)
        {
            layoutSlot[j + 1] = layoutSlot[j];
            layoutRank[j + 1] = layoutRank[j];
            j--;
        }
        layoutSlot[j + 1] = slot;
        layoutRank[j + 1] = rank;
    }

    void Form(float deltaTime, Hive hive, Champion champion)
    {
        float funnelZ = hive.FrontZ - funnelDistance;
        float funnelEdge = hive.HalfWidth * 0.8f;
        bool bowWave = champion.InFlight;
        Vector3 bow = champion.transform.position;

        for (int i = 0; i < Count; i++)
        {
            Drone drone = drones[i];
            if (drone.z > funnelZ)
            {
                float band = Mathf.Clamp(drone.x, hive.X - funnelEdge, hive.X + funnelEdge);
                drone.x = Mathf.MoveTowards(drone.x, band, FunnelSpeed * deltaTime);
            }

            float ahead = drone.z - bow.z;
            if (bowWave && ahead > -BowBehind && ahead < BowAhead && Mathf.Abs(drone.x - bow.x) < BowSide)
            {
                drone.x = bow.x + (drone.x < bow.x ? -BowSide : BowSide);
            }

            drones[i] = drone;
        }
    }

    Vector3 Position(int index)
    {
        float x = drones[index].x + Mathf.Sin(Time.time * 5f + drones[index].phase) * 0.05f;
        return new Vector3(x, flightHeight, drones[index].z);
    }

    void Add(Drone drone)
    {
        if (Count >= Capacity)
        {
            return;
        }

        float limit = Lane.HalfWidth - UnitBox * 0.5f;
        drone.x = Mathf.Clamp(drone.x, -limit, limit);
        drone.phase = Random.Range(0f, 6.28f);
        drones[Count] = drone;
        Count++;
    }
}
