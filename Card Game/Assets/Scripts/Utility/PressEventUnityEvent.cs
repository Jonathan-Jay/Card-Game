using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PressEventUnityEvent : MonoBehaviour
{
	[SerializeField] UnityEvent pressEvent;

	private void OnEnable() {
		GetComponent<PressEventButton>().pressed += Press;
	}

	private void OnDisable() {
		GetComponent<PressEventButton>().pressed -= Press;
	}

	void Press() {
		pressEvent?.Invoke();
	}
}
