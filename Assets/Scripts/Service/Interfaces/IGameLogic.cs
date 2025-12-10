
public interface IGameLogic
{
    GameState CurrentState { get; }
    
    void SetState(GameState state);
    void FindAllMatches(GridPosition? posIndex = null, GridPosition? otherPosIndex = null);
    void DestroyMatches();
}

