using Interfaces;
using UI;
using Zenject;

public class GameInstaller : MonoInstaller {
	public override void InstallBindings(){
		Container.Bind<IClientSide>().To<ClientSide>().FromResolve().AsSingle().NonLazy();
		Container.Bind<IServerSide>().To<Room>().FromResolve().AsSingle().NonLazy();
	}
}