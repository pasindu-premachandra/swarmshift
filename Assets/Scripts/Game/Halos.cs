using UnityEngine;

// The only glow (DESIGN 9): camera-facing quads with one 64 px radial texture, tinted per instance, drawn in one instanced call.
public class Halos
{
    public const float Scale = 1.6f; // halo diameter against its object

    const int Capacity = 64;
    const int TextureSize = 64;
    static readonly int HaloTexId = Shader.PropertyToID("_HaloTex");
    static Texture2D texture;

    readonly InstanceBatch batch;
    readonly Transform view;

    public Halos(Material material, Transform view)
    {
        if (texture == null)
        {
            texture = BuildTexture();
            Shader.SetGlobalTexture(HaloTexId, texture);
        }
        batch = new InstanceBatch(BuildQuad(), material, Capacity);
        this.view = view;
    }

    public void Clear()
    {
        batch.Clear();
    }

    // lift slides the sprite along the camera ray, positive toward the camera, keeping its size on screen
    public void Add(Vector3 centre, Vector2 size, Color light, float centreAlpha, float lift)
    {
        Vector3 eye = view.position;
        float distance = Vector3.Distance(eye, centre);
        float keep = (distance - lift) / distance;
        Vector3 position = Vector3.LerpUnclamped(eye, centre, keep);
        Vector4 colour = MeshBuilder.ToShaderColour(light);
        colour.w = centreAlpha;
        batch.Add(Matrix4x4.TRS(position, view.rotation, new Vector3(size.x * keep, size.y * keep, 1f)), colour);
    }

    public void Draw(Bounds worldBounds)
    {
        batch.Draw(worldBounds);
    }

    // Alpha 1.0 at the centre, 0.35 at half radius, 0 at the edge
    static Texture2D BuildTexture()
    {
        var halo = new Texture2D(TextureSize, TextureSize, TextureFormat.R8, false, true)
        {
            name = "Halo",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.DontUnloadUnusedAsset
        };
        var pixels = new byte[TextureSize * TextureSize];
        float half = TextureSize * 0.5f;
        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++)
            {
                float r = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(half, half)) / half;
                float alpha = r < 0.5f ? Mathf.Lerp(1f, 0.35f, r * 2f) : Mathf.Lerp(0.35f, 0f, r * 2f - 1f);
                pixels[y * TextureSize + x] = (byte)Mathf.RoundToInt(alpha * 255f);
            }
        }
        halo.SetPixelData(pixels, 0);
        halo.Apply(false, true);
        return halo;
    }

    static Mesh BuildQuad()
    {
        var quad = new Mesh { name = "Halo" };
        quad.vertices = new[] { new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f), new Vector3(0.5f, 0.5f, 0f), new Vector3(-0.5f, 0.5f, 0f) };
        quad.uv = new[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f) };
        quad.triangles = new[] { 0, 2, 1, 0, 3, 2 };
        quad.RecalculateBounds();
        return quad;
    }
}
