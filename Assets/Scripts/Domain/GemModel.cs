using Domain.Interfaces;

namespace Domain
{
    public class GemModel : IPiece
    {
        private readonly IGemData gemData;
        
        private bool isMatch;
        private bool isSwapMovement;
        private bool isMoving;
        private bool justSpawned;
        private GridPosition position;
        private GridPosition prevPosition;
        
        public GemType Type => gemData.Type;
        public bool IsColorBomb => gemData.IsColorBomb;
        public int BlastSize => gemData.BlastSize;
        public int ScoreValue => gemData.ScoreValue;
        public ref GridPosition Position => ref position;
        public ref GridPosition PrevPosition => ref prevPosition;
        public bool IsMatch
        {
            get => isMatch;
            set => isMatch = value;
        }
        public bool IsSwapMovement 
        {
            get => isSwapMovement;
            set => isSwapMovement = value;
        }
        public bool IsMoving
        {
            get => isMoving;
            set => isMoving = value;
        }

        public bool JustSpawned
        {
            get => justSpawned;
            set => justSpawned = value;
        }

        public GemModel(IGemData gemData, GridPosition position)
        {
            this.gemData = gemData;
            this.position = position;
            this.prevPosition = position;
            this.isMatch = false;
            this.isSwapMovement = false;
            this.isMoving = false;
            this.justSpawned = true;
        }
    }
}