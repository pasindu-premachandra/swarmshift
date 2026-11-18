using UnityEngine;
using UnityEngine.Rendering;

public class InstanceBatch
{
    const int ChunkSize = 500;
    static readonly int ColorId = Shader.PropertyToID("_Color");

    readonly Mesh mesh;
    readonly Material[] materials;
    readonly int capacity;
    readonly Matrix4x4[][] matrices;
    readonly Vector4[][] colours;
    readonly MaterialPropertyBlock[] blocks;
    int count;

    public InstanceBatch(Mesh mesh, Material material, int capacity) : this(mesh, new[] { material }, capacity)
    {
    }

    // One material per submesh.
    public InstanceBatch(Mesh mesh, Material[] materials, int capacity)
    {
        this.mesh = mesh;
        this.materials = materials;
        this.capacity = capacity;

        int chunks = (capacity + ChunkSize - 1) / ChunkSize;
        matrices = new Matrix4x4[chunks][];
        colours = new Vector4[chunks][];
        blocks = new MaterialPropertyBlock[chunks];
        for (int i = 0; i < chunks; i++)
        {
            matrices[i] = new Matrix4x4[ChunkSize];
            colours[i] = new Vector4[ChunkSize];
            blocks[i] = new MaterialPropertyBlock();
        }
    }

    public void Clear()
    {
        count = 0;
    }

    public void Add(Matrix4x4 matrix, Vector4 colour)
    {
        if (count >= capacity)
        {
            return;
        }

        matrices[count / ChunkSize][count % ChunkSize] = matrix;
        colours[count / ChunkSize][count % ChunkSize] = colour;
        count++;
    }

    public void Draw(Bounds worldBounds)
    {
        for (int chunk = 0; chunk * ChunkSize < count; chunk++)
        {
            int instances = Mathf.Min(ChunkSize, count - chunk * ChunkSize);
            blocks[chunk].SetVectorArray(ColorId, colours[chunk]);
            for (int submesh = 0; submesh < materials.Length; submesh++)
            {
                var renderParams = new RenderParams(materials[submesh])
                {
                    matProps = blocks[chunk],
                    worldBounds = worldBounds,
                    shadowCastingMode = ShadowCastingMode.Off,
                    receiveShadows = false
                };
                Graphics.RenderMeshInstanced(renderParams, mesh, submesh, matrices[chunk], instances);
            }
        }
    }
}
