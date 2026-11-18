using UnityEngine;

public static class Palette
{
    public static readonly Color Ink = Hex("#050826");
    public static readonly Color Lane = Hex("#0B1142");
    public static readonly Color LaneBand = Hex("#0F1750");
    public static readonly Color Panel = Hex("#111A55");
    public static readonly Color Raised = Hex("#18237A");
    public static readonly Color FieldLow = Hex("#1F31A3");
    public static readonly Color FieldHigh = Hex("#2B44C8");
    public static readonly Color FieldDeep = Hex("#15206F");
    public static readonly Color Border = Hex("#4B66EA");
    public static readonly Color Rail = Hex("#8FA3FF");
    public static readonly Color Hull = Hex("#F4F7FF");
    public static readonly Color HullShade = Hex("#A7B5E6");
    public static readonly Color White = Hex("#FFFFFF");
    public static readonly Color Cyan = Hex("#18E2FF");
    public static readonly Color CyanShade = Hex("#0A9CD1");
    public static readonly Color CyanLight = Hex("#B5F7FF");
    public static readonly Color Yellow = Hex("#FFE01A");
    public static readonly Color YellowShade = Hex("#D99A00");
    public static readonly Color YellowLight = Hex("#FFF4A3");
    public static readonly Color Magenta = Hex("#FF52E3");
    public static readonly Color MagentaShade = Hex("#C21AA5");
    public static readonly Color MagentaLight = Hex("#FFC6F5");
    public static readonly Color Plus = Hex("#27E36C");
    public static readonly Color PlusShade = Hex("#12A04B");
    public static readonly Color Minus = Hex("#FF3B4F");
    public static readonly Color MinusShade = Hex("#B8142C");
    public static readonly Color MinusLight = Hex("#FFB0B8");
    public static readonly Color Hive900 = Hex("#1C0736");
    public static readonly Color Hive700 = Hex("#3A1170");
    public static readonly Color Hive500 = Hex("#7A2CF0");
    public static readonly Color Hive300 = Hex("#BC8CFF");
    public static readonly Color HiveCrystal = Hex("#4E1A94");
    public static readonly Color Credit = Hex("#FFAE1F");
    public static readonly Color Planet = Hex("#5A74F0");
    public static readonly Color PlanetShade = Hex("#2E46C2");
    public static readonly Color Haze = Hex("#5FD4FF");

    public static Color UnitLight(UnitColour colour)
    {
        switch (colour)
        {
            case UnitColour.Cyan: return CyanLight;
            case UnitColour.Yellow: return YellowLight;
            case UnitColour.Magenta: return MagentaLight;
            default: return White;
        }
    }

    public static Color Unit(UnitColour colour)
    {
        switch (colour)
        {
            case UnitColour.Cyan: return Cyan;
            case UnitColour.Yellow: return Yellow;
            case UnitColour.Magenta: return Magenta;
            default: return Hull;
        }
    }

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color colour);
        return colour;
    }
}
