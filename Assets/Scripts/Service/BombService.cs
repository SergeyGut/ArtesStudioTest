using System.Collections.Generic;
using Domain;
using Domain.Interfaces;
using Service.Interfaces;

namespace Service
{
    public class BombService : IBombService
    {
        private readonly ISpawnService spawnService;
        private readonly ISettings settings;

        public BombService(
            ISpawnService spawnService,
            ISettings settings)
        {
            this.spawnService = spawnService;
            this.settings = settings;
        }

        public void CreateBombs(Dictionary<GridPosition, PieceType> bombPositions)
        {
            foreach (var (pos, type) in bombPositions)
            {
                var gemData = GetBombPrefabForType(type);
                spawnService.SpawnGem(pos, gemData);
            }
        }

        private IPieceData GetBombPrefabForType(PieceType type)
        {
            foreach (var gemData in settings.GemBombs)
            {
                if (gemData.Type == type)
                {
                    return gemData;
                }
            }

            return null;
        }
    }
}
