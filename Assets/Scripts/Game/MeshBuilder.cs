using System.Collections.Generic;
using UnityEngine;

public class MeshBuilder
{
    readonly List<Vector3> vertices = new List<Vector3>();
    readonly List<Color> colours = new List<Color>();
    readonly List<int> triangles = new List<int>();

    public int VertexCount => vertices.Count;

    public void Triangle(Vector3 a, Vector3 b, Vector3 c, Color colour)
    {
        int start = vertices.Count;
        AddVertex(a, colour);
        AddVertex(b, colour);
        AddVertex(c, colour);
        triangles.Add(start);
        triangles.Add(start + 1);
        triangles.Add(start + 2);
    }

    public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color colour)
    {
        Quad(a, b, c, d, colour, colour);
    }

    public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color colourAB, Color colourCD)
    {
        int start = vertices.Count;
        AddVertex(a, colourAB);
        AddVertex(b, colourAB);
        AddVertex(c, colourCD);
        AddVertex(d, colourCD);
        triangles.Add(start);
        triangles.Add(start + 1);
        triangles.Add(start + 2);
        triangles.Add(start);
        triangles.Add(start + 2);
        triangles.Add(start + 3);
    }

    public void Box(Vector3 centre, Vector3 size, Color top, Color side)
    {
        Vector3 h = size * 0.5f;
        Vector3 p000 = centre + new Vector3(-h.x, -h.y, -h.z);
        Vector3 p100 = centre + new Vector3(h.x, -h.y, -h.z);
        Vector3 p010 = centre + new Vector3(-h.x, h.y, -h.z);
        Vector3 p110 = centre + new Vector3(h.x, h.y, -h.z);
        Vector3 p001 = centre + new Vector3(-h.x, -h.y, h.z);
        Vector3 p101 = centre + new Vector3(h.x, -h.y, h.z);
        Vector3 p011 = centre + new Vector3(-h.x, h.y, h.z);
        Vector3 p111 = centre + new Vector3(h.x, h.y, h.z);

        Quad(p010, p110, p111, p011, top);
        Quad(p000, p100, p110, p010, side);
        Quad(p001, p101, p111, p011, side);
        Quad(p000, p001, p011, p010, side);
        Quad(p100, p101, p111, p110, side);
    }

    public void Prism(Vector3 baseCentre, float radius, float height, int sides, float angleDegrees, Color top, Color side)
    {
        Vector3 up = Vector3.up * height;
        for (int i = 0; i < sides; i++)
        {
            Vector3 a = baseCentre + RingPoint(radius, sides, angleDegrees, i, true);
            Vector3 b = baseCentre + RingPoint(radius, sides, angleDegrees, i + 1, true);
            Quad(a, b, b + up, a + up, side);
            Triangle(baseCentre + up, a + up, b + up, top);
        }
    }

    public void FlatPolygon(Vector3 centre, float radius, int sides, float angleDegrees, Color colour)
    {
        for (int i = 0; i < sides; i++)
        {
            Triangle(centre,
                centre + RingPoint(radius, sides, angleDegrees, i, false),
                centre + RingPoint(radius, sides, angleDegrees, i + 1, false),
                colour);
        }
    }

    public void FlatOutline(Vector3 centre, float radius, float width, int sides, float angleDegrees, Color colour)
    {
        float inner = radius - width / Mathf.Cos(Mathf.PI / sides); // keeps the band the same width on every edge
        for (int i = 0; i < sides; i++)
        {
            Quad(centre + RingPoint(inner, sides, angleDegrees, i, false),
                centre + RingPoint(radius, sides, angleDegrees, i, false),
                centre + RingPoint(radius, sides, angleDegrees, i + 1, false),
                centre + RingPoint(inner, sides, angleDegrees, i + 1, false),
                colour);
        }
    }

    public Mesh ToMesh(string name)
    {
        var mesh = new Mesh { name = name };
        mesh.SetVertices(vertices);
        mesh.SetColors(colours);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        return mesh;
    }

    public static Vector4 ToShaderColour(Color colour)
    {
        // vertex colours and vector arrays skip the sRGB to linear conversion that material colours get
        return QualitySettings.activeColorSpace == ColorSpace.Linear ? colour.linear : colour;
    }

    void AddVertex(Vector3 position, Color colour)
    {
        vertices.Add(position);
        colours.Add(ToShaderColour(colour));
    }

    static Vector3 RingPoint(float radius, int sides, float angleDegrees, int index, bool horizontal)
    {
        float angle = (angleDegrees + 360f * index / sides) * Mathf.Deg2Rad;
        float a = Mathf.Cos(angle) * radius;
        float b = Mathf.Sin(angle) * radius;
        return horizontal ? new Vector3(a, 0f, b) : new Vector3(a, b, 0f);
    }
}
