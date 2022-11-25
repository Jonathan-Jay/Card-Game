using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeTable : MonoBehaviour
{
	public float range = 15f;
	public float counter = 0f;

	public UnityEngine.InputSystem.InputAction shake;

	private void Awake() {
		shake.started += ctx => {
			counter = range;
			StartCoroutine(Shake());
		};
		shake.canceled += ctx => {
			transform.rotation = Quaternion.identity;
			counter = 0f;
		};
	}

	IEnumerator Shake() {
		while (counter > 0) {
			transform.rotation = Quaternion.Euler(Random.Range(-counter, counter), Random.Range(-counter, counter), Random.Range(-range, range));
			counter += Time.deltaTime;
			yield return null;
		}
	}

	private void OnEnable() {
		shake.Enable();
	}

	private void OnDisable() {
		shake.Disable();
	}
}
