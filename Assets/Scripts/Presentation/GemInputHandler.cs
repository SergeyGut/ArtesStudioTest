using Domain.Interfaces;
using Service.Interfaces;
using UnityEngine;
using Zenject;

namespace Presentation
{
    public class GemInputHandler : MonoBehaviour
    {
        private Vector2 firstTouchPosition;
        private Vector2 currentTouchPosition;
        private bool mousePressed;
        private bool swapTriggered;
        private float swipeAngle;

        private SC_Gem gem;
        private IGameStateProvider gameStateProvider;
        private ISwapService swapService;

        public float SwapAngle => swipeAngle;

        [Inject]
        public void Construct(
            IGameStateProvider gameStateProvider,
            ISwapService swapService)
        {
            this.gameStateProvider = gameStateProvider;
            this.swapService = swapService;
        }

        private void Awake()
        {
            gem = GetComponent<SC_Gem>();
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            if (mousePressed)
            {
                if (Input.GetMouseButton(0))
                {
                    currentTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    if (!swapTriggered && gameStateProvider.CurrentState == GameState.move)
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

        private void OnMouseDown()
        {
            if (gameStateProvider.CurrentState == GameState.move)
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
                swapService.MovePieces(gem);
                swapTriggered = true;
            }
        }

        public void ResetState()
        {
            mousePressed = false;
            swapTriggered = false;
            swipeAngle = 0;
        }
    }
}

