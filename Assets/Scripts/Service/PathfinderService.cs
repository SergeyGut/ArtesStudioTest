using Domain;
using Domain.Interfaces;
using Domain.Pool;
using Service.Interfaces;

namespace Service
{
    public class PathfinderService : IPathfinderService
    {
        private readonly IMatchService matchService;
        private readonly ISettings settings;

        public PathfinderService(
            IMatchService matchService,
            ISettings settings)
        {
            this.matchService = matchService;
            this.settings = settings;
        }

        public PooledDictionary<GridPosition, PieceType> CollectBombCreationPositions()
        {
            PooledDictionary<GridPosition, PieceType> bombCreationPositions = PooledDictionary<GridPosition, PieceType>.Get();

            foreach (MatchInfo matchInfo in matchService.MatchInfoMap)
            {
                if (matchInfo.MatchedPieces.Count < settings.MinMatchForBomb)
                {
                    continue;
                }
                
                IPiece firstPiece = null;
                foreach (IPiece piece in matchInfo.MatchedPieces)
                {
                    firstPiece = piece;
                    break;
                }

                if (firstPiece != null)
                {
                    bombCreationPositions.Value.TryAdd(matchInfo.UserActionPos ?? firstPiece.Position, firstPiece.Type);
                }
            }

            return bombCreationPositions;
        }

        public PooledHashSet<IPiece> CollectMatchedPieces()
        {
            PooledHashSet<IPiece> matchedPieces = PooledHashSet<IPiece>.Get();
            
            foreach (MatchInfo matchInfo in matchService.MatchInfoMap)
            {
                foreach (IPiece piece in matchInfo.MatchedPieces)
                {
                    if (piece is { IsColorBomb: false } && piece.Type != PieceType.bomb)
                    {
                        matchedPieces.Value.Add(piece);
                    }
                }
            }

            return matchedPieces;
        }

        public PooledList<IPiece> CollectExplosionsNonBomb(PooledHashSet<IPiece> matchedPieces)
        {
            PooledList<IPiece> nonBombExplosions = PooledList<IPiece>.Get();
            
            foreach (IPiece piece in matchService.Explosions)
            {
                if (piece is { IsColorBomb: false } && piece.Type != PieceType.bomb && !matchedPieces.Value.Contains(piece))
                {
                    nonBombExplosions.Value.Add(piece);
                }
            }

            return nonBombExplosions;
        }

        public PooledList<IPiece> CollectExplosionsBomb(PooledHashSet<IPiece> matchedPieces)
        {
            PooledList<IPiece> bombExplosions = PooledList<IPiece>.Get();
            
            foreach (IPiece piece in matchService.Explosions)
            {
                if (piece != null && (piece.IsColorBomb || piece.Type == PieceType.bomb) && !matchedPieces.Value.Contains(piece))
                {
                    bombExplosions.Value.Add(piece);
                }
            }

            return bombExplosions;
        }
    }
}
