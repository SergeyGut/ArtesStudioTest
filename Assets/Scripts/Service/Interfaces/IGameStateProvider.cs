
public interface IGameStateProvider
{
    GameState CurrentState { get; }
    void SetState(GameState state);
}
