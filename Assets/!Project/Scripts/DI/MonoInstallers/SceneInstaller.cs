using UnityEngine;
using Zenject;

public class SceneInstaller : MonoInstaller
{
    [SerializeField] MenuPageNavigator _menuPageNavigator;
    public override void InstallBindings()
    {
        Container.Bind<MenuPageNavigator>().FromInstance(_menuPageNavigator).AsSingle();

    }
}
