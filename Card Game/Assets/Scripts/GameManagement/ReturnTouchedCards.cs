using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnTouchedCards : MonoBehaviour
{
	Card card;
	private void OnCollisionEnter(Collision other) {
		card = other.transform.GetComponent<Card>();
		if (card) {
			card.CallBackCard();
			card = null;
		}
	}
}
