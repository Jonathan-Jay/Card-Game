using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisableSelf : MonoBehaviour
{
	Button button;

    private void Awake() {
		button = GetComponent<Button>();
	}

    public void DisableForAMoment(float length) {
		StartCoroutine(Wait(length));
	}

	IEnumerator Wait(float length) {
		button.interactable = false;
		//play the sound
		yield return new WaitForSeconds(length);
		button.interactable = true;
	}
}
