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
    private Vector2 startPosition;
    private float moveStartTime;
    private bool isMoving = false;
    private bool isSwapMovement = false;

    private SC_GameVariables Settings
    {
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => SC_GameVariables.Instance;
    }
    
    void Update()
    {
        Vector2 currentPos = transform.position;
        Vector2 targetPos = new Vector2(posIndex.x, posIndex.y);
        float sqrDistance = (currentPos - targetPos).sqrMagnitude;
        
        if (sqrDistance > 0.0001f)
        {
            if (!isMoving)
            {
                startPosition = currentPos;
                moveStartTime = Time.time;
                isMoving = true;
            }

            float sqrStartDistance = (startPosition - targetPos).sqrMagnitude;
            float distance = Mathf.Sqrt(sqrStartDistance);
            float elapsed = Time.time - moveStartTime;
            float speed = Settings.gemSpeed;
            float t = Mathf.Clamp01((elapsed * speed) / Mathf.Max(distance, 0.1f));
            
            AnimationCurve curve = isSwapMovement 
                ? Settings.gemSwapEaseCurve 
                : Settings.gemEaseCurve;
            
            if (curve != null && curve.length > 0)
            {
                t = curve.Evaluate(t);
            }
            else
            {
                t = EaseInOutCustom(t);
            }
            
            transform.position = Vector2.Lerp(startPosition, targetPos, t);
        }
        else
        {
            transform.position = new Vector3(posIndex.x, posIndex.y, 0);
            scGameLogic.SetGem(posIndex.x, posIndex.y, this);
            isMoving = false;
            isSwapMovement = false;
        }
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

    private float EaseInOutCustom(float t)
    {
        if (t < 0.5f)
        {
            return 2f * t * t;
        }
        else
        {
            float f = 2f * t - 2f;
            return 1f - f * f * f * f * f;
        }
    }

    public void SetupGem(IGameLogic _ScGameLogic,Vector2Int _Position)
    {
        posIndex = _Position;
        scGameLogic = _ScGameLogic;
        isMoving = false;
        isSwapMovement = false;
    }

    public void OnSpawnFromPool()
    {
        isMatch = false;
        mousePressed = false;
        swapTriggered = false;
        otherGem = null;
        swipeAngle = 0;
        isMoving = false;
        isSwapMovement = false;
    }

    public void OnReturnToPool()
    {
        isMatch = false;
        mousePressed = false;
        swapTriggered = false;
        otherGem = null;
        swipeAngle = 0;
        previousPos = Vector2Int.zero;
        isMoving = false;
        isSwapMovement = false;
        StopAllCoroutines();
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
            otherGem = scGameLogic.GetGem(posIndex.x + 1, posIndex.y);
            otherGem.posIndex.x--;
            posIndex.x++;
            if (otherGem != null) otherGem.isSwapMovement = true;

        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && posIndex.y < Settings.colsSize - 1)
        {
            otherGem = scGameLogic.GetGem(posIndex.x, posIndex.y + 1);
            otherGem.posIndex.y--;
            posIndex.y++;
            if (otherGem != null) otherGem.isSwapMovement = true;
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && posIndex.y > 0)
        {
            otherGem = scGameLogic.GetGem(posIndex.x, posIndex.y - 1);
            otherGem.posIndex.y++;
            posIndex.y--;
            if (otherGem != null) otherGem.isSwapMovement = true;
        }
        else if ((swipeAngle > 135 || swipeAngle < -135) && posIndex.x > 0)
        {
            otherGem = scGameLogic.GetGem(posIndex.x - 1, posIndex.y);
            otherGem.posIndex.x++;
            posIndex.x--;
            if (otherGem != null) otherGem.isSwapMovement = true;
        }

        scGameLogic.SetGem(posIndex.x,posIndex.y, this);
        scGameLogic.SetGem(otherGem.posIndex.x, otherGem.posIndex.y, otherGem);

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

                scGameLogic.SetGem(posIndex.x, posIndex.y, this);
                scGameLogic.SetGem(otherGem.posIndex.x, otherGem.posIndex.y, otherGem);

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
