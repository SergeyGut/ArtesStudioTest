namespace Domain.Interfaces
{
    public interface IPieceView
    {
        IReadOnlyPiece Piece { get; }
        float SwapAngle { get; }
        bool TargetPositionArrived { get; }
        void RunDestroyEffect();
    }
}