using System;

public class GameBoard : IGameBoard
{
    private readonly int height;
    private readonly int width;
    private readonly IPiece[,] allGems;
    
    public int Width => width;
    public int Height => height;

    public GameBoard(IBoardSettings settings)
    {
        height = settings.RowsSize;
        width = settings.ColsSize;
        allGems = new IPiece[width, height];
    }

    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public void SetGem(int x, int y, IPiece gem)
    {
        if (!IsValidPosition(x, y))
        {
            throw new ArgumentException($"SetGem: Position ({x}, {y}) is out of bounds.");
        }
        
        allGems[x, y] = gem;
    }
    
    public IPiece GetGem(int x, int y)
    {
        if (!IsValidPosition(x, y))
        {
            throw new ArgumentException($"GetGem: Position ({x}, {y}) is out of bounds.");
        }
        
        return allGems[x, y];
    }

    public void SetGem(GridPosition position, IPiece gem)
    {
        SetGem(position.X, position.Y, gem);
    }

    public IPiece GetGem(GridPosition position)
    {
        return GetGem(position.X, position.Y);
    }
}

