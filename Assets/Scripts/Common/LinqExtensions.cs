using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LinqExtensions
{
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> src, System.Action<T> action)
    {
        foreach (var i in src) action(i);
        return src;
    }
}
