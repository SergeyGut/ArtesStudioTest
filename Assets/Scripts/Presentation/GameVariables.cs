using System.Collections.Generic;
using Domain.Interfaces;
using Service.Interfaces;
using UnityEngine;

namespace Presentation
{
    [CreateAssetMenu(fileName = "GameVariables", menuName = "Game/GameVariables")]
    public class GameVariables : ScriptableObject, ISettings
    {
        public GameObject bgTilePrefabs;
        public SC_Gem bomb;
        public SC_Gem[] gems;
        public SC_Gem[] gemBombs;
        public float bombChance = 3f;
        public int dropHeight = 1;
        public float gemSpeed = 12;
        public AnimationCurve gemSwapEaseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public AnimationCurve gemDropSpeedCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        public float scoreSpeed = 5;
        public float bombNeighborDelay = 0.3f;
        public float bombSelfDelay = 0.2f;
        public float bombPostSelfDelay = 0.1f;
        public float decreaseRowDelay = 0.2f;
        public float decreaseSingleRowDelay = 0.05f;
        public float decreaseSingleColumnDelay = 0.03f;
        public float findAllMatchesDelay = 0.2f;
        public float destroyMatchesDelay = 0.1f;
        public float changeStateDelay = 0f;
        public int minMatchForBomb = 4;

        [HideInInspector] public int rowsSize = 7;
        [HideInInspector] public int colsSize = 7;

        public IReadOnlyList<IPiece> Gems => gems;
        public float BombChance => bombChance;
        public int DropHeight => dropHeight;
        public IPiece Bomb => bomb;
        public int MinMatchForBomb => minMatchForBomb;
        public float ScoreSpeed => scoreSpeed;
        public float BombNeighborDelay => bombNeighborDelay;
        public float BombSelfDelay => bombSelfDelay;
        public float BombPostSelfDelay => bombPostSelfDelay;
        public float DecreaseRowDelay => decreaseRowDelay;
        public float DecreaseSingleRowDelay => decreaseSingleRowDelay;
        public float DecreaseSingleColumnDelay => decreaseSingleColumnDelay;
        public float FindAllMatchesDelay => findAllMatchesDelay;
        public float DestroyMatchesDelay => destroyMatchesDelay;
        public float ChangeStateDelay => changeStateDelay;
        public IReadOnlyList<IPiece> GemBombs => gemBombs;
        public object TilePrefabs => bgTilePrefabs;
        public float GemSpeed => gemSpeed;
        public int RowsSize => rowsSize;
        public int ColsSize => colsSize;

        public float GemSwapEase(float t)
        {
            return gemSwapEaseCurve.Evaluate(t);
        }

        public float GemDropSpeedEase(float t)
        {
            return gemDropSpeedCurve.Evaluate(t);
        }
    }
}