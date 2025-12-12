using System.Collections.Generic;

namespace Domain.Pool
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public static class PoolTracker
    {
        private static Dictionary<string, int> activeCounts = new Dictionary<string, int>();
        private static Dictionary<string, int> totalGets = new Dictionary<string, int>();
        private static Dictionary<string, int> totalReleases = new Dictionary<string, int>();

        public static void TrackGet(string poolName)
        {
            if (!activeCounts.ContainsKey(poolName))
                activeCounts[poolName] = 0;
            activeCounts[poolName]++;

            if (!totalGets.ContainsKey(poolName))
                totalGets[poolName] = 0;
            totalGets[poolName]++;
        }

        public static void TrackRelease(string poolName)
        {
            if (activeCounts.ContainsKey(poolName) && activeCounts[poolName] > 0)
                activeCounts[poolName]--;

            if (!totalReleases.ContainsKey(poolName))
                totalReleases[poolName] = 0;
            totalReleases[poolName]++;
        }

        public static int GetActiveCount(string poolName)
        {
            return activeCounts.ContainsKey(poolName) ? activeCounts[poolName] : 0;
        }

        public static void GetStats(out Dictionary<string, int> active, out Dictionary<string, int> gets,
            out Dictionary<string, int> releases)
        {
            active = new Dictionary<string, int>(activeCounts);
            gets = new Dictionary<string, int>(totalGets);
            releases = new Dictionary<string, int>(totalReleases);
        }

        public static void Clear()
        {
            activeCounts.Clear();
            totalGets.Clear();
            totalReleases.Clear();
        }
    }
#endif
}
