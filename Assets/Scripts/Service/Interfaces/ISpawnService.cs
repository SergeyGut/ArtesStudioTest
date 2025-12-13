using Domain;
using Domain.Interfaces;

namespace Service.Interfaces
{
    public interface ISpawnService
    {
        IPiece SpawnGem(GridPosition position, IGemData gemToSpawn);
        void SpawnTopX(int x);
    }
}