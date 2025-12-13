using Domain.Interfaces;

namespace Service.Interfaces
{
    public interface IBoardView
    {
        void AddPieceView(IPieceView pieceView);
        IPieceView RemovePieceView(IPiece piece);
        IPieceView GetPieceView(IPiece piece);
        void CheckMisplacedPieces();
    }
}