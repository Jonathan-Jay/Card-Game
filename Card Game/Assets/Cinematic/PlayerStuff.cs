using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStuff : MonoBehaviour
{
	public PlayerData p1;
	public PlayerData p2;
	public void HoverP1(int index) {
		if (index < p1.heldCards.Count)
			p1.hand.HoverManagement(p1.heldCards[index].transform);
		else
			p1.hand.HoverManagement(null);
	}
	public void GrabP1() {
		p1.hand.input.GrabCard();
	}
	public void PlaceP1(int index) {
		p1.PlayCard(index);
	}
	public void RingP1() {
		p1.turnEndButton.Press();
	}
	public void HoverP2(int index) {
		if (index < p2.heldCards.Count)
			p2.hand.HoverManagement(p2.heldCards[index].transform);
		else
			p2.hand.HoverManagement(null);
	}
	public void GrabP2() {
		p2.hand.input.GrabCard();
	}
	public void PlaceP2(int index) {
		p2.PlayCard(index);
	}
	public void RingP2() {
		p2.turnEndButton.Press();
	}
}
