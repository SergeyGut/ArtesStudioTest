using Domain.Interfaces;

namespace Domain
{
    public class GemModel : IPiece
    {
        private IGemData gemData;
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
        public bool IsMoving => isMoving;
        public bool JustSpawned => justSpawned;
        
        public void RunDestroyEffect()
        {
            throw new System.NotImplementedException();
        }
    }
}