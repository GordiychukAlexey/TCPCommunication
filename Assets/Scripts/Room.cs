using Interfaces;
using UnityEngine;

public class Room : MonoBehaviour, IServerSide {
	[SerializeField] private Light light;
	[SerializeField] private ParticleSystem explosionPrefab;
	[SerializeField] private Transform explosionSpawnPoint;

	public void SwitchLight() => light.gameObject.SetActive(!light.gameObject.activeSelf);

	public void Explode() => Instantiate(explosionPrefab, explosionSpawnPoint.position, Quaternion.identity, transform);
}