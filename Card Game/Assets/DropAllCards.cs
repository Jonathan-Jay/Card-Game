using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropAllCards : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Slash)) {
			Card[] cards = transform.GetComponentsInChildren<Card>();
			transform.DetachChildren();
			foreach(Card card in cards) {
				card.CallBackCard();
			}
		}
    }
}
