using System.Collections.Generic;
using System.Text;
using Unity.Profiling;
using UnityEngine;

public class Autopilot : MonoBehaviour
{
    const float NearGate = 3f; // the C test: drones inside the multiply gate's half, within this of its plane

    [SerializeField] float sweepPeriod = 8f;
    [SerializeField] float busiestWindow = 5f;
    [SerializeField] float pauseThreshold = 0.25f;
    [SerializeField] float holdTime = 2f;
    [SerializeField] float farDwell = 0.2f;
    [SerializeField] float midDwell = 0.2f;
    [SerializeField] float moveSpeed = 10f; // between rings; no faster than the swarm follows the ship
    [SerializeField] float arriveSlack = 0.5f;
    [SerializeField] bool planFire = true;

    // Proof scripts set this to pause the Editor on each new best frame of the C test (from these counts up).
    public bool breakOnBest;
    public int breakTotalFrom = 30;
    public int breakEvenFrom = 3;
    public string BestTotal { get; private set; } = "none";
    public string BestEven { get; private set; } = "none";

    readonly List<float> times = new List<float>();
    readonly List<float> frameTimes = new List<float>();
    readonly List<int> liveUnits = new List<int>();
    readonly List<long> drawCalls = new List<long>();
    readonly int[] peakByColour = new int[4];
    readonly int[] liveByColour = new int[4];
    readonly int[] nearGate = new int[4];
    readonly List<int>[] windowCrossings = new List<int>[8];
    int[] lastCrossings;
    int bestTotal;
    int bestEven;
    bool ringPlan;
    WarpRing[] planRings;
    float holdX;
    float farX;
    float midX;
    float toRings;
    float toGate;
    List<Vector2>[] absorbEvents;
    int[] lastAbsorbed;
    float startTime;
    float simStart = -1f;
    float simNow;
    int peakDrones;
    int peakMites;
    int pausedFrames;
    ProfilerRecorder drawCallRecorder;

    void Awake()
    {
        if (!Application.isEditor)
        {
            enabled = false;
        }
    }

    void OnEnable()
    {
        startTime = Time.time;
        times.Clear();
        frameTimes.Clear();
        liveUnits.Clear();
        drawCalls.Clear();
        drawCallRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
        System.Array.Clear(peakByColour, 0, peakByColour.Length);
        absorbEvents = null;
        simStart = -1f;
        peakDrones = 0;
        peakMites = 0;
        pausedFrames = 0;
        lastCrossings = null;
        bestTotal = 0;
        bestEven = 0;
        BestTotal = "none";
        BestEven = "none";
        PlanRings();
    }

    // A ring row before a multiply gate: hold over the nearest ring, as close to the gate's middle as a whole rank allows;
    // from time to time visit the farthest ring, then the middle one, and come back. Fire only drones that will cross
    // inside a ring and reach the gate while the swarm is back over it.
    void PlanRings()
    {
        Gate multiply = null;
        foreach (Gate gate in FindObjectsByType<Gate>())
        {
            if (gate.Value > 0)
            {
                multiply = gate;
            }
        }
        planRings = FindObjectsByType<WarpRing>();
        ringPlan = multiply != null && planRings.Length == 3 && planRings[0].Z < multiply.Z;
        if (!ringPlan)
        {
            return;
        }

        float aimX = multiply.transform.position.x;
        System.Array.Sort(planRings, (a, b) => Mathf.Abs(a.transform.position.x - aimX).CompareTo(Mathf.Abs(b.transform.position.x - aimX)));
        float nearX = planRings[0].transform.position.x;
        midX = planRings[1].transform.position.x;
        farX = planRings[2].transform.position.x;
        holdX = nearX;
        for (float x = aimX; Mathf.Abs(x - nearX) > 0.01f; x = Mathf.MoveTowards(x, nearX, 0.01f))
        {
            if (FitsRing(x))
            {
                holdX = x;
                break;
            }
        }

        float speed = FindAnyObjectByType<Swarm>().Speed;
        toRings = (planRings[0].Z - FindAnyObjectByType<Mothership>().NoseZ) / speed;
        toGate = (multiply.Z - planRings[0].Z) / speed;
    }

