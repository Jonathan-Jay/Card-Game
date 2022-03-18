using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioQueue : MonoBehaviour
{
	[SerializeField] List<AudioSource> sounds = new List<AudioSource>();
	private void Start() {
		//only fill if not manually assigned
		if (sounds.Count == 0) {
			foreach (AudioSource source in GetComponents<AudioSource>()) {
				sounds.Add(source);
			}
		}
	}

	int index = 0;
	public void Play() {
		sounds[index].Play();
		index = (index + 1) % sounds.Count;
	}

	private void OnEnable() {
		PressEventButton test = GetComponent<PressEventButton>();
		if (test) {
			test.pressed += Play;
		}
	}
	
	private void OnDisable() {
		PressEventButton test = GetComponent<PressEventButton>();
		if (test) {
			test.pressed -= Play;
		}
	}
}
