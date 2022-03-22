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
	public void PlayRandom()
	{
		//Randomise the track played in the queue (will not play the track that was played last)
		int newIndex = Random.Range(0, sounds.Count);
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
