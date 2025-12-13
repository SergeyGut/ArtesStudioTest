using System.Collections.Generic;
using Domain.Interfaces;
using Service.Interfaces;

namespace Service
{
    public class DestroyService : IDestroyService
    {
        private readonly IGameBoard gameBoard;
        private readonly IBoardView boardView;
        private readonly IPiecePool<IPieceView> piecePool;
        private readonly IScoreService scoreService;

        public DestroyService(
            IGameBoard gameBoard,
            IBoardView boardView,
            IPiecePool<IPieceView> piecePool,
            IScoreService scoreService)
        {
            this.gameBoard = gameBoard;
            this.boardView = boardView;
            this.piecePool = piecePool;
            this.scoreService = scoreService;
        }

        public void DestroyMatchedPieces(IEnumerable<IPiece> pieces)
        {
            foreach (var piece in pieces)
            {
                if (piece == null) continue;
                
                scoreService.AddScore(piece.ScoreValue);
                DestroyPiece(piece);
            }
        }

        private void DestroyPiece(IPiece piece)
        {
            if (piece == null) return;
            
            var pieceView = boardView.RemovePieceView(piece);
            pieceView.RunDestroyEffect();
            piecePool.ReturnPiece(pieceView);

            if (gameBoard.GetPiece(piece.Position) == piece)
            {
                gameBoard.SetPiece(piece.Position, null);
            }
            
            piece.Dispose();
        }
    }
}
