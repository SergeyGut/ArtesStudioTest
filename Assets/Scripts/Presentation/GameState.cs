
public class GameStateProvider : IGameStateProvider
{
    private GameState currentState = GameState.move;

    public GameState CurrentState => currentState;

    public void SetState(GameState newState)
    {
        currentState = newState;
    }
}
