using System.Collections.Generic;
using UnityEngine;

public interface IGameBoard
{
    int Width { get; }
    int Height { get; }
    int Score { get; set; }
    HashSet<SC_Gem> Explosions { get; }
    List<MatchInfo> MatchInfoMap { get; }
    
    bool MatchesAt(Vector2Int position, SC_Gem gemToCheck);
    void SetGem(int x, int y, SC_Gem gem);
    SC_Gem GetGem(int x, int y);
    void FindAllMatches(Vector2Int? userActionPos = null);
}

public class MatchInfo
{
    public HashSet<SC_Gem> MatchedGems;
    public Vector2Int? UserActionPos;
}

