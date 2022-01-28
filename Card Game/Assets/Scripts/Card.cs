using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
	public CardData data;
	[SerializeField]	MeshRenderer frontFace;
	[SerializeField]	TextMesh attackMesh;
	[SerializeField]	TextMesh healthMesh;
	[SerializeField]	TextMesh costMesh;

	private void Start() {
		if (data != null) {
			SetData(data);
		}
	}

	public void SetData(CardData newData) {
		data = newData;
		frontFace.material.mainTexture = data.cardArt;
		attackMesh.text = data.attack.ToString();
		healthMesh.text = data.health.ToString();
		healthMesh.color = Color.black;
		string cost = "";
		for (int i = data.cost; i >= 0; --i) {
			cost += 'o';
		}
		costMesh.text = cost;
	}
}
