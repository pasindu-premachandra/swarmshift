using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Lane : MonoBehaviour
{
    public const float HalfWidth = 5f;

    [SerializeField] float nearZ = -6f;
    [SerializeField] float farZ = 40f;
    [SerializeField] float railWidth = 0.35f;
    [SerializeField] float railHeight = 0.3f;

    void Awake()
    {
        GetComponent<MeshFilter>().sharedMesh = BuildMesh();
    }

    Mesh BuildMesh()
    {
        var builder = new MeshBuilder();

        for (float z = nearZ; z < farZ; z += 2f)
        {
            float middle = Mathf.Min(z + 1f, farZ);
            float end = Mathf.Min(z + 2f, farZ);
            LaneStrip(builder, z, middle, Palette.Lane);
            LaneStrip(builder, middle, end, Palette.LaneBand);
        }

        for (int side = -1; side <= 1; side += 2)
        {
            float x = side * (HalfWidth + railWidth * 0.5f);
            builder.Box(new Vector3(x, railHeight * 0.5f, (nearZ + farZ) * 0.5f),
                new Vector3(railWidth, railHeight, farZ - nearZ), Palette.Rail, Palette.Border);

            for (float z = nearZ + 0.75f; z < farZ; z += 1.5f)
            {
                builder.Box(new Vector3(x, railHeight + 0.01f, z), new Vector3(0.12f, 0.02f, 0.12f), Color.white, Color.white);
            }
        }

        return builder.ToMesh("Lane");
    }

    static void LaneStrip(MeshBuilder builder, float fromZ, float toZ, Color colour)
    {
        if (toZ <= fromZ)
        {
            return;
        }

        builder.Quad(new Vector3(-HalfWidth, 0f, fromZ), new Vector3(HalfWidth, 0f, fromZ),
            new Vector3(HalfWidth, 0f, toZ), new Vector3(-HalfWidth, 0f, toZ), colour);
    }
}
