using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        var unityObjects = new Dictionary<string, GameObject>();
        GameObject[] obj = GameObject.FindGameObjectsWithTag("UnityObject");
        foreach (GameObject g in obj)
            unityObjects.Add(g.name,g);

        IGemPool<IPiece> gemPool = new GemPool(unityObjects["GemsHolder"].transform, Container);
        
        Container.Bind<IGameBoard>().To<GameBoard>().AsSingle();
        Container.Bind<IGameStateProvider>().To<GameStateProvider>().AsSingle();
        Container.Bind<IGameLogic>().To<GameLogic>().AsSingle();
        Container.BindInstance(unityObjects).AsSingle();
        Container.Bind<IGemPool<IPiece>>().FromInstance(gemPool).AsSingle();
        Container.Bind<IMatchService>().To<MatchService>().AsSingle();
        Container.Bind<ISpawnService>().To<SpawnService>().AsSingle();
        Container.Bind<IDestroyService>().To<DestroyService>().AsSingle();
        Container.Bind<IScoreService>().To<ScoreService>().AsSingle();
        Container.Bind<IBombService>().To<BombService>().AsSingle();
        Container.Bind<IDropService>().To<DropService>().AsSingle();

        Container.BindInterfacesTo<ScoreUpdater>().AsSingle();
        Container.BindInterfacesTo<BoardView>().AsSingle();
    }
}