using Domain;
using Domain.Interfaces;

namespace Service.Interfaces
{
    public interface ISpawnService
    {
        void SpawnPiece(GridPosition position, IPieceData pieceToSpawn, int dropHeight = 0);
        void SpawnTopX(int x);
    }
}