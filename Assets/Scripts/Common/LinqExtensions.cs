using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class LinqExtensions
{
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> src, System.Action<T> action)
    {
        foreach (var i in src) action(i);
        return src;
    }

    public static T Random<T>(this IEnumerable<T> src) where T:Object
        => src.Count() == 0 ? null : src.ElementAt(Mathf.FloorToInt(UnityEngine.Random.value * src.Count()));
}
