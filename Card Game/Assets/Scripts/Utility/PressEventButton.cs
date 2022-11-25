using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PressEventButton : MonoBehaviour
{
	[SerializeField]	bool hasBinding = false;
	public UnityEngine.InputSystem.InputAction binding;

	public PlayerData player;
	//could set it automatically, but don't need to lol
	public int id = 0;
	public event Action pressed;

	public void Press() {
		pressed?.Invoke();
	}

	private void Awake() {
		if (hasBinding) {
			binding.started += ctx => {
				if (player == null || !(player.mouse.disabled || player.mouse.ignore || !player.mouse.essentials || player.mouse.activeSpells != 0))
					Press();
			};
		}
		else {
			binding.Dispose();
			binding = null;
		}
	}

	private void OnEnable() {
		if (hasBinding)
			binding.Enable();
	}

	private void OnDisable() {
		if (hasBinding)
			binding.Disable();
	}
}
