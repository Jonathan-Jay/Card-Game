using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GoBackMenu : MonoBehaviour
{
	[SerializeField]	InputActionReference reference;
	InputAction goBack;
	[SerializeField]	GameObject newSelected;

	UnityEngine.EventSystems.EventSystem eventSystem;
	private void Awake() {
		eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();

		goBack = reference.action;

		goBack.started += ctx => eventSystem.SetSelectedGameObject(newSelected);
	}

	private void OnEnable() {
		goBack.Enable();
	}

	private void OnDisable() {
		goBack.Disable();
	}

	public void SetBackSelection(GameObject selected) {
		newSelected = selected;
	}
}
