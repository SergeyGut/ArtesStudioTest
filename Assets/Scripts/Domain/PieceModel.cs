using System.Threading;
using Domain.Interfaces;

namespace Domain
{
    public class PieceModel : IPiece
    {
        private readonly IPieceData pieceData;

        private GridPosition position;
        private CancellationTokenSource cancellationTokenSource = new();
        
        public PieceType Type => pieceData.Type;
        public bool IsColorBomb => pieceData.IsColorBomb;
        public int BlastSize => pieceData.BlastSize;
        public int ScoreValue => pieceData.ScoreValue;
        public ref GridPosition Position => ref position;
        public GridPosition PrevPosition { get; set; }
        public bool IsMatch { get; set; }
        public bool IsSwap { get; set; }
        public bool IsMoving { get; set; }
        public CancellationToken Token => cancellationTokenSource.Token;
        GridPosition IReadOnlyPiece.Position => position;

        public PieceModel(IPieceData pieceData, GridPosition position)
        {
            this.pieceData = pieceData;
            this.position = position;
            
            PrevPosition = position;
            IsMatch = false;
            IsSwap = false;
            IsMoving = false;
        }


        public void Dispose()
        {
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
        }
    }
}