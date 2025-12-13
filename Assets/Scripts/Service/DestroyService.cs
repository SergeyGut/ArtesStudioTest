using System.Collections.Generic;
using Domain.Interfaces;
using Service.Interfaces;

namespace Service
{
    public class DestroyService : IDestroyService
    {
        private readonly IGameBoard gameBoard;
        private readonly IBoardView boardView;
        private readonly IGemPool<IPieceView> gemPool;
        private readonly IScoreService scoreService;

        public DestroyService(
            IGameBoard gameBoard,
            IBoardView boardView,
            IGemPool<IPieceView> gemPool,
            IScoreService scoreService)
        {
            this.gameBoard = gameBoard;
            this.boardView = boardView;
            this.gemPool = gemPool;
            this.scoreService = scoreService;
        }

        public void DestroyMatchedGems(IEnumerable<IPiece> gems)
        {
            foreach (var gem in gems)
            {
                if (gem == null) continue;
                
                scoreService.AddScore(gem.ScoreValue);
                DestroyGem(gem);
            }
        }

        private void DestroyGem(IPiece piece)
        {
            if (piece == null) return;
            
            var pieceView = boardView.RemovePieceView(piece);
            pieceView.RunDestroyEffect();
            gemPool.ReturnGem(pieceView);

            if (gameBoard.GetGem(piece.Position) == piece)
            {
                gameBoard.SetGem(piece.Position, null);
            }
        }
    }
}
