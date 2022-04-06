using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioQueue : MonoBehaviour
{
	[SerializeField] List<AudioSource> sounds = new List<AudioSource>();
	private void Awake() {
		//only fill if not manually assigned
		if (sounds.Count == 0) {
			foreach (AudioSource source in GetComponents<AudioSource>()) {
				sounds.Add(source);
			}
		}
	}

	public void AddClip(AudioClip clip) {
		foreach (AudioSource source in sounds) {
			source.clip = clip;
		}
	}

	int index = 0;
	public void Play() 
	{
		sounds[index].Play();
		index = (index + 1) % sounds.Count;
	}

	public void PlayIndex(int index)
	{
		//selects a track to play and will play it.
		sounds[index % sounds.Count].Play();
	}

	int lastPlayed = -1;
	System.Random rand = null;
	public void PlayRandom()
	{
		if (rand == null)
			rand = new System.Random();
		//Randomise the track played in the queue (will not play the track that was played last)
		int newIndex = rand.Next() % sounds.Count;
		if(newIndex == lastPlayed)
		{
			newIndex = (newIndex + 1) % sounds.Count;
		}
		sounds[newIndex].Play();
		lastPlayed = newIndex;
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
