using Domain.Interfaces;

namespace Service.Interfaces
{
    public interface IGemPool<T> where T : IPieceView
    {
        int AvailableCount { get; }
        int ActiveCount { get; }

        T SpawnGem(IPieceView item, IPiece piece, float dropHeight = 0f);
        void ReturnGem(T item);
        void ClearPool();
    }
}