using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerHand : MonoBehaviour
{
	public HandManager hand;
	public Mouse mouse;

	public InputAction navigateHand;
	public InputAction interact;
	public InputAction denavigate;
	public InputAction navigateMiddle;
	public float delay;
	bool scrolling = false;
	bool rightwards = true;

	private void Awake() {
		navigateHand.started += ctx => StartCoroutine(ScrollHand(ctx.ReadValue<float>()));
		navigateHand.performed += ctx => rightwards = ctx.ReadValue<float>() > 0f;
		navigateHand.canceled += ctx => scrolling = false;

		interact.started += ctx => {
			if (!mouse.disabled && mouse.essentials && hand.splaySelectIndex >= 0) {
				mouse.ForwardClickEvent(hand.transform.GetChild(hand.splaySelectIndex));
				mouse.holding.localPosition = Vector3.up * mouse.vertOffset;
				hand.HoverManagement(null);
				scrolling = false;
			}
		};

		denavigate.started += ctx => {
			if (!mouse.disabled && mouse.essentials)
				hand.HoverManagement(null);
		};

		navigateMiddle.started += ctx => {
			if (!mouse.disabled && mouse.essentials && hand.transform.childCount > 0)
				hand.HoverManagement(hand.transform.GetChild(hand.transform.childCount / 2));
		};
	}

	private void OnEnable() {
		navigateHand.Enable();
		interact.Enable();
		denavigate.Enable();
		navigateMiddle.Enable();
	}

	private void OnDisable() {
		navigateHand.Disable();
		interact.Disable();
		denavigate.Disable();
		navigateMiddle.Disable();
		scrolling = false;
	}

	IEnumerator ScrollHand(float input) {
		rightwards = input > 0f;
		bool prev = rightwards;
		scrolling = true;
		float counter = 0f;

		//if moving left from 0, make a sneaky fix
		if (hand.splaySelectIndex < 0 && !rightwards) {
			hand.splaySelectIndex = 0;
		}

		while (scrolling) {
			if (mouse.disabled) {
				yield return null;
				continue;
			}

			if (counter > 0f) {
				counter -= Time.deltaTime;
				if (counter < 0f)	counter = 0f;
			}

			if ((counter <= 0f || prev != rightwards) && hand.transform.childCount > 0f) {
				if (rightwards)
					hand.HoverManagement(hand.transform.GetChild(
						(hand.splaySelectIndex + 1) % hand.transform.childCount));
				else
					hand.HoverManagement(hand.transform.GetChild(
						(hand.splaySelectIndex - 1 + hand.transform.childCount) % hand.transform.childCount));
				counter = delay;
				prev = rightwards;
			}

			yield return null;
		}
	}
}
