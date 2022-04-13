using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class ScreenControl : MonoBehaviour
{
	[SerializeField] UnityEngine.UI.Slider masterSlider;
	[SerializeField] UnityEngine.UI.Slider musicSlider;
	[SerializeField] UnityEngine.UI.Slider sfxSlider;

	private void Start() {
		masterSlider.value = Mathf.Pow(10f, MixerInitializer.GetMixerVolume("Master") / 80f);
		musicSlider.value = Mathf.Pow(10f, MixerInitializer.GetMixerVolume("Music") / 80f);
		sfxSlider.value = Mathf.Pow(10f, MixerInitializer.GetMixerVolume("SFX") / 80f);
	}

	static int windowedWidth = Screen.width;
	static int windowedHeight = Screen.height;
	public static void ToggleFullScreen() {
		if (Screen.fullScreen) {
			Screen.SetResolution(windowedWidth, windowedHeight, false);
		}
		else {
			windowedWidth = Screen.width;
			windowedHeight = Screen.height;

			Resolution temp = Screen.resolutions[Screen.resolutions.Length - 1];
			Screen.SetResolution(temp.width, temp.height, true);
		}
	}

	public static void SetMasterVolume(float val) {
		MixerInitializer.SetMixerVolume("Master", 80f * Mathf.Log10(val));
	}

	public static void SetMusicVolume(float val) {
		MixerInitializer.SetMixerVolume("Music", 80f * Mathf.Log10(val));
	}

	public static void SetSFXVolume(float val) {
		MixerInitializer.SetMixerVolume("SFX", 80f * Mathf.Log10(val));
	}
}
