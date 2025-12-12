using Domain;
using Domain.Interfaces;
using Service.Interfaces;
using UnityEngine;
using Zenject;
using IPoolable = Presentation.Pool.IPoolable;

namespace Presentation
{
    public class SC_Gem : MonoBehaviour, IPoolable, IPiece, IGemView
    {
        private const float POSITION_THRESHOLD = 0.01f;

        [HideInInspector] public GridPosition posIndex;

        private GemInputHandler inputHandler;

        public GemType type;
        public bool isColorBomb;
        public bool isMatch;
        private GridPosition previousPos;
        public GameObject destroyEffect;
        public int scoreValue = 10;
        public int blastSize = 1;

        private Vector2 startPosition;
        private Vector2 previousTargetPos;
        private float moveStartTime;
        private bool isMoving;
        private bool justSpawned;
        private bool isSwapMovement;
        private bool isStopMovingReqiested;

        private IGameBoard gameBoard;
        private ISettings settings;

        public GemType Type => type;

        public bool IsColorBomb => isColorBomb;
        public int BlastSize => blastSize;

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

        public int ScoreValue => scoreValue;
        public ref GridPosition Position => ref posIndex;
        public ref GridPosition PrevPosition => ref previousPos;
        public bool JustSpawned => justSpawned;
        public bool IsMoving => isMoving;

        private void Awake()
        {
            inputHandler = GetComponent<GemInputHandler>();
        }

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
            justSpawned = false;

            Vector2 currentPos = transform.position;
            Vector2 targetPos = new Vector2(posIndex.X, posIndex.Y);
            float sqrDistance = (currentPos - targetPos).sqrMagnitude;

            if (isMoving && sqrDistance <= POSITION_THRESHOLD)
            {
                if (posIndex.Y > 0 && gameBoard.GetGem(posIndex.X, posIndex.Y - 1) == null)
                {
                    gameBoard.SetGem(posIndex.X, posIndex.Y - 1, this);
                    gameBoard.SetGem(posIndex.X, posIndex.Y, null);
                    posIndex.Y--;

                    currentPos = transform.position;
                    targetPos = new Vector2(posIndex.X, posIndex.Y);
                    sqrDistance = (currentPos - targetPos).sqrMagnitude;
                }

                if (isStopMovingReqiested)
                {
                    StopMoving();
                }
            }

            if (sqrDistance > POSITION_THRESHOLD)
            {
                if (!isMoving || previousTargetPos != targetPos)
                {
                    startPosition = currentPos;
                    moveStartTime = Time.time;
                    isMoving = true;
                    previousTargetPos = targetPos;
                }

                float sqrStartDistance = (startPosition - targetPos).sqrMagnitude;
                float distance = Mathf.Sqrt(sqrStartDistance);
                float elapsed = Time.time - moveStartTime;
                float speed = settings.GemSpeed;
                float t = Mathf.Clamp01((elapsed * speed) / Mathf.Max(distance, 0.1f));

                if (isSwapMovement)
                {
                    t = settings.GemSwapEase(t);
                }
                else
                {
                    var multiplier = settings.GemDropSpeedEase(distance / gameBoard.Height);
                    speed = settings.GemSpeed * multiplier * multiplier;
                    t = Mathf.Clamp01((elapsed * speed) / Mathf.Max(distance, 0.1f));
                }

                transform.position = Vector2.Lerp(startPosition, targetPos, t);
            }
            else if (isMoving)
            {
                RequestStopMoving();
            }

        }

        private void RequestStopMoving()
        {
            isStopMovingReqiested = true;
        }

        private void StopMoving()
        {
            if (isMoving)
            {
                transform.position = new Vector3(posIndex.X, posIndex.Y, 0);
                gameBoard.SetGem(posIndex, this);
                isMoving = false;
                isSwapMovement = false;
                isStopMovingReqiested = false;
                previousTargetPos = new Vector2(posIndex.X, posIndex.Y);
            }
        }

        public void SetupGem(GridPosition position)
        {
            posIndex = position;

            isMoving = false;
            isSwapMovement = false;
            isStopMovingReqiested = false;
            previousTargetPos = new Vector2(posIndex.X, posIndex.Y);
            justSpawned = true;
        }

        public void OnSpawnFromPool()
        {
            ResetState();
        }

        public void OnReturnToPool()
        {
            // TODO: Stop all async operations
        }

        private void ResetState()
        {
            isMatch = false;
            previousPos = GridPosition.zero;
            isMoving = false;
            isSwapMovement = false;
            isStopMovingReqiested = false;
            previousTargetPos = Vector2.zero;
            
            if (inputHandler != null)
            {
                inputHandler.ResetState();
            }
        }
        
        public void RunDestroyEffect()
        {
            Instantiate(destroyEffect, posIndex.ToVector3(), Quaternion.identity);
        }

        public float SwapAngle => inputHandler != null ? inputHandler.SwapAngle : 0;

        public float TargetPositionDistance => Vector2.Distance(transform.position, posIndex.ToVector2());
    }
}