using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MixerInitializer : MonoBehaviour
{
	public AudioMixer tempMixer;
	static AudioMixer mixer;
	private void Start() {
		mixer = tempMixer;

		SceneController.ChangeScene("Main Menu");
	}

	public static void SetMixerVolume(string name, float val) {
		mixer?.SetFloat(name, val);
	}

	public static float GetMixerVolume(string name) {
		float val = 0;
		mixer?.GetFloat(name, out val);
		return val;
	}
}
