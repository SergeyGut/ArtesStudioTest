using System.Collections;
using UnityEngine;

public class SC_Gem : MonoBehaviour, IPoolable
{
    [HideInInspector]
    public Vector2Int posIndex;

    private Vector2 firstTouchPosition;
    private Vector2 currentTouchPosition;
    private bool mousePressed;
    private bool swapTriggered = false;
    private float swipeAngle = 0;
    private SC_Gem otherGem;

    public GlobalEnums.GemType type;
    public bool isColorBomb = false;
    public bool isMatch = false;
    private Vector2Int previousPos;
    public GameObject destroyEffect;
    public int scoreValue = 10;

    public int blastSize = 1;
    private IGameLogic scGameLogic;
    private IGameBoard scBoardService;
    private Vector2 startPosition;
    private Vector2 previousTargetPos;
    private float moveStartTime;
    private bool isMoving = false;
    private bool justSpawned;
    private bool isSwapMovement = false;
    private bool isStopMovingReqiested = false;

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
        Vector2 targetPos = new Vector2(posIndex.x, posIndex.y);
        float sqrDistance = (currentPos - targetPos).sqrMagnitude;
        
        if (isMoving && sqrDistance <= 0.01f)
        {
            if (posIndex.y > 0 && scBoardService.GetGem(posIndex.x, posIndex.y - 1) == null)
            {
                scBoardService.SetGem(posIndex.x, posIndex.y - 1, this);
                scBoardService.SetGem(posIndex.x, posIndex.y, null);
                posIndex.y--;
                
                currentPos = transform.position;
                targetPos = new Vector2(posIndex.x, posIndex.y);
                sqrDistance = (currentPos - targetPos).sqrMagnitude;
            }
            
            if (isStopMovingReqiested)
            {
                StopMoving();
            }
        }
        
        if (sqrDistance > 0.01f)
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
            
            AnimationCurve curve = isSwapMovement 
                ? Settings.gemSwapEaseCurve 
                : Settings.gemEaseCurve;
            
            t = curve.Evaluate(t);
            
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
            transform.position = new Vector3(posIndex.x, posIndex.y, 0);
            scBoardService.SetGem(posIndex, this);
            isMoving = false;
            isSwapMovement = false;
            isStopMovingReqiested = false;
            previousTargetPos = new Vector2(posIndex.x, posIndex.y);
        }
    }

    private void HandleInput()
    {
        if (mousePressed)
        {
            if (Input.GetMouseButton(0))
            {
                currentTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (!swapTriggered && scGameLogic.CurrentState == GlobalEnums.GameState.move)
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

    public void SetupGem(IGameLogic _ScGameLogic, IGameBoard _BoardService, Vector2Int _Position)
    {
        posIndex = _Position;
        scGameLogic = _ScGameLogic;
        scBoardService = _BoardService;
        isMoving = false;
        isSwapMovement = false;
        isStopMovingReqiested = false;
        previousTargetPos = new Vector2(posIndex.x, posIndex.y);
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
        previousPos = Vector2Int.zero;
        isMoving = false;
        isSwapMovement = false;
        isStopMovingReqiested = false;
        previousTargetPos = Vector2.zero;
    }

    private void OnMouseDown()
    {
        if (scGameLogic.CurrentState == GlobalEnums.GameState.move)
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

        if (swipeAngle < 45 && swipeAngle > -45 && posIndex.x < Settings.rowsSize - 1)
        {
            otherGem = scBoardService.GetGem(posIndex.x + 1, posIndex.y);
            otherGem.posIndex.x--;
            posIndex.x++;
            if (otherGem != null) otherGem.isSwapMovement = true;

        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && posIndex.y < Settings.colsSize - 1)
        {
            otherGem = scBoardService.GetGem(posIndex.x, posIndex.y + 1);
            otherGem.posIndex.y--;
            posIndex.y++;
            if (otherGem != null) otherGem.isSwapMovement = true;
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && posIndex.y > 0)
        {
            otherGem = scBoardService.GetGem(posIndex.x, posIndex.y - 1);
            otherGem.posIndex.y++;
            posIndex.y--;
            if (otherGem != null) otherGem.isSwapMovement = true;
        }
        else if ((swipeAngle > 135 || swipeAngle < -135) && posIndex.x > 0)
        {
            otherGem = scBoardService.GetGem(posIndex.x - 1, posIndex.y);
            otherGem.posIndex.x++;
            posIndex.x--;
            if (otherGem != null) otherGem.isSwapMovement = true;
        }

        scBoardService.SetGem(posIndex, this);
        scBoardService.SetGem(otherGem.posIndex, otherGem);

        StartCoroutine(CheckMoveCo());
    }

    public IEnumerator CheckMoveCo()
    {
        scGameLogic.SetState(GlobalEnums.GameState.wait);

        yield return WaitForSwapCompletion();
        scGameLogic.FindAllMatches(posIndex, otherGem.posIndex);

        if (otherGem != null)
        {
            if (isMatch == false && otherGem.isMatch == false)
            {
                otherGem.posIndex = posIndex;
                posIndex = previousPos;
                isSwapMovement = true;
                otherGem.isSwapMovement = true;

                scBoardService.SetGem(posIndex, this);
                scBoardService.SetGem(otherGem.posIndex, otherGem);

                yield return WaitForSwapCompletion();
                scGameLogic.SetState(GlobalEnums.GameState.move);
            }
            else
            {
                scGameLogic.DestroyMatches();
            }
        }
    }

    private IEnumerator WaitForSwapCompletion()
    {
        while (Vector2.Distance(transform.position, posIndex) > 0.01f || 
               (otherGem != null && Vector2.Distance(otherGem.transform.position, otherGem.posIndex) > 0.01f))
        {
            yield return null;
        }
    }
}
