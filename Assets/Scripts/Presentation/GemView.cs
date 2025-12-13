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
        private float moveStartTime;
        private bool wasMovedLastFrame;

        private ISettings settings;
        private IReadOnlyPiece piece;

        public IReadOnlyPiece Piece => piece;
        public float SwapAngle => inputHandler != null ? inputHandler.SwapAngle : 0;
        public bool TargetPositionArrived => Vector2.Distance(transform.position, piece.Position.ToVector2()) < POSITION_THRESHOLD;

        [Inject]
        public void Construct(ISettings settings)
        {
            this.settings = settings;
        }

        public void Update()
        {
            if (piece.IsMoving || piece.IsSwap)
            {
                Vector2 targetPos = piece.Position.ToVector2();
                float sqrStartDistance = (startPosition - targetPos).sqrMagnitude;
                float distance = Mathf.Sqrt(sqrStartDistance);
                float elapsed = Time.time - moveStartTime;
                float speed = settings.GemSpeed;
                float t = Mathf.Clamp01(elapsed * speed / Mathf.Max(distance, POSITION_THRESHOLD));

                if (piece.IsSwap)
                {
                    t = settings.GemSwapEase(t);
                }

                transform.position = Vector2.Lerp(startPosition, targetPos, t);
                
                wasMovedLastFrame = true;
            }
            else
            {
                if (wasMovedLastFrame)
                {
                    transform.position = startPosition = piece.Position.ToVector2();
                }
                
                moveStartTime = Time.time;
                wasMovedLastFrame = false;
            }
        }

        public void SetupGem(IReadOnlyPiece piece)
        {
            this.piece = piece;
            
            startPosition = transform.position;
            moveStartTime = Time.time;
            wasMovedLastFrame = false;
        }

        public void OnSpawnFromPool()
        {
            inputHandler.ResetState();
        }

        public void OnReturnToPool()
        {
            piece = null;
        }
        
        public void RunDestroyEffect()
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }
    }
}