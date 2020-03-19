using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class DestroyAfterEndPlay : MonoBehaviour {
	private ParticleSystem thisParticleSystem;

	private void Awake(){
		thisParticleSystem = GetComponent<ParticleSystem>();
	}

	void Start(){
		StartCoroutine(WaitAndDestroy());
	}

	private IEnumerator WaitAndDestroy(){
		yield return new WaitWhile(thisParticleSystem.IsAlive);
		Destroy(gameObject);
	}
}