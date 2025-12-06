using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolDebugger : MonoBehaviour
{
    [SerializeField] private bool enableDebug = true;
    [SerializeField] private float logInterval = 5f;
    [SerializeField] private bool logOnChange = true;
    [SerializeField] private int maxGemPoolGrowth = 10;
    
    private float lastLogTime;
    private int lastGemActiveCount;
    private int lastGemAvailableCount;
    private int lastWaitForSecondsCacheSize;
    
    private GemPool gemPool;
    private SC_GameLogic gameLogic;
    
    private void Start()
    {
        gameLogic = FindObjectOfType<SC_GameLogic>();
        if (gameLogic == null)
        {
            Debug.LogWarning("[PoolDebugger] SC_GameLogic not found. Pool tracking may be limited.");
            return;
        }
        
        var gemPoolField = typeof(SC_GameLogic).GetField("gemPool", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (gemPoolField != null)
        {
            gemPool = gemPoolField.GetValue(gameLogic) as GemPool;
        }
        
        if (gemPool == null)
        {
            Debug.LogWarning("[PoolDebugger] GemPool not found.");
        }
        
        lastLogTime = Time.time;
        UpdateBaselineCounts();
        
        if (enableDebug)
        {
            Debug.Log($"[PoolDebugger] Started. Baseline counts - Gems Active: {lastGemActiveCount}, Available: {lastGemAvailableCount}, WaitForSeconds Cache: {lastWaitForSecondsCacheSize}");
        }
    }
    
    private void Update()
    {
        if (!enableDebug)
            return;
            
        float currentTime = Time.time;
        
        if (currentTime - lastLogTime >= logInterval)
        {
            LogPoolStatus();
            lastLogTime = currentTime;
        }
        
        if (logOnChange)
        {
            CheckForGrowth();
        }
    }
    
    private void UpdateBaselineCounts()
    {
        if (gemPool != null)
        {
            lastGemActiveCount = gemPool.ActiveCount;
            lastGemAvailableCount = gemPool.AvailableCount;
        }
        
        var cacheField = typeof(WaitForSecondsPool).GetField("cache", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        if (cacheField != null)
        {
            var cache = cacheField.GetValue(null) as Dictionary<float, WaitForSeconds>;
            lastWaitForSecondsCacheSize = cache != null ? cache.Count : 0;
        }
    }
    
    private void CheckForGrowth()
    {
        if (gemPool == null)
            return;
            
        int currentActive = gemPool.ActiveCount;
        int currentAvailable = gemPool.AvailableCount;
        
        int activeGrowth = currentActive - lastGemActiveCount;
        int availableGrowth = currentAvailable - lastGemAvailableCount;
        
        if (activeGrowth > maxGemPoolGrowth || availableGrowth > maxGemPoolGrowth)
        {
            Debug.LogWarning($"[PoolDebugger] POOL GROWTH DETECTED! Active: {lastGemActiveCount} -> {currentActive} (+{activeGrowth}), Available: {lastGemAvailableCount} -> {currentAvailable} (+{availableGrowth})");
        }
        
        lastGemActiveCount = currentActive;
        lastGemAvailableCount = currentAvailable;
    }
    
    private void LogPoolStatus()
    {
        if (gemPool == null)
        {
            Debug.Log("[PoolDebugger] GemPool: Not available");
            return;
        }
        
        var cacheField = typeof(WaitForSecondsPool).GetField("cache", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        int waitForSecondsCacheSize = 0;
        if (cacheField != null)
        {
            var cache = cacheField.GetValue(null) as Dictionary<float, WaitForSeconds>;
            waitForSecondsCacheSize = cache != null ? cache.Count : 0;
        }
        
        Debug.Log($"[PoolDebugger] Pool Status - Gems Active: {gemPool.ActiveCount}, Available: {gemPool.AvailableCount}, Total: {gemPool.ActiveCount + gemPool.AvailableCount}, WaitForSeconds Cache: {waitForSecondsCacheSize}");
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        PoolTracker.GetStats(out var active, out var gets, out var releases);
        if (active.Count > 0)
        {
            Debug.Log("[PoolDebugger] Pooled Collections Status:");
            foreach (var kvp in active)
            {
                int getCount = gets.ContainsKey(kvp.Key) ? gets[kvp.Key] : 0;
                int releaseCount = releases.ContainsKey(kvp.Key) ? releases[kvp.Key] : 0;
                if (kvp.Value > 0)
                {
                    Debug.LogWarning($"  {kvp.Key}: {kvp.Value} active (Gets: {getCount}, Releases: {releaseCount}) - POTENTIAL LEAK!");
                }
                else
                {
                    Debug.Log($"  {kvp.Key}: {kvp.Value} active (Gets: {getCount}, Releases: {releaseCount})");
                }
            }
        }
#endif
        
        UpdateBaselineCounts();
    }
    
    [ContextMenu("Log Pool Status")]
    public void LogPoolStatusManual()
    {
        LogPoolStatus();
    }
    
    [ContextMenu("Check for Leaks")]
    public void CheckForLeaks()
    {
        if (gemPool == null)
        {
            Debug.LogWarning("[PoolDebugger] GemPool not available for leak check.");
            return;
        }
        
        int totalGems = gemPool.ActiveCount + gemPool.AvailableCount;
        int expectedGems = 7 * 7;
        
        if (totalGems > expectedGems * 2)
        {
            Debug.LogError($"[PoolDebugger] POTENTIAL LEAK! Total gems ({totalGems}) is more than 2x expected ({expectedGems}). Active: {gemPool.ActiveCount}, Available: {gemPool.AvailableCount}");
        }
        else
        {
            Debug.Log($"[PoolDebugger] Leak check passed. Total gems: {totalGems}, Expected: ~{expectedGems}");
        }
    }
}

