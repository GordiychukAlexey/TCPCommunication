using Interfaces;
using Zenject;

public class GameInstaller : MonoInstaller {
	public override void InstallBindings(){
		Container.Bind<IClientSideControls>().To<ClientSideControls>().FromResolve().AsSingle().NonLazy();
		Container.Bind<IServerSide>().To<Room>().FromResolve().AsSingle().NonLazy();
	}
}