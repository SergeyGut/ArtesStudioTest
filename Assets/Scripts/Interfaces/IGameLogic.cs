using UnityEngine;

public interface IGameLogic
{
    GlobalEnums.GameState CurrentState { get; }
    
    void SetState(GlobalEnums.GameState state);
    void FindAllMatches(Vector2Int? posIndex = null, Vector2Int? otherPosIndex = null);
    void DestroyMatches();
}

