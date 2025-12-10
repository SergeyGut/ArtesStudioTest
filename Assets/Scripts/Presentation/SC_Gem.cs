using Cysharp.Threading.Tasks;
using UnityEngine;

public class SC_Gem : MonoBehaviour, IPoolable, IPiece
{
    [HideInInspector]
    public GridPosition posIndex;

    private Vector2 firstTouchPosition;
    private Vector2 currentTouchPosition;
    private bool mousePressed;
    private bool swapTriggered = false;
    private float swipeAngle = 0;
    private IPiece otherGem;

    public GemType type;
    public bool isColorBomb = false;
    public bool isMatch = false;
    private GridPosition previousPos;
    public GameObject destroyEffect;
    public int scoreValue = 10;

    public int blastSize = 1;
    private IGameLogic scGameLogic;
    private IGameBoard gameBoard;
    private Vector2 startPosition;
    private Vector2 previousTargetPos;
    private float moveStartTime;
    private bool isMoving = false;
    private bool justSpawned;
    private bool isSwapMovement = false;
    private bool isStopMovingReqiested = false;
    private const float POSITION_THRESHOLD = 0.01f;

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
    public bool JustSpawned => justSpawned;
    public bool IsMoving => isMoving;
    
    private SC_GameVariables Settings
    {
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => SC_GameVariables.Instance;
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
            float speed = Settings.gemSpeed;
            float t = Mathf.Clamp01((elapsed * speed) / Mathf.Max(distance, 0.1f));
            
            if (isSwapMovement)
            {
                t = Settings.gemSwapEaseCurve.Evaluate(t);
            }
            else
            {
                var multiplier = Settings.gemDropSpeedCurve.Evaluate(distance / gameBoard.Height);
                speed = Settings.gemSpeed * multiplier * multiplier;
                t = Mathf.Clamp01((elapsed * speed) / Mathf.Max(distance, 0.1f));
            }
            
            transform.position = Vector2.Lerp(startPosition, targetPos, t);
        }
        else if (isMoving)
        {
            RequestStopMoving();
        }

        HandleInput();
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

    private void HandleInput()
    {
        if (mousePressed)
        {
            if (Input.GetMouseButton(0))
            {
                currentTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (!swapTriggered && scGameLogic.CurrentState == GameState.move)
                {
                    CheckForBorderCross();
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                mousePressed = false;
                swapTriggered = false;
            }
        }
    }

    public void SetupGem(IGameLogic _ScGameLogic, IGameBoard _BoardService, GridPosition _Position)
    {
        posIndex = _Position;
        scGameLogic = _ScGameLogic;
        gameBoard = _BoardService;
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
        ResetState();
        StopAllCoroutines();
    }

    private void ResetState()
    {
        isMatch = false;
        mousePressed = false;
        swapTriggered = false;
        otherGem = null;
        swipeAngle = 0;
        previousPos = GridPosition.zero;
        isMoving = false;
        isSwapMovement = false;
        isStopMovingReqiested = false;
        previousTargetPos = Vector2.zero;
    }

    private void OnMouseDown()
    {
        if (scGameLogic.CurrentState == GameState.move)
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentTouchPosition = firstTouchPosition;
            mousePressed = true;
            swapTriggered = false;
        }
    }

    private void CheckForBorderCross()
    {
        Vector2 delta = currentTouchPosition - firstTouchPosition;
        float distance = delta.magnitude;
        
        if (distance > 0.5f)
        {
            swipeAngle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            MovePieces();
            swapTriggered = true;
        }
    }

    private void MovePieces()
    {
        previousPos = posIndex;
        isSwapMovement = true;

        if (swipeAngle < 45 && swipeAngle > -45 && posIndex.X < Settings.rowsSize - 1)
        {
            otherGem = gameBoard.GetGem(posIndex.X + 1, posIndex.Y);
            otherGem.Position.X--;
            posIndex.X++;
            if (otherGem != null) otherGem.IsSwapMovement = true;
        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && posIndex.Y < Settings.colsSize - 1)
        {
            otherGem = gameBoard.GetGem(posIndex.X, posIndex.Y + 1);
            otherGem.Position.Y--;
            posIndex.Y++;
            if (otherGem != null) otherGem.IsSwapMovement = true;
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && posIndex.Y > 0)
        {
            otherGem = gameBoard.GetGem(posIndex.X, posIndex.Y - 1);
            otherGem.Position.Y++;
            posIndex.Y--;
            if (otherGem != null) otherGem.IsSwapMovement = true;
        }
        else if ((swipeAngle > 135 || swipeAngle < -135) && posIndex.X > 0)
        {
            otherGem = gameBoard.GetGem(posIndex.X - 1, posIndex.Y);
            otherGem.Position.X++;
            posIndex.X--;
            if (otherGem != null) otherGem.IsSwapMovement = true;
        }

        gameBoard.SetGem(posIndex, this);
        gameBoard.SetGem(otherGem.Position, otherGem);

        CheckMoveCo().Forget();
    }

    public async UniTask CheckMoveCo()
    {
        scGameLogic.SetState(GameState.wait);

        await WaitForSwapCompletion();
        scGameLogic.FindAllMatches(posIndex, otherGem.Position);

        if (otherGem != null)
        {
            if (isMatch == false && otherGem.IsMatch == false)
            {
                otherGem.Position = posIndex;
                posIndex = previousPos;
                isSwapMovement = true;
                otherGem.IsSwapMovement = true;

                gameBoard.SetGem(posIndex, this);
                gameBoard.SetGem(otherGem.Position, otherGem);

                await WaitForSwapCompletion();
                scGameLogic.SetState(GameState.move);
            }
            else
            {
                scGameLogic.DestroyMatches();
            }
        }
    }

    private async UniTask WaitForSwapCompletion()
    {
        var other = otherGem as SC_Gem;
        
        while (Vector2.Distance(transform.position, posIndex.ToVector2()) > POSITION_THRESHOLD || 
               (otherGem != null && Vector2.Distance(other.transform.position, otherGem.Position.ToVector2()) > POSITION_THRESHOLD))
        {
            await UniTask.Yield();
        }
    }
    
    public void RunDestroyEffect()
    {
        Instantiate(destroyEffect, posIndex.ToVector3(), Quaternion.identity);
    }
}
