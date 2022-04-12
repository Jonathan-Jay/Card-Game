using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardGenerator : MonoBehaviour
{
	[SerializeField]	PlayerData player;
	[SerializeField]	Texture2D emptyTexture;
	[SerializeField]	MeshRenderer mesh;
	[SerializeField]	TMPro.TMP_Text text;
	[SerializeField]	CardData templateData;
	//make sure this matched the templateData type
	[SerializeField]	Card prefab;
	[SerializeField]	int manaCost = 2;
	[SerializeField]	int usagesPerTurn = 1;
	[SerializeField]	Vector3 spawnOffset = Vector3.back * 0.5f + Vector3.up * 0.1f;
	[SerializeField]	Vector3 spawnRot = Vector3.zero;

	private void OnEnable() {
		PressEventButton test = GetComponent<PressEventButton>();
		if (test)
			test.pressed += GenerateCard;

		if (player) {
			player.startOfTurn += Refresh;
			player.manaUpdated += HideTest;
		}
	}

	private void OnDisable() {
		PressEventButton test = GetComponent<PressEventButton>();
		if (test)
			test.pressed -= GenerateCard;

		if (player) {
			player.startOfTurn -= Refresh;
			player.manaUpdated -= HideTest;
		}
	}

	private void Start() {
		Refresh();
	}

	public void SetPlayer(PlayerData newPlayer) {
		player = newPlayer;
		GetComponent<PressEventButton>().player = player;
		if (player) {
			player.startOfTurn += Refresh;
			player.manaUpdated += HideTest;
		}

		Refresh();
	}

	int uses = 0;
	public void GenerateCard() {
		if (uses <= 0)	return;

		if (player && !player.ReduceMana(manaCost))	return;

		Card card = Instantiate(prefab, transform.position + transform.rotation * spawnOffset,
			transform.rotation * Quaternion.Euler(spawnRot));

		card.SetData(templateData);
		if (--uses <= 0) {
			mesh.material.mainTexture = emptyTexture;
			text.text = "";
		}

		if (!player) {
			card.RenderFace();
			return;
		}

		card.player = player;

		player.AddCard(card);

		if (ServerManager.CheckIfClient(player, true)) {
			card.RenderFace();
		}

		//immediately return to hand? yes
		card.CallBackCard();
	}

	public void Refresh() {
		if ((player && (player.currentMana >= manaCost)) || !player) {
			uses = usagesPerTurn;
		}
		HideTest(0);
	}

	public void HideTest(int prevVal) {
		if (uses > 0 && player.currentMana >= manaCost) {
			text.text = manaCost.ToString() + "*";
			mesh.material.mainTexture = templateData.cardArt;
		}
		else {
			text.text = "";
			mesh.material.mainTexture = emptyTexture;
		}
	}
}
