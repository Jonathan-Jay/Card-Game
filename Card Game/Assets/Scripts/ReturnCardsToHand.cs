using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnCardsToHand : MonoBehaviour
{
	[SerializeField] PlayerData player;
	private void OnEnable() {
		GetComponent<PressEventButton>().pressed += Return;
	}
	private void OnDisable() {
		GetComponent<PressEventButton>().pressed -= Return;
	}

    // Update is called once per frame
    void Return()
    {
        foreach(Card card in FindObjectsOfType<Card>()) {
			if (!card.placement) {
				if (player == null || card.player == player)
					card.CallBackCard();
			}
		}
    }
}
