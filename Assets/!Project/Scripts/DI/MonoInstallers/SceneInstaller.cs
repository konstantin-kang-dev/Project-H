using UnityEngine;
using Zenject;

public class SceneInstaller : MonoInstaller
{
    [SerializeField] WindowsNavigator _menuPageNavigator;
    public override void InstallBindings()
    {
        Container.Bind<WindowsNavigator>().FromInstance(_menuPageNavigator).AsSingle();

    }
}
