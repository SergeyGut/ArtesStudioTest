using Domain.Interfaces;

namespace Domain
{
    public class GemModel : IPiece
    {
        private readonly IGemData gemData;

        private GridPosition position;
        
        public GemType Type => gemData.Type;
        public bool IsColorBomb => gemData.IsColorBomb;
        public int BlastSize => gemData.BlastSize;
        public int ScoreValue => gemData.ScoreValue;
        public ref GridPosition Position => ref position;
        public GridPosition PrevPosition { get; set; }
        public bool IsMatch { get; set; }
        public bool IsSwap { get; set; }
        public bool IsMoving { get; set; }
        GridPosition IReadOnlyPiece.Position => position;

        public GemModel(IGemData gemData, GridPosition position)
        {
            this.gemData = gemData;
            this.position = position;
            this.PrevPosition = position;
            this.IsMatch = false;
            this.IsSwap = false;
            this.IsMoving = false;
        }
    }
}