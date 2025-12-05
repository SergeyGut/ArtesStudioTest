using System.Collections.Generic;
using UnityEngine;

public static class WaitForSecondsPool
{
    private static readonly Dictionary<float, WaitForSeconds> cache = new Dictionary<float, WaitForSeconds>();

    public static WaitForSeconds Get(float seconds)
    {
        if (!cache.TryGetValue(seconds, out var waitForSeconds))
        {
            waitForSeconds = new WaitForSeconds(seconds);
            cache[seconds] = waitForSeconds;
        }
        return waitForSeconds;
    }

    public static void Clear()
    {
        cache.Clear();
    }
}

