using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        var unityObjects = new Dictionary<string, GameObject>();
        GameObject[] _obj = GameObject.FindGameObjectsWithTag("UnityObject");
        foreach (GameObject g in _obj)
            unityObjects.Add(g.name,g);

        var settings = SC_GameVariables.Instance;
        var gameLogic = FindObjectOfType<SC_GameLogic>();
        
        var gameBoard = new GameBoard(7, 7);
        var gemPool = new GemPool(unityObjects["GemsHolder"].transform);
        
        var scoreService = new ScoreService();
        var destroyService = new DestroyService(gameBoard, gemPool, scoreService);
        var matchService = new MatchService(gameBoard, settings);
        var spawnService = new SpawnService(gameBoard, gemPool, settings);
        var bombService = new BombService(gameBoard, gameLogic, gemPool, settings);
        var boardService = new BoardService(gameBoard);

        Container.BindInstance(unityObjects).AsSingle();
        Container.Bind<IGameLogic>().FromInstance(gameLogic).AsSingle();
        Container.Bind<IGameBoard>().FromInstance(gameBoard).AsSingle();
        Container.Bind<IGemPool>().FromInstance(gemPool).AsSingle();
        Container.Bind<IMatchService>().FromInstance(matchService).AsSingle();
        Container.Bind<ISpawnService>().FromInstance(spawnService).AsSingle();
        Container.Bind<IDestroyService>().FromInstance(destroyService).AsSingle();
        Container.Bind<IScoreService>().FromInstance(scoreService).AsSingle();
        Container.Bind<IBombService>().FromInstance(bombService).AsSingle();
        Container.Bind<IBoardService>().FromInstance(boardService).AsSingle();
    }
}