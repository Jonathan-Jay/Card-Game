using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundOnCollision : MonoBehaviour
{
	[SerializeField] float minStrength = 2f;
	[SerializeField] AudioQueue soundSource;

	private void OnCollisionEnter(Collision other) {
		if (!soundSource)	return;

		if (other.impulse.magnitude >= minStrength) {
			soundSource.Play();
		}
	}
}
