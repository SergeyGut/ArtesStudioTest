using Domain;
using Domain.Interfaces;

namespace Service.Interfaces
{
    public interface ISpawnService
    {
        IGemData SelectNonMatchingGem(GridPosition position);
        void SpawnGem(GridPosition position, IGemData gemToSpawn);
        void SpawnTopX(int x);
    }
}