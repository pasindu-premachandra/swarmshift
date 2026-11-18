using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Hive : MonoBehaviour
{
    const float GlowAlpha = 0.6f;
    const float PodHaloAlpha = 0.55f;
    const int PodQueue = 3001; // pods draw after the halos, so a pod covers its own halo
    static readonly Color[] PodLights = { Palette.CyanLight, Palette.YellowLight, Palette.MagentaLight };

    [SerializeField] int startHitPoints = 120;
    [SerializeField] float radius = 2.4f;
    [SerializeField] TextMeshPro label;

    readonly Transform[] pods = new Transform[3];
    Bounds body;

    public int HitPoints { get; private set; }
    public int StartHitPoints => startHitPoints;
    public float X => transform.position.x;
    public float HalfWidth => radius;
    public float FrontZ => transform.position.z - radius * 0.866f;

    void Awake()
    {
        HitPoints = startHitPoints;
        label.SetText("{0}", HitPoints);

        // The spec hive: five crystal pieces and three pods, kept separate so they can be thrown apart; its front face stands at FrontZ
        Material world = GetComponent<MeshRenderer>().sharedMaterial;
        var podMaterial = new Material(world) { name = "Hive Pod", renderQueue = PodQueue };
        var spec = new GameObject("Spec").transform;
        spec.SetParent(transform, false);
        spec.localPosition = new Vector3(0f, 0f, -radius * 0.866f - SpecMeshData.HiveFrontZ);
        body = AddPiece(spec, SpecMeshData.HiveCrystals[0], world).bounds;
        for (int i = 1; i < SpecMeshData.HiveCrystals.Length; i++)
        {
            body.Encapsulate(AddPiece(spec, SpecMeshData.HiveCrystals[i], world).bounds);
        }
        for (int i = 0; i < pods.Length; i++)
        {
            MeshRenderer pod = AddPiece(spec, SpecMeshData.HivePods[i], podMaterial);
            body.Encapsulate(pod.bounds);
            pods[i] = pod.transform;
        }

        // Number plate, its bottom 0.5 above the spike tips
        float plateHeight = 2.25f;
        Vector3 plate = new Vector3(0f, body.max.y - transform.position.y + 0.5f + plateHeight * 0.5f, -0.5f);
        var builder = new MeshBuilder();
        builder.Box(plate + new Vector3(0f, 0f, 0.05f), new Vector3(4.85f, plateHeight, 0.15f), Palette.Hive300, Palette.Hive300);
        builder.Box(plate, new Vector3(4.6f, 2f, 0.2f), Palette.Hive900, Palette.Hive900);
        label.transform.localPosition = plate + new Vector3(0f, 0f, -0.15f);

        GetComponent<MeshFilter>().sharedMesh = builder.ToMesh("Hive Plate");
    }

    // The hive glow sits behind the whole hive; each pod's halo sits in front of the crystal face and under its pod
    public void AddHalos(Halos halos)
    {
        halos.Add(body.center, new Vector2(body.size.x, body.size.y) * Halos.Scale, Palette.Hive500, GlowAlpha, -(body.extents.z + 1f));
        for (int i = 0; i < pods.Length; i++)
        {
            float size = 2f * SpecMeshData.HivePods[i].Radius * Halos.Scale;
            halos.Add(pods[i].position, new Vector2(size, size), PodLights[i], PodHaloAlpha, size * 0.5f);
        }
    }

    public void Hit()
    {
        Damage(1);
    }

    public void Damage(int amount)
    {
        if (HitPoints > 0)
        {
            HitPoints = Mathf.Max(0, HitPoints - amount);
            label.SetText("{0}", HitPoints);
        }
    }

    static MeshRenderer AddPiece(Transform parent, SpecMesh spec, Material material)
    {
        var piece = new GameObject(spec.Name);
        piece.transform.SetParent(parent, false);
        piece.transform.localPosition = spec.Origin;
        piece.AddComponent<MeshFilter>().sharedMesh = spec.ToMesh();
        var renderer = piece.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return renderer;
    }
}
