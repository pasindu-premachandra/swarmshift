using UnityEngine;

// One spec mesh from the design's swarmshift-meshes.js, as exported into SpecMeshData: flat faces with baked lit and shade colours.
public class SpecMesh
{
    public readonly string Name;
    public readonly Vector3 Origin;
    public readonly float Radius;

    readonly float[] positions;
    readonly byte[] colours;
    readonly int[][] submeshes;

    public SpecMesh(string name, Vector3 origin, float radius, float[] positions, byte[] colours, int[][] submeshes)
    {
        Name = name;
        Origin = origin;
        Radius = radius;
        this.positions = positions;
        this.colours = colours;
        this.submeshes = submeshes;
    }

    public Mesh ToMesh()
    {
        var vertices = new Vector3[colours.Length];
        var vertexColours = new Color[colours.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(positions[i * 3], positions[i * 3 + 1], positions[i * 3 + 2]);
            vertexColours[i] = MeshBuilder.ToShaderColour(SpecMeshData.Colours[colours[i]]);
        }

        var mesh = new Mesh { name = Name };
        mesh.vertices = vertices;
        mesh.colors = vertexColours;
        mesh.subMeshCount = submeshes.Length;
        for (int i = 0; i < submeshes.Length; i++)
        {
            mesh.SetTriangles(submeshes[i], i);
        }
        mesh.RecalculateBounds();
        return mesh;
    }
}
