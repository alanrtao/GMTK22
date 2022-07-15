using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathExtensions
{
    public static Vector2 Reciprocal(this Vector2 v)
    {
        v.x = 1 / v.x;
        v.y = 1 / v.y;
        return v;
    }

    public static Vector3 Reciprocal(this Vector3 v)
    {
        v.x = 1 / v.x;
        v.y = 1 / v.y;
        v.z = 1 / v.z;
        return v;
    }

    public static Vector3 Extend(this Vector2 v, float z = 1) => new Vector3(v.x, v.y, z);

    public static Vector2 Mult(this Vector2 u, Vector2 v) => new Vector2(u.x * v.x, u.y * v.y);
    public static Vector2 Div(this Vector2 u, Vector2 v) => new Vector2(u.x / v.x, u.y / v.y);
    public static float Max(this Vector2 u) => Mathf.Max(u.x, u.y);
}