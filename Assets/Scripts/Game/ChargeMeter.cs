using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ChargeMeter : MonoBehaviour
{
    public const int DronesToFill = 180; // at 20 launches per second, holding fire fills the meter in 9 s
    const int Segments = 5;

    [SerializeField] float width = 3.4f;

    readonly int[] segmentStarts = new int[Segments];
    Mesh mesh;
    Color[] colours;
    int shownSegments = -1;

    public int Charge { get; private set; }
    public bool Full => Charge >= DronesToFill;

    void Awake()
    {
        mesh = BuildMesh();
        colours = mesh.colors;
        GetComponent<MeshFilter>().sharedMesh = mesh;
        Show();
    }

    public void Add(int drones)
    {
        Charge = Mathf.Min(DronesToFill, Charge + drones);
        Show();
    }

    public void Empty()
    {
        Charge = 0;
        Show();
    }

    void Show()
    {
        int lit = Charge * Segments / DronesToFill;
        if (lit == shownSegments)
        {
            return;
        }

        shownSegments = lit;
        Color edge = MeshBuilder.ToShaderColour(Full ? Color.white : Palette.Ink); // a white edge means ready
        for (int v = 0; v < 4; v++)
        {
            colours[v] = edge;
        }
        for (int i = 0; i < Segments; i++)
        {
            Color fill = MeshBuilder.ToShaderColour(i < lit ? Color.white : Palette.Raised);
            for (int v = 0; v < 4; v++)
            {
                colours[segmentStarts[i] + v] = fill;
            }
        }
        mesh.colors = colours;
    }

    // The 136 x 28 plate of DESIGN 16.3 scaled to this width, in the XY plane facing -z.
    Mesh BuildMesh()
    {
        float unit = width / 136f;
        float halfWidth = width * 0.5f;
        float halfHeight = 14f * unit;
        float inset = 2.5f * unit;
        var builder = new MeshBuilder();

        builder.Quad(new Vector3(-halfWidth, -halfHeight, 0f), new Vector3(halfWidth, -halfHeight, 0f),
            new Vector3(halfWidth, halfHeight, 0f), new Vector3(-halfWidth, halfHeight, 0f), Palette.Ink);
        builder.Quad(new Vector3(-halfWidth + inset, -halfHeight + inset, -0.01f), new Vector3(halfWidth - inset, -halfHeight + inset, -0.01f),
            new Vector3(halfWidth - inset, halfHeight - inset, -0.01f), new Vector3(-halfWidth + inset, halfHeight - inset, -0.01f), Palette.Panel);

        float glyphX = -halfWidth + 20f * unit;
        builder.Quad(new Vector3(glyphX - 11f * unit, -4f * unit, -0.02f), new Vector3(glyphX + 11f * unit, -4f * unit, -0.02f),
            new Vector3(glyphX + 8f * unit, 4f * unit, -0.02f), new Vector3(glyphX - 8f * unit, 4f * unit, -0.02f), Palette.Hull);

        float top = 7f * unit;
        for (int i = 0; i < Segments; i++)
        {
            float x = -halfWidth + (37f + 19f * i) * unit;
            segmentStarts[i] = builder.VertexCount;
            builder.Quad(new Vector3(x, -top, -0.02f), new Vector3(x + 13f * unit, -top, -0.02f),
                new Vector3(x + 18f * unit, top, -0.02f), new Vector3(x + 5f * unit, top, -0.02f), Palette.Raised);
        }

        return builder.ToMesh("Charge Meter");
    }
}
