using UnityEngine;
using Zenject;

public class SceneInstaller : MonoInstaller
{
    [SerializeField] MenuWindowNavigator _menuPageNavigator;
    public override void InstallBindings()
    {
        Container.Bind<MenuWindowNavigator>().FromInstance(_menuPageNavigator).AsSingle();

    }
}
