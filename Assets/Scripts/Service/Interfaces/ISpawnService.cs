using Domain;
using Domain.Interfaces;

namespace Service.Interfaces
{
    public interface ISpawnService
    {
        void SpawnGem(GridPosition position, IGemData gemToSpawn, int dropHeight = 0);
        void SpawnTopX(int x);
    }
}