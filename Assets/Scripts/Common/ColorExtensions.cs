using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorExtensions
{
    public static Color LerpOpaque(Color a, Color b, float t) => new Color(
        Mathf.Lerp(a.r, b.r, t),
        Mathf.Lerp(a.g, b.g, t),
        Mathf.Lerp(a.b, b.b, t)
        );
}
