using System.Collections.Generic;

public interface IGameBoard
{
    int Width { get; }
    int Height { get; }

    void SetGem(int x, int y, IPiece gem);
    IPiece GetGem(int x, int y);
    void SetGem(GridPosition position, IPiece gem);
    IPiece GetGem(GridPosition position);
    bool IsValidPosition(int x, int y);
}


