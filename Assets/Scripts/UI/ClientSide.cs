using Interfaces;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
	public class ClientSide : MonoBehaviour, IClientSide {
		public Subject<Unit> SwitchLight{ get; } = new Subject<Unit>();
		public Subject<Unit> Explode{ get; } = new Subject<Unit>();

		[SerializeField] private Button SwitchLightButton;
		[SerializeField] private Button ExplosionButton;

		private void Awake(){
			SwitchLightButton.OnClickAsObservable().Subscribe(_ => SwitchLight.OnNext(Unit.Default));
			ExplosionButton.OnClickAsObservable().Subscribe(_ => Explode.OnNext(Unit.Default));
		}
	}
}