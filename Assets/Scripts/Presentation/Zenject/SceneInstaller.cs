using System.Collections.Generic;
using TMPro;
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

        var gemsHolder = unityObjects["GemsHolder"].transform;
        var scoreText = unityObjects["Txt_Score"].GetComponent<TextMeshProUGUI>();
        
        Container.Bind<IGameBoard>().To<GameBoard>().AsSingle();
        Container.Bind<IMatchService>().To<MatchService>().AsSingle();
        Container.Bind<IMatchCounterService>().To<MatchCounterService>().AsSingle();
        Container.Bind<IGameStateProvider>().To<GameStateProvider>().AsSingle();
        Container.Bind<IMatchDispatcher>().To<MatchDispatcher>().AsSingle();
        Container.Bind<Transform>().WithId("GemsHolder").FromInstance(gemsHolder).AsSingle();
        Container.Bind<TextMeshProUGUI>().WithId("ScoreText").FromInstance(scoreText).AsSingle();
        Container.Bind<IGemPool<IPiece>>().To<GemPool>().AsSingle();
        Container.Bind<IPathfinderService>().To<PathfinderService>().AsSingle();
        Container.Bind<ISpawnService>().To<SpawnService>().AsSingle();
        Container.Bind<IDestroyService>().To<DestroyService>().AsSingle();
        Container.Bind<IScoreService>().To<ScoreService>().AsSingle();
        Container.Bind<IBombService>().To<BombService>().AsSingle();
        Container.Bind<IDropService>().To<DropService>().AsSingle();

        Container.BindInterfacesTo<ScoreUpdater>().AsSingle();
        Container.BindInterfacesTo<BoardView>().AsSingle();
    }
}