using Domain.Interfaces;

namespace Service.Interfaces
{
    public interface IPiecePool<T> where T : IPieceView
    {
        int AvailableCount { get; }
        int ActiveCount { get; }

        T SpawnPiece(IPieceView item, IPiece piece);
        void ReturnPiece(T item);
        void ClearPool();
    }
}