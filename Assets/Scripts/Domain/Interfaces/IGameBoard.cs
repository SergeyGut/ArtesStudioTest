
namespace Domain.Interfaces
{
    public interface IGameBoard
    {
        int Width { get; }
        int Height { get; }

        void SetPiece(int x, int y, IPiece piece);
        IPiece GetPiece(int x, int y);
        void SetPiece(GridPosition position, IPiece piece);
        IPiece GetPiece(GridPosition position);
        bool IsValidPosition(int x, int y);
    }
}


