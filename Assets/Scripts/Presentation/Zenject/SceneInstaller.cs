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
        public override void InstallBindings()
        {
            DomainInstaller.Install(Container);
            ServiceInstaller.Install(Container);

            InstallUnityBindings();
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
            
            Container.Bind<IGameStateProvider>().To<GameStateProvider>().AsSingle();
            Container.Bind<IGemPool<IPiece>>().To<GemPool>().AsSingle();
            
            Container.BindInterfacesTo<ScoreUpdater>().AsSingle();
            Container.BindInterfacesTo<BoardView>().AsSingle();
        }
    }
}