using System.Collections.Generic;
using Domain.Interfaces;
using Service.Interfaces;
using TMPro;
using UnityEngine;
using Zenject;

namespace Presentation.Installers
{
    public class SceneInstaller : MonoInstaller
    {
        private DiContainer domainContainer;
        private DiContainer serviceContainer;

        public override void InstallBindings()
        {
            InstallUnityBindings();
            InstallPresentationBindings();
            InstallDomainBindings();
            InstallServiceBindings();
            InstallPresentationComponents();
        }

        private void InstallUnityBindings()
        {
            var unityObjects = new Dictionary<string, GameObject>();
            GameObject[] obj = GameObject.FindGameObjectsWithTag("UnityObject");
            foreach (GameObject g in obj)
                unityObjects.Add(g.name, g);

            var gemsHolder = unityObjects["GemsHolder"].transform;
            var scoreText = unityObjects["Txt_Score"].GetComponent<TextMeshProUGUI>();

            Container.Bind<Transform>().WithId("GemsHolder").FromInstance(gemsHolder).AsSingle();
            Container.Bind<TextMeshProUGUI>().WithId("ScoreText").FromInstance(scoreText).AsSingle();
        }

        private void InstallPresentationBindings()
        {
            Container.Bind<IGameStateProvider>().To<GameStateProvider>().AsSingle();
            Container.Bind<IGemPool<IPiece>>().To<GemPool>().AsSingle();
            Container.BindInterfacesTo<BoardView>().AsSingle();
        }

        private void InstallDomainBindings()
        {
            domainContainer = Container.CreateSubContainer();
            DomainInstaller.Install(domainContainer);

            Container.Bind<IGameBoard>().FromMethod(ctx => domainContainer.Resolve<IGameBoard>()).AsSingle();
            Container.Bind<IMatchService>().FromMethod(ctx => domainContainer.Resolve<IMatchService>()).AsSingle();
            Container.Bind<IMatchCounterService>().FromMethod(ctx => domainContainer.Resolve<IMatchCounterService>()).AsSingle();
        }

        private void InstallServiceBindings()
        {
            serviceContainer = domainContainer.CreateSubContainer();
            ServiceInstaller.Install(serviceContainer);

            Container.Bind<IPathfinderService>().FromMethod(ctx => serviceContainer.Resolve<IPathfinderService>()).AsSingle();
            Container.Bind<ISpawnService>().FromMethod(ctx => serviceContainer.Resolve<ISpawnService>()).AsSingle();
            Container.Bind<IDestroyService>().FromMethod(ctx => serviceContainer.Resolve<IDestroyService>()).AsSingle();
            Container.Bind<IScoreService>().FromMethod(ctx => serviceContainer.Resolve<IScoreService>()).AsSingle();
            Container.Bind<IBombService>().FromMethod(ctx => serviceContainer.Resolve<IBombService>()).AsSingle();
            Container.Bind<IDropService>().FromMethod(ctx => serviceContainer.Resolve<IDropService>()).AsSingle();
            Container.Bind<IMatchDispatcher>().FromMethod(ctx => serviceContainer.Resolve<IMatchDispatcher>()).AsSingle();
        }

        private void InstallPresentationComponents()
        {
            Container.BindInterfacesTo<ScoreUpdater>().AsSingle();
        }
    }
}