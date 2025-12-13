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
                var pieceData = GetBombPrefabForType(type);
                spawnService.SpawnPiece(pos, pieceData);
            }
        }

        private IPieceData GetBombPrefabForType(PieceType type)
        {
            foreach (var pieceData in settings.PieceBombs)
            {
                if (pieceData.Type == type)
                {
                    return pieceData;
                }
            }

            return null;
        }
    }
}
