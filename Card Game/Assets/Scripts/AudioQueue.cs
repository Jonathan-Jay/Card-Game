using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioQueue : MonoBehaviour
{
	List<AudioSource> sounds = new List<AudioSource>();
	private void Start() {
		foreach (AudioSource source in GetComponents<AudioSource>()) {
			sounds.Add(source);
		}
	}

	int index = 0;
	public void Play() {
		sounds[index].Play();
		index = (index + 1) % sounds.Count;
	}
}
