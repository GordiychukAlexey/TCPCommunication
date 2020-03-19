using UniRx;

namespace Interfaces {
	public interface IClientSideControls {
		Subject<Unit> SwitchLight{ get; }
		Subject<Unit> Explode{ get; }
	}
}