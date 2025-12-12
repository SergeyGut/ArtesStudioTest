using Domain;
using Domain.Interfaces;
using Zenject;

namespace Presentation.Installers
{
    public class DomainInstaller : Installer<DomainInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IGameBoard>().To<GameBoard>().AsSingle();
            Container.Bind<IMatchService>().To<MatchService>().AsSingle();
            Container.Bind<IMatchCounterService>().To<MatchCounterService>().AsSingle();
        }
    }
}

