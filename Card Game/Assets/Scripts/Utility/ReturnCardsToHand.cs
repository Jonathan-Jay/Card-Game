using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PressEventButton))]
public class ReturnCardsToHand : MonoBehaviour
{
	[SerializeField] PlayerData player;
	private void OnEnable() {
		GetComponent<PressEventButton>().pressed += Return;
	}
	private void OnDisable() {
		GetComponent<PressEventButton>().pressed -= Return;
	}

	public void SetPlayer(PlayerData newPlayer) {
		player = newPlayer;
		GetComponent<PressEventButton>().player = player;
	}

	public void Return() {
		ReturnAll(player);
	}

	public static void ReturnAll(PlayerData player) {
		if (!player)	return;

		foreach(Card card in player.heldCards) {
			card.CallBackCard();
		}
	}
}