    // A rank of the bay's width centred on x passes wholly through one ring.
    bool FitsRing(float x)
    {
        float half = (Swarm.BayAbreast - 1) * 0.5f * Swarm.BaySpacing;
        foreach (WarpRing ring in planRings)
        {
            if (ring.Contains(x - half) && ring.Contains(x + half))
            {
                return true;
            }
        }
        return false;
    }

    float PlanX(float time)
    {
        float toFar = Mathf.Abs(farX - holdX) / moveSpeed;
        float toMid = Mathf.Abs(midX - farX) / moveSpeed;
        float back = Mathf.Abs(holdX - midX) / moveSpeed;
        float t = time % (holdTime + toFar + farDwell + toMid + midDwell + back);
        if ((t -= holdTime) < 0f)
        {
            return holdX;
        }
        if ((t -= toFar) < 0f)
        {
            return Mathf.Lerp(farX, holdX, -t / toFar);
        }
        if ((t -= farDwell) < 0f)
        {
            return farX;
        }
        if ((t -= toMid) < 0f)
        {
            return Mathf.Lerp(midX, farX, -t / toMid);
        }
        if ((t -= midDwell) < 0f)
        {
            return midX;
        }
        return Mathf.Lerp(midX, holdX, t / back);
    }

    void OnDisable()
    {
        drawCallRecorder.Dispose();
    }

    public float SweepX(float limit)
    {
        float time = Time.time - startTime;
        if (ringPlan)
        {
            return PlanX(time);
        }
        return -Mathf.Cos(time / sweepPeriod * 2f * Mathf.PI) * limit;
    }

    // Holds fire, lifting for one step whenever the charge meter is full, which launches the champion.
    public bool Firing(bool meterFull)
    {
        if (meterFull)
        {
            return false;
        }
        if (!ringPlan || !planFire)
        {
            return true;
        }

        float time = Time.time - startTime;
        return FitsRing(PlanX(time + toRings)) && Mathf.Abs(PlanX(time + toRings + toGate) - holdX) <= arriveSlack;
    }

    public void Record(float simTime, Swarm swarm, int mites, Gate[] gates)
    {
        if (absorbEvents == null)
        {
            simStart = simTime;
            absorbEvents = new List<Vector2>[gates.Length];
            lastAbsorbed = new int[gates.Length];
            for (int g = 0; g < gates.Length; g++)
            {
                absorbEvents[g] = new List<Vector2>();
                lastAbsorbed[g] = gates[g].Absorbed;
            }
        }

        simNow = simTime;
        for (int g = 0; g < gates.Length; g++)
        {
            int absorbed = gates[g].Absorbed - lastAbsorbed[g];
            if (absorbed > 0)
            {
                absorbEvents[g].Add(new Vector2(simTime, absorbed)); // the same time value the gate saw
                lastAbsorbed[g] = gates[g].Absorbed;
            }
        }
        CountCrossings(simTime, gates);
        CountNearMultiply(swarm, gates);

        System.Array.Clear(liveByColour, 0, liveByColour.Length);
        for (int i = 0; i < swarm.Count; i++)
        {
            liveByColour[(int)swarm.Drones[i].colour]++;
        }
        for (int c = 0; c < peakByColour.Length; c++)
        {
            peakByColour[c] = Mathf.Max(peakByColour[c], liveByColour[c]);
        }

        int drones = swarm.Count;
        peakDrones = Mathf.Max(peakDrones, drones);
        peakMites = Mathf.Max(peakMites, mites);
        if (Time.unscaledDeltaTime > pauseThreshold)
        {
            pausedFrames++; // the Editor was paused for a capture, so this delta is not a frame time
            return;
        }

        times.Add(Time.time - startTime);
        frameTimes.Add(Time.unscaledDeltaTime);
        liveUnits.Add(drones + mites);
        drawCalls.Add(drawCallRecorder.LastValue); // the previous frame's count
    }

