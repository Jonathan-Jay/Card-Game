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

    void Return() {
		//don't include placed cards
		foreach(Card card in player.heldCards) {
			card.CallBackCard();
		}
    }
}
