using UnityEngine;
using System.Linq;

public class Utility
{
    public static Color FloatsToColor(float[] rgb) {
        return new Color(rgb[0], rgb[1], rgb[2]);
    }

    public static string ID_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    public static int ID_LENGTH = 32;
    public static string GenerateId()
    {
        var random = new System.Random();
        return new string(Enumerable.Repeat(ID_CHARS, ID_LENGTH).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
