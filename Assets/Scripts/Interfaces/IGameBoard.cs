using System.Collections.Generic;
using UnityEngine;

public interface IGameBoard
{
    int Width { get; }
    int Height { get; }
    HashSet<SC_Gem> Explosions { get; }
    List<MatchInfo> MatchInfoMap { get; }

    int GetMatchCountAt(Vector2Int _PositionToCheck, SC_Gem _GemToCheck);
    void SetGem(int x, int y, SC_Gem gem);
    SC_Gem GetGem(int x, int y);
    void SetGem(Vector2Int position, SC_Gem gem);
    SC_Gem GetGem(Vector2Int position);
    void FindAllMatches(Vector2Int? userActionPos = null);
}

public class MatchInfo
{
    public HashSet<SC_Gem> MatchedGems;
    public Vector2Int? UserActionPos;
}

