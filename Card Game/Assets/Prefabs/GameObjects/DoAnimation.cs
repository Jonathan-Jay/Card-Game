using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoAnimation : MonoBehaviour
{
	[SerializeField] Animator anim;

	private void OnEnable() {
		GetComponent<PressEventButton>().pressed += Play;
	}
	private void OnDisable() {
		GetComponent<PressEventButton>().pressed -= Play;
	}

	void Play() {
		anim.Play("Ding");
	}
}
