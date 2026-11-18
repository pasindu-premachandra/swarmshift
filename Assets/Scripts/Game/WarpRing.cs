using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WarpRing : MonoBehaviour
{
    [SerializeField] UnitColour colour = UnitColour.Cyan;
    [SerializeField] float radius = 1.1f;
    [SerializeField] float bandWidth = 0.2f;

    public UnitColour Colour => colour;
    public float Z => transform.position.z;
    public int Charged { get; private set; }

    public bool Contains(float x)
    {
        return Mathf.Abs(x - transform.position.x) <= radius;
    }

    public UnitColour Charge()
    {
        Charged++;
        return colour;
    }

    void Awake()
    {
        int sides = 32;
        float angle = 0f;
        float shapeRadius = radius;
        if (colour == UnitColour.Yellow)
        {
            sides = 3;
            angle = 90f;
            shapeRadius = radius * 1.4f; // a triangle needs a larger circumradius to be as wide at its centre
        }
        else if (colour == UnitColour.Magenta)
        {
            sides = 4;
            angle = 90f;
        }

        var builder = new MeshBuilder();
        builder.FlatOutline(new Vector3(0f, 0f, 0.03f), shapeRadius + 0.06f, bandWidth + 0.12f, sides, angle, Palette.Ink);
        builder.FlatOutline(Vector3.zero, shapeRadius, bandWidth, sides, angle, Palette.Unit(colour));
        GetComponent<MeshFilter>().sharedMesh = builder.ToMesh(name);
    }
}
