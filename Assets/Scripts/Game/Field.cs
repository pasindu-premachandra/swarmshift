using System.Collections.Generic;
using UnityEngine;

// The art sheet's field card as a camera-child backdrop: a three-row gradient, a two-tone planet and 80 star diamonds.
// The faint cyan haze is a halo quad (see AddHalos). Laid out on the 390 x 844 frame like the kit's backdrop().
[RequireComponent(typeof(MeshFilter))]
public class Field : MonoBehaviour
{
    const float FrameWidth = 390f;
    const float FrameHeight = 844f;
    const uint StarSeed = 9; // the field card's seed
    const int Stars = 80;
    const float HazeAlpha = 0.14f;

    [SerializeField] float distance = 150f;

    Camera view;
    float builtAspect;
    float width;
    float height;

    void Awake()
    {
        view = GetComponentInParent<Camera>();
        Build();
    }

    void LateUpdate()
    {
        if (view.aspect != builtAspect)
        {
            Build(); // the frame was resized
        }
    }

    public void AddHalos(Halos halos)
    {
        float k = (distance - 1f) / distance; // just in front of the field
        Vector3 centre = transform.TransformPoint(At(0.92f, 0.42f) * k);
        halos.Add(centre, new Vector2(width * 0.9f, height * 0.32f) * k, Palette.Haze, HazeAlpha, 0f);
    }

    void Build()
    {
        builtAspect = view.aspect;
        height = 2f * distance * Mathf.Tan(view.fieldOfView * 0.5f * Mathf.Deg2Rad);
        width = height * view.aspect;
        float dp = height / FrameHeight;
        var builder = new MeshBuilder();

        builder.Quad(At(0f, 0.55f), At(1f, 0.55f), At(1f, 0f), At(0f, 0f), Palette.FieldLow, Palette.FieldHigh);
        builder.Quad(At(0f, 1f), At(1f, 1f), At(1f, 0.55f), At(0f, 0.55f), Palette.FieldDeep, Palette.FieldLow);

        // Planet: radius 13% of the frame width at (6%, 24%); the shade disc is offset (0.32, 0.26) radii down-right and clipped to the planet
        Vector3 planet = At(0.06f, 0.24f);
        float radius = 0.13f * FrameWidth * dp;
        var disc = new List<Vector3>();
        for (int i = 0; i < 16; i++)
        {
            disc.Add(planet + Direction(i * 22.5f) * radius);
        }
        Fan(builder, disc, Palette.Planet);
        Fan(builder, Lens(planet, planet + new Vector3(0.32f, -0.26f, 0f) * radius, radius), Palette.PlanetShade);

        // Stars: the kit's generator, 0.5 to 1.6 dp radius in the top 75%, white at 45 to 95%
        uint seed = StarSeed;
        for (int i = 0; i < Stars; i++)
        {
            float u = Random01(ref seed);
            float v = Random01(ref seed) * 0.75f;
            float r = (0.5f + Random01(ref seed) * 1.1f) * dp;
            Color colour = Palette.White;
            colour.a = 0.45f + Random01(ref seed) * 0.5f;
            Vector3 star = At(u, v);
            builder.Quad(star + Vector3.up * r, star + Vector3.right * r, star + Vector3.down * r, star + Vector3.left * r, colour);
        }

        GetComponent<MeshFilter>().sharedMesh = builder.ToMesh("Field");
    }

    // Frame point (0, 0 top left; 1, 1 bottom right) on the field plane, in camera space
    Vector3 At(float u, float v)
    {
        return new Vector3((u - 0.5f) * width, (0.5f - v) * height, distance);
    }

    static Vector3 Direction(float degrees)
    {
        float angle = degrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
    }

    // The part of the disc at shade that lies inside the disc at lit, both of this radius: 8 points on each arc, 16 in all
    static List<Vector3> Lens(Vector3 lit, Vector3 shade, float radius)
    {
        Vector3 between = shade - lit;
        float toShade = Mathf.Atan2(between.y, between.x) * Mathf.Rad2Deg;
        float half = Mathf.Acos(between.magnitude * 0.5f / radius) * Mathf.Rad2Deg;
        var points = new List<Vector3>();
        for (int i = 0; i < 8; i++)
        {
            points.Add(lit + Direction(toShade - half + 2f * half * i / 7f) * radius);
        }
        for (int i = 1; i <= 8; i++)
        {
            points.Add(shade + Direction(toShade + 180f - half + 2f * half * i / 9f) * radius);
        }
        return points;
    }

    static void Fan(MeshBuilder builder, List<Vector3> polygon, Color colour)
    {
        for (int i = 1; i < polygon.Count - 1; i++)
        {
            builder.Triangle(polygon[0], polygon[i], polygon[i + 1], colour);
        }
    }

    // The kit's rng(): a 32-bit linear congruential generator
    static float Random01(ref uint seed)
    {
        seed = seed * 1664525u + 1013904223u;
        return (float)(seed / 4294967296.0);
    }
}
