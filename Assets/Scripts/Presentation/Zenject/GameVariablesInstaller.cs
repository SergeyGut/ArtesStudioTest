using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "GameVariablesInstaller", menuName = "Installers/GameVariablesInstaller")]
public class GameVariablesInstaller : ScriptableObjectInstaller<GameVariablesInstaller>
{
    [SerializeField]
    private SC_GameVariables gameVariables;
    
    public override void InstallBindings()
    {
        Container.Bind<ISettings>().FromInstance(gameVariables).AsSingle();
    }
}

