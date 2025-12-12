using UnityEngine;
using Zenject;

namespace Presentation.Installers
{
    [CreateAssetMenu(fileName = "GameVariablesInstaller", menuName = "Installers/GameVariablesInstaller")]
    public class GameVariablesInstaller : ScriptableObjectInstaller<GameVariablesInstaller>
    {
        [SerializeField] private GameVariables gameVariables;

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<GameVariables>().FromInstance(gameVariables).AsSingle();
        }
    }
}