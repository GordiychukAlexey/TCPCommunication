using Interfaces;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ClientSideControls : MonoBehaviour, IClientSideControls {
	public Subject<Unit> SwitchLight{ get; } = new Subject<Unit>();
	public Subject<Unit> Explode{ get; } = new Subject<Unit>();

	[SerializeField] private Button SwitchLightButton;
	[SerializeField] private Button ExplosionButton;

	private void Awake(){
		SwitchLightButton.OnClickAsObservable().Subscribe(_ => SwitchLight.OnNext(Unit.Default));
		ExplosionButton.OnClickAsObservable().Subscribe(_ => Explode.OnNext(Unit.Default));
	}
}