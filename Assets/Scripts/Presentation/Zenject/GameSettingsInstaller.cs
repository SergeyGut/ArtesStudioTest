using Presentation.Settings;
using UnityEngine;
using Zenject;

namespace Presentation.Installers
{
    [CreateAssetMenu(fileName = "GameSettingsInstaller", menuName = "Installers/GameSettingsInstaller")]
    public class GameSettingsInstaller : ScriptableObjectInstaller<GameSettingsInstaller>
    {
        [SerializeField] private GameSettings gameSettings;

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<GameSettings>().FromInstance(gameSettings).AsSingle();
        }
    }
}