using UnityEngine;

public class SC_GameVariables : MonoBehaviour
{
    public GameObject bgTilePrefabs;
    public SC_Gem bomb;
    public SC_Gem[] gems;
    public SC_Gem[] gemBombs;
    public float bonusAmount = 0.5f;
    public float bombChance = 2f;
    public int dropHeight = 0;
    public float gemSpeed;
    public AnimationCurve gemEaseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve gemSwapEaseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public float scoreSpeed = 5;
    public float bombNeighborDelay = 0.2f;
    public float bombSelfDelay = 0.3f;
    public float bombPostSelfDelay = 0.3f;
    public float decreaseRowDelay = 0.2f;
    public float decreaseSingleRowDelay = 0.05f;
    public float findAllMatchesDelay = 0.5f;
    public float destroyMatchesDelay = 0.5f;
    public float changeStateDelay = 0.5f;
    public int minMatchForBomb = 4;
    
    [HideInInspector]
    public int rowsSize = 7;
    [HideInInspector]
    public int colsSize = 7;

    #region Singleton

    static SC_GameVariables instance;
    public static SC_GameVariables Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SC_GameVariables>();
            }

            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    #endregion
}
