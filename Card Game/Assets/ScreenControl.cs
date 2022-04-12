using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class ScreenControl : MonoBehaviour
{
	[SerializeField] UnityEngine.UI.Slider musicSlider;
	[SerializeField] UnityEngine.UI.Slider sfxSlider;

	private void Start() {
		musicSlider.value = MixerInitializer.GetMixerVolume("Music");
		sfxSlider.value = MixerInitializer.GetMixerVolume("SFX");
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

	public static void SetMusicVolume(float val) {
		MixerInitializer.SetMixerVolume("Music", val);
	}

	public static void SetSFXVolume(float val) {
		MixerInitializer.SetMixerVolume("SFX", val);
	}
}