    public void Report(string result, Gate[] gates, WarpRing[] rings, int championLaunches)
    {
        if (times.Count == 0)
        {
            return;
        }

        int bestStart = 0;
        int bestEnd = 0;
        float bestLoad = -1f;
        int start = 0;
        long unitSum = 0;
        for (int end = 0; end < times.Count; end++)
        {
            unitSum += liveUnits[end];
            while (times[end] - times[start] > busiestWindow)
            {
                unitSum -= liveUnits[start];
                start++;
            }

            float load = unitSum / (float)(end - start + 1);
            if (load > bestLoad)
            {
                bestLoad = load;
                bestStart = start;
                bestEnd = end;
            }
        }

        float frameSum = 0f;
        float worstFrame = 0f;
        long drawSum = 0;
        long drawMost = 0;
        for (int i = bestStart; i <= bestEnd; i++)
        {
            frameSum += frameTimes[i];
            worstFrame = Mathf.Max(worstFrame, frameTimes[i]);
            drawSum += drawCalls[i];
            drawMost = System.Math.Max(drawMost, drawCalls[i]);
        }
        int frames = bestEnd - bestStart + 1;
        List<long> sorted = drawCalls.GetRange(bestStart, frames);
        sorted.Sort();
        long drawMedian = sorted[frames / 2];
        int drawSpikes = sorted.FindAll(n => n > drawMedian * 3 / 2).Count;

        Debug.Log($"[Autopilot] {result} after {Time.time - startTime:F1} s | peak drones {peakDrones} | peak mites {peakMites} | " +
            $"busiest 5 s window {times[bestStart]:F1}-{times[bestEnd]:F1} s: mean {bestLoad:F0} live units, " +
            $"mean frame {frameSum / frames * 1000f:F2} ms over {frames} frames, worst {worstFrame * 1000f:F2} ms | " +
            $"draw calls mean {drawSum / (float)frames:F1}, median {drawMedian}, most {drawMost}, {drawSpikes} frames over 1.5 x median | {pausedFrames} paused frames left out");

        var rows = new StringBuilder("[Autopilot] rings charged:");
        foreach (WarpRing ring in rings)
        {
            rows.Append($" {ring.Colour} {ring.Charged},");
        }
        rows.Append($" | peak live drones by colour: white {peakByColour[0]}, cyan {peakByColour[1]}, yellow {peakByColour[2]}, magenta {peakByColour[3]}");
        rows.Append($" | champion launches {championLaunches}");

        for (int g = 0; g < gates.Length; g++)
        {
            Gate gate = gates[g];
            int crossed = 0;
            for (int c = 0; c < 4; c++)
            {
                crossed += gate.Crossings((UnitColour)c);
            }

            string sign = gate.Value > 0 ? "x" + gate.Value : "-" + -gate.Value;
            rows.Append($"\n[Autopilot] gate {sign}: crossed {crossed} (white {gate.Crossings(UnitColour.White)}, cyan {gate.Crossings(UnitColour.Cyan)}, " +
                $"yellow {gate.Crossings(UnitColour.Yellow)}, magenta {gate.Crossings(UnitColour.Magenta)})");
            if (gate.Value > 0)
            {
                rows.Append($", left the gate {crossed * gate.Value}");
            }
            else
            {
                rows.Append($", absorbed {gate.Absorbed}, passed {crossed - gate.Absorbed}, {WindowCounts(absorbEvents[g])}");
            }
        }
        rows.Append($"\n[Autopilot] C test, drones inside the multiply gate's half within {NearGate:F0} units of its plane: most {BestTotal} | most even {BestEven}");
        rows.Append(HalfShares(gates));
        Debug.Log(rows.ToString());
    }

    // Crossings per gate in each window of Gate.MinusWindow, to see how often the whole swarm takes one half of a row.
    void CountCrossings(float simTime, Gate[] gates)
    {
        if (lastCrossings == null)
        {
            lastCrossings = new int[gates.Length];
            for (int g = 0; g < gates.Length; g++)
            {
                windowCrossings[g] = new List<int>();
                lastCrossings[g] = Crossed(gates[g]);
            }
        }

        int window = (int)((simTime - simStart) / Gate.MinusWindow);
        for (int g = 0; g < gates.Length; g++)
        {
            int crossed = Crossed(gates[g]);
            List<int> counts = windowCrossings[g];
            while (counts.Count <= window)
            {
                counts.Add(0);
            }
            counts[window] += crossed - lastCrossings[g];
            lastCrossings[g] = crossed;
        }
    }

