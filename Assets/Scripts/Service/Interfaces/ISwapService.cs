using Domain.Interfaces;

namespace Service.Interfaces
{
    public interface ISwapService
    {
        void MovePieces(IPieceView pieceView);
    }
}
