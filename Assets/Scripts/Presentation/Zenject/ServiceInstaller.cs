using Service;
using Service.Interfaces;
using Zenject;

namespace Presentation.Installers
{
    public class ServiceInstaller : Installer<ServiceInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IPathfinderService>().To<PathfinderService>().AsSingle();
            Container.Bind<ISpawnService>().To<SpawnService>().AsSingle();
            Container.Bind<IDestroyService>().To<DestroyService>().AsSingle();
            Container.Bind<IScoreService>().To<ScoreService>().AsSingle();
            Container.Bind<IBombService>().To<BombService>().AsSingle();
            Container.Bind<IDropService>().To<DropService>().AsSingle();
            Container.Bind<IMatchDispatcher>().To<MatchDispatcher>().AsSingle();
            Container.Bind<ISwapService>().To<SwapService>().AsSingle();
        }
    }
}

