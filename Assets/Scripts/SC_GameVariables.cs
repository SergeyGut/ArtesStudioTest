using System.Collections;
using System.Collections.Generic;
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
    public float scoreSpeed = 5;
    public float bombNeighborDelay = 0.2f;
    public float bombSelfDelay = 0.3f;
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
                instance = GameObject.Find("SC_GameVariables").GetComponent<SC_GameVariables>();

            return instance;
        }
    }

    #endregion
}
