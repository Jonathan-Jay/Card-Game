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
		GetComponent<PressEventButton>().pressed += GenerateCard;
		if (player)
			player.startOfTurn += Refresh;
	}

	private void OnDisable() {
		GetComponent<PressEventButton>().pressed -= GenerateCard;
		if (player)
			player.startOfTurn -= Refresh;
	}

	private void Start() {
		Refresh();
	}

	public void SetPlayer(PlayerData newPlayer) {
		player = newPlayer;
		GetComponent<PressEventButton>().player = player;
		if (player)
			player.startOfTurn += Refresh;

		Refresh();
	}

	int uses = 0;
	public void GenerateCard() {
		if (!player || uses <= 0)	return;

		if (!player.ReduceMana(manaCost))	return;

		Card card = Instantiate(prefab, transform.position + transform.rotation * spawnOffset,
			transform.rotation * Quaternion.Euler(spawnRot));

		card.SetData(templateData);
		card.player = player;

		player.AddCard(card);

		if (ServerManager.CheckIfClient(player, true)) {
			card.RenderFace();
		}

		//immediately return to hand? yes
		card.CallBackCard();

		if (--uses <= 0) {
			mesh.material.mainTexture = emptyTexture;
			text.text = "";
		}
	}

	public void Refresh() {
		if (player && (player.currentMana >= manaCost)) {
			uses = usagesPerTurn;
			text.text = manaCost.ToString() + "*";
			mesh.material.mainTexture = templateData.cardArt;
		}
		else {
			mesh.material.mainTexture = emptyTexture;
			text.text = "";
		}
	}
}
