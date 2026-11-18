using TMPro;
using UnityEngine;

public class Gate : MonoBehaviour
{
    public const float MinusWindow = 1f; // seconds; a -N gate absorbs at most N drones in any window this long

    [SerializeField] int value = 2; // xN when positive, -N when negative
    [SerializeField] float halfWidth = 2.5f;
    [SerializeField] float height = 2.4f;
    [SerializeField] MeshFilter frame;
    [SerializeField] MeshFilter field;
    [SerializeField] TextMeshPro label;

    readonly int[] crossings = new int[4];
    float[] absorbTimes;
    int nextAbsorb;

    public int Value => value;
    public float Z => transform.position.z;
    public int Absorbed { get; private set; }

    public bool Contains(float x)
    {
        return Mathf.Abs(x - transform.position.x) <= halfWidth;
    }

    public int Crossings(UnitColour colour)
    {
        return crossings[(int)colour];
    }

    // How many drones leave the gate for one that crosses at this time: N for xN, 0 or 1 for -N.
    public int Cross(UnitColour colour, float time)
    {
        crossings[(int)colour]++;
        if (value > 0)
        {
            return value;
        }

        if (time - absorbTimes[nextAbsorb] < MinusWindow)
        {
            return 1; // the oldest of the last N absorptions is inside the window, so this one passes
        }

        absorbTimes[nextAbsorb] = time;
        nextAbsorb = (nextAbsorb + 1) % absorbTimes.Length;
        Absorbed++;
        return 0;
    }

    void Awake()
    {
        absorbTimes = new float[Mathf.Max(1, -value)];
        for (int i = 0; i < absorbTimes.Length; i++)
        {
            absorbTimes[i] = float.NegativeInfinity;
        }

        bool plus = value > 0;
        Color light = plus ? Palette.Plus : Palette.Minus;
        Color shade = plus ? Palette.PlusShade : Palette.MinusShade;

        var frameBuilder = new MeshBuilder();
        float pylonX = halfWidth - 0.15f;
        frameBuilder.Box(new Vector3(-pylonX, height * 0.5f, 0f), new Vector3(0.3f, height, 0.3f), light, shade);
        frameBuilder.Box(new Vector3(pylonX, height * 0.5f, 0f), new Vector3(0.3f, height, 0.3f), light, shade);
        frameBuilder.Box(new Vector3(0f, height - 0.1f, 0f), new Vector3(halfWidth * 2f, 0.2f, 0.25f), light, shade);

        Vector3 plate = new Vector3(0f, height + 0.6f, 0f);
        frameBuilder.Box(plate + new Vector3(0f, 0f, 0.05f), new Vector3(2.6f, 1.35f, 0.15f), Palette.Ink, Palette.Ink);
        frameBuilder.Box(plate, new Vector3(2.4f, 1.15f, 0.2f), light, shade);
        frame.sharedMesh = frameBuilder.ToMesh(name + " Frame");

        var fieldBuilder = new MeshBuilder();
        float left = -pylonX;
        float right = pylonX;
        float top = height - 0.2f;
        fieldBuilder.Quad(new Vector3(left, 0f, 0f), new Vector3(right, 0f, 0f), new Vector3(right, top, 0f), new Vector3(left, top, 0f),
            new Color(light.r, light.g, light.b, 0.22f));
        for (float y = 0.2f; y < top; y += 0.3f)
        {
            fieldBuilder.Quad(new Vector3(left, y, -0.01f), new Vector3(right, y, -0.01f), new Vector3(right, y + 0.06f, -0.01f),
                new Vector3(left, y + 0.06f, -0.01f), new Color(light.r, light.g, light.b, 0.3f));
        }
        field.sharedMesh = fieldBuilder.ToMesh(name + " Field");

        label.text = plus ? "×" + value : "-" + -value;
        label.transform.localPosition = plate + new Vector3(0f, 0f, -0.15f);
    }
}
