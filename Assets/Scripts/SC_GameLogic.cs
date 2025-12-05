using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class SC_GameLogic : MonoBehaviour
{
    private Dictionary<string, GameObject> unityObjects;
    private int score = 0;
    private float displayScore = 0;
    private GameBoard gameBoard;
    private GlobalEnums.GameState currentState = GlobalEnums.GameState.move;
    public GlobalEnums.GameState CurrentState { get { return currentState; } }
    private GemPool gemPool;

    #region MonoBehaviour
    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        displayScore = Mathf.Lerp(displayScore, gameBoard.Score, SC_GameVariables.Instance.scoreSpeed * Time.deltaTime);
        unityObjects["Txt_Score"].GetComponent<TMPro.TextMeshProUGUI>().text = displayScore.ToString("0");
    }
    #endregion

    #region Logic
    private void Init()
    {
        unityObjects = new Dictionary<string, GameObject>();
        GameObject[] _obj = GameObject.FindGameObjectsWithTag("UnityObject");
        foreach (GameObject g in _obj)
            unityObjects.Add(g.name,g);

        gameBoard = new GameBoard(7, 7);
        gemPool = new GemPool(unityObjects["GemsHolder"].transform);
        Setup();
    }
    private void Setup()
    {
        for (int x = 0; x < gameBoard.Width; x++)
            for (int y = 0; y < gameBoard.Height; y++)
            {
                Vector2 _pos = new Vector2(x, y);
                GameObject _bgTile = Instantiate(SC_GameVariables.Instance.bgTilePrefabs, _pos, Quaternion.identity);
                _bgTile.transform.SetParent(unityObjects["GemsHolder"].transform);
                _bgTile.name = "BG Tile - " + x + ", " + y;

                SC_Gem gemToSpawn = SelectNonMatchingGem(new Vector2Int(x, y));
                SpawnGem(new Vector2Int(x, y), gemToSpawn);
            }
    }
    public void StartGame()
    {
        unityObjects["Txt_Score"].GetComponent<TextMeshProUGUI>().text = score.ToString("0");
    }
    private SC_Gem SelectNonMatchingGem(Vector2Int _Position)
    {
        int gemToUse = Random.Range(0, SC_GameVariables.Instance.gems.Length);
        int iterations = 0;
        int maxIterations = 100;

        while (gameBoard.MatchesAt(_Position, SC_GameVariables.Instance.gems[gemToUse]) && iterations < maxIterations)
        {
            gemToUse = Random.Range(0, SC_GameVariables.Instance.gems.Length);
            iterations++;
        }

        return SC_GameVariables.Instance.gems[gemToUse];
    }

    private void SpawnGem(Vector2Int _Position, SC_Gem _GemToSpawn)
    {
        if (Random.Range(0, 100f) < SC_GameVariables.Instance.bombChance)
            _GemToSpawn = SC_GameVariables.Instance.bomb;

        SC_Gem _gem = gemPool.SpawnGem(_GemToSpawn, _Position, this, SC_GameVariables.Instance.dropHeight);
        gameBoard.SetGem(_Position.x,_Position.y, _gem);
    }
    public void SetGem(int _X,int _Y, SC_Gem _Gem)
    {
        gameBoard.SetGem(_X,_Y, _Gem);
    }
    public SC_Gem GetGem(int _X, int _Y)
    {
        return gameBoard.GetGem(_X, _Y);
    }
    public void SetState(GlobalEnums.GameState _CurrentState)
    {
        currentState = _CurrentState;
    }
    public void DestroyMatches()
    {
        StartCoroutine(DestroyMatchesCo());
    }

    private IEnumerator DestroyMatchesCo()
    {
        Dictionary<Vector2Int, GlobalEnums.GemType> bombCreationPositions = new Dictionary<Vector2Int, GlobalEnums.GemType>();

        foreach (var matchInfo in gameBoard.MatchInfoMap)
        {
            if (matchInfo.matchedGems.Count >= SC_GameVariables.Instance.minMatchForBomb)
            {
                var firstGem = matchInfo.matchedGems.First();
                bombCreationPositions.TryAdd(matchInfo.userActionPos?? firstGem.posIndex, firstGem.type);
            }
        }
        
        foreach (var matchInfo in gameBoard.MatchInfoMap)
        foreach (var gem in matchInfo.matchedGems)
        {
            if (gem && !gem.isColorBomb && gem.type != GlobalEnums.GemType.bomb)
            {
                ScoreCheck(gem);
                DestroyMatchedGemsAt(gem.posIndex);
            }
        }
        
        foreach (var bombCreationPosition in bombCreationPositions)
        {
            var bombToSpawn = GetBombPrefabForType(bombCreationPosition.Value);
            var bombPos = bombCreationPosition.Key;
            
            SC_Gem newBomb = gemPool.SpawnGem(bombToSpawn, bombPos, this, 0);
            newBomb.transform.position = new Vector3(bombPos.x, bombPos.y, 0);
            gameBoard.SetGem(bombPos.x, bombPos.y, newBomb);
        }
        
        yield return new WaitForSeconds(SC_GameVariables.Instance.bombNeighborDelay);

        foreach (var gem in gameBoard.CurrentMatches)
        {
            if (gem && !gem.isColorBomb && gem.type != GlobalEnums.GemType.bomb)
            {
                ScoreCheck(gem);
                DestroyMatchedGemsAt(gem.posIndex);
            }
        }
        
        yield return new WaitForSeconds(SC_GameVariables.Instance.bombSelfDelay);

        foreach (var gem in gameBoard.CurrentMatches)
        {
            if (gem && (gem.isColorBomb || gem.type == GlobalEnums.GemType.bomb))
            {
                ScoreCheck(gem);
                DestroyMatchedGemsAt(gem.posIndex);
            }
        }
        
        yield return DecreaseRowCo();
    }

    private SC_Gem GetBombPrefabForType(GlobalEnums.GemType type)
    {
        foreach (SC_Gem bomb in SC_GameVariables.Instance.gemBombs)
        {
            if (bomb.type == type)
                return bomb;
        }
        return null;
    }
    private IEnumerator DecreaseRowCo()
    {
        yield return new WaitForSeconds(.2f);

        bool hasActivity = true;
        while (hasActivity)
        {
            SpawnTopRow();
            hasActivity = DropSingleRow();

            if (hasActivity)
            {
                yield return new WaitForSeconds(0.05f);
            }
        }

        CheckMisplacedGems();
        yield return new WaitForSeconds(0.5f);
        gameBoard.FindAllMatches();
        if (gameBoard.CurrentMatches.Count > 0)
        {
            yield return new WaitForSeconds(0.5f);
            DestroyMatches();
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            currentState = GlobalEnums.GameState.move;
        }
    }

    private void SpawnTopRow()
    {
        for (int x = 0; x < gameBoard.Width; x++)
        {
            int topY = gameBoard.Height - 1;
            SC_Gem topGem = gameBoard.GetGem(x, topY);
            
            if (topGem == null)
            {
                SC_Gem gemToSpawn = SelectNonMatchingGem(new Vector2Int(x, topY));
                SpawnGem(new Vector2Int(x, topY), gemToSpawn);
            }
        }
    }

    private bool DropSingleRow()
    {
        bool anyDropped = false;

        for (int y = 1; y < gameBoard.Height; y++)
        {
            for (int x = 0; x < gameBoard.Width; x++)
            {
                SC_Gem currentGem = gameBoard.GetGem(x, y);
                SC_Gem gemBelow = gameBoard.GetGem(x, y - 1);

                if (currentGem != null && gemBelow == null)
                {
                    currentGem.posIndex.y--;
                    SetGem(x, y - 1, currentGem);
                    SetGem(x, y, null);
                    anyDropped = true;
                }
            }

            if (anyDropped)
            {
                return true;
            }
        }

        return false;
    }

    public void ScoreCheck(SC_Gem gemToCheck)
    {
        gameBoard.Score += gemToCheck.scoreValue;
    }
    private void DestroyMatchedGemsAt(Vector2Int _Pos)
    {
        SC_Gem _curGem = gameBoard.GetGem(_Pos.x,_Pos.y);
        if (_curGem != null)
        {
            Instantiate(_curGem.destroyEffect, new Vector2(_Pos.x, _Pos.y), Quaternion.identity);

            gemPool.ReturnGem(_curGem);
            SetGem(_Pos.x,_Pos.y, null);
        }
    }

    private void CheckMisplacedGems()
    {
        List<SC_Gem> foundGems = new List<SC_Gem>();
        foundGems.AddRange(FindObjectsOfType<SC_Gem>());
        for (int x = 0; x < gameBoard.Width; x++)
        {
            for (int y = 0; y < gameBoard.Height; y++)
            {
                SC_Gem _curGem = gameBoard.GetGem(x, y);
                if (foundGems.Contains(_curGem))
                    foundGems.Remove(_curGem);
            }
        }

        foreach (SC_Gem g in foundGems)
            gemPool.ReturnGem(g);
    }
    public void FindAllMatches(Vector2Int? posIndex = null, Vector2Int? otherPosIndex = null)
    {
        if (posIndex.HasValue)
            gameBoard.FindAllMatches(posIndex);
        else if (otherPosIndex.HasValue)
            gameBoard.FindAllMatches(otherPosIndex);
        else
            gameBoard.FindAllMatches();
    }

    #endregion
}
