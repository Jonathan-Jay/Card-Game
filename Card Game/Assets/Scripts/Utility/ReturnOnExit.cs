using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnOnExit : MonoBehaviour
{
	[SerializeField]	Transform parentTest;
	[SerializeField]	Vector3 spawnPos = Vector3.up * 1f;

	private void OnTriggerExit(Collider other) {
		if (other.transform.parent == parentTest) {
			StartCoroutine(DelayedSpawning(other.transform));
		}
	}

	float delay = 0f;
	public float respawnDelay = 0.25f;
	IEnumerator DelayedSpawning(Transform target) {
		float tempDelay = delay;
		delay += respawnDelay;
		yield return new WaitForSeconds(tempDelay);

		target.localPosition = spawnPos;

		delay -= respawnDelay;
	}
}
