using UniRx;

namespace Interfaces {
	public interface IClientSide {
		Subject<Unit> SwitchLight{ get; }
		Subject<Unit> Explode{ get; }
	}
}