    static int Crossed(Gate gate)
    {
        int crossed = 0;
        for (int c = 0; c < 4; c++)
        {
            crossed += gate.Crossings((UnitColour)c);
        }
        return crossed;
    }

    string HalfShares(Gate[] gates)
    {
        var text = new StringBuilder();
        for (int a = 0; a < gates.Length; a++)
        {
            for (int b = a + 1; b < gates.Length; b++)
            {
                if (!Mathf.Approximately(gates[a].Z, gates[b].Z) || windowCrossings[a] == null)
                {
                    continue;
                }

                int windows = 0;
                int onlyA = 0;
                int onlyB = 0;
                var shares = new List<string>();
                for (int w = 0; w < windowCrossings[a].Count; w++)
                {
                    int ca = windowCrossings[a][w];
                    int cb = windowCrossings[b][w];
                    if (ca + cb == 0)
                    {
                        continue;
                    }
                    windows++;
                    onlyA += cb == 0 ? 1 : 0;
                    onlyB += ca == 0 ? 1 : 0;
                    shares.Add($"{ca}:{cb}");
                }
                text.Append($"\n[Autopilot] gate row z {gates[a].Z:F1}, {Sign(gates[a])}:{Sign(gates[b])} crossings per {Gate.MinusWindow:F0} s window [{string.Join(", ", shares)}]; " +
                    $"{windows} windows with crossings, the whole swarm took one half in {onlyA + onlyB} ({Sign(gates[a])} only {onlyA}, {Sign(gates[b])} only {onlyB})");
            }
        }
        return text.ToString();
    }

    static string Sign(Gate gate)
    {
        return gate.Value > 0 ? "x" + gate.Value : "-" + -gate.Value;
    }

    // The C test: per colour, the drones inside the multiply gate's half and within NearGate of its plane.
    void CountNearMultiply(Swarm swarm, Gate[] gates)
    {
        Gate multiply = null;
        foreach (Gate gate in gates)
        {
            if (gate.Value > 0)
            {
                multiply = gate;
            }
        }
        if (multiply == null)
        {
            return;
        }

        System.Array.Clear(nearGate, 0, nearGate.Length);
        for (int i = 0; i < swarm.Count; i++)
        {
            Drone drone = swarm.Drones[i];
            if (Mathf.Abs(drone.z - multiply.Z) <= NearGate && multiply.Contains(drone.x))
            {
                nearGate[(int)drone.colour]++;
            }
        }

        int total = nearGate[0] + nearGate[1] + nearGate[2] + nearGate[3];
        int even = Mathf.Min(nearGate[1], Mathf.Min(nearGate[2], nearGate[3]));
        string counts = $"{total} at {Time.time - startTime:F2} s (cyan {nearGate[1]}, yellow {nearGate[2]}, magenta {nearGate[3]}, white {nearGate[0]})";
        bool best = false;
        if (total > bestTotal)
        {
            bestTotal = total;
            BestTotal = counts;
            best = total >= breakTotalFrom;
        }
        if (even > bestEven)
        {
            bestEven = even;
            BestEven = counts;
            best |= even >= breakEvenFrom;
        }
        if (best && breakOnBest)
        {
            Debug.Break();
        }
    }

    // Absorptions in each consecutive window since the Autopilot started, and the most in any sliding window.
    string WindowCounts(List<Vector2> events)
    {
        float window = Gate.MinusWindow;
        int windows = Mathf.Max(1, Mathf.CeilToInt((simNow - simStart) / window));
        var counts = new int[windows];
        foreach (Vector2 e in events)
        {
            counts[Mathf.Min(windows - 1, (int)((e.x - simStart) / window))] += (int)e.y;
        }

        int most = 0;
        for (int i = 0; i < events.Count; i++)
        {
            int sum = 0;
            for (int j = i; j < events.Count && events[j].x < events[i].x + window; j++)
            {
                sum += (int)events[j].y;
            }
            most = Mathf.Max(most, sum);
        }

        return $"absorbed per {window:F0} s window [{string.Join(", ", counts)}], most in any sliding {window:F0} s window {most}";
    }
}
