using UnityEngine;

public interface IGameLogic
{
    GlobalEnums.GameState CurrentState { get; }
    
    void SetState(GlobalEnums.GameState state);
    void SetGem(int x, int y, SC_Gem gem);
    SC_Gem GetGem(int x, int y);
    void FindAllMatches(Vector2Int? posIndex = null, Vector2Int? otherPosIndex = null);
    void DestroyMatches();
}

