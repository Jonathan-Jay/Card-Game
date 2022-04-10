using UnityEngine;

public class FodderRain : MonoBehaviour {
	CardGenerator gen;
	void Start() {
		gen = GetComponent<CardGenerator>();
	}

	void Update() {
		gen.GenerateCard();
	}
}
