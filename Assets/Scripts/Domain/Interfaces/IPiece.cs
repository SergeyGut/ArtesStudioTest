
public interface IPiece
{
    GemType Type { get; }
    ref GridPosition Position { get; }
    bool IsColorBomb { get; }
    int BlastSize { get; }
    bool IsMatch { get; set; }
    bool IsSwapMovement { get; set; }
    bool IsMoving { get; }
    bool JustSpawned { get; }
    int ScoreValue { get; }
    void RunDestroyEffect();
}
