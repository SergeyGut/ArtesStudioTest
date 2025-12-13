namespace Domain.Interfaces
{
    public interface IPieceView
    {
        IPiece Piece { get; }
        float SwapAngle { get; }
        bool TargetPositionArrived { get; }
        void RunDestroyEffect();
    }
}