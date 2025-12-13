using Domain.Interfaces;
using Service.Interfaces;
using UnityEngine;
using Zenject;
using IPoolable = Presentation.Pool.IPoolable;

namespace Presentation
{
    public class GemView : MonoBehaviour, IPoolable, IPieceView
    {
        private const float POSITION_THRESHOLD = 0.01f;
        
        [SerializeField]
        private GemInputHandler inputHandler;
        [SerializeField]
        private GameObject destroyEffect;

        private Vector2 startPosition;
        private Vector2 previousTargetPos;
        private float moveStartTime;
        private bool isStopMovingRequested;

        private IGameBoard gameBoard;
        private ISettings settings;
        private IPiece piece;

        public IPiece Piece => piece;
        public float SwapAngle => inputHandler != null ? inputHandler.SwapAngle : 0;
        public bool TargetPositionArrived => Vector2.Distance(transform.position, piece.Position.ToVector2()) < POSITION_THRESHOLD;

        [Inject]
        public void Construct(
            IGameBoard gameBoard,
            ISettings settings)
        {
            this.gameBoard = gameBoard;
            this.settings = settings;
        }

        public void Update()
        {
            piece.JustSpawned = false;

            Vector2 currentPos = transform.position;
            Vector2 targetPos = new Vector2(piece.Position.X, piece.Position.Y);
            float sqrDistance = (currentPos - targetPos).sqrMagnitude;

            if (piece.IsMoving && sqrDistance <= POSITION_THRESHOLD)
            {
                if (piece.Position.Y > 0 && gameBoard.GetGem(piece.Position.X, piece.Position.Y - 1) == null)
                {
                    gameBoard.SetGem(piece.Position.X, piece.Position.Y - 1, piece);
                    gameBoard.SetGem(piece.Position.X, piece.Position.Y, null);
                    piece.Position.Y--;

                    currentPos = transform.position;
                    targetPos = new Vector2(piece.Position.X, piece.Position.Y);
                    sqrDistance = (currentPos - targetPos).sqrMagnitude;
                }

                if (isStopMovingRequested)
                {
                    StopMoving();
                }
            }

            if (sqrDistance > POSITION_THRESHOLD)
            {
                if (!piece.IsMoving || previousTargetPos != targetPos)
                {
                    startPosition = currentPos;
                    moveStartTime = Time.time;
                    piece.IsMoving = true;
                    previousTargetPos = targetPos;
                }

                float sqrStartDistance = (startPosition - targetPos).sqrMagnitude;
                float distance = Mathf.Sqrt(sqrStartDistance);
                float elapsed = Time.time - moveStartTime;
                float speed = settings.GemSpeed;
                float t = Mathf.Clamp01((elapsed * speed) / Mathf.Max(distance, 0.1f));

                if (piece.IsSwapMovement)
                {
                    t = settings.GemSwapEase(t);
                }
                else
                {
                    var multiplier = settings.GemDropSpeedEase(distance / settings.RowsSize);
                    speed = settings.GemSpeed * multiplier * multiplier;
                    t = Mathf.Clamp01((elapsed * speed) / Mathf.Max(distance, 0.1f));
                }

                transform.position = Vector2.Lerp(startPosition, targetPos, t);
            }
            else if (piece.IsMoving)
            {
                RequestStopMoving();
            }
        }

        private void RequestStopMoving()
        {
            isStopMovingRequested = true;
        }

        private void StopMoving()
        {
            if (piece.IsMoving)
            {
                transform.position = new Vector3(piece.Position.X, piece.Position.Y, 0);
                gameBoard.SetGem(piece.Position, piece);
                piece.IsMoving = false;
                piece.IsSwapMovement = false;
                isStopMovingRequested = false;
                previousTargetPos = new Vector2(piece.Position.X, piece.Position.Y);
            }
        }

        public void SetupGem(IPiece piece)
        {
            this.piece = piece;
            
            piece.IsMoving = false;
            piece.JustSpawned = true;
            piece.IsSwapMovement = false;
            isStopMovingRequested = false;
            previousTargetPos = new Vector2(piece.Position.X, piece.Position.Y);
        }

        public void OnSpawnFromPool()
        {
            ResetState();
        }

        public void OnReturnToPool()
        {
            piece = null;
            
            // TODO: Stop all async operations
        }

        private void ResetState()
        {
            isStopMovingRequested = false;
            previousTargetPos = Vector2.zero;
            inputHandler.ResetState();
        }
        
        public void RunDestroyEffect()
        {
            Instantiate(destroyEffect, piece.Position.ToVector3(), Quaternion.identity);
        }
    }
}