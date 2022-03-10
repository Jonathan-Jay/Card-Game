using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignPlayer : MonoBehaviour
{
	public GameController game;
	public bool isPlayer1;
	bool dirty = false;

	private void Awake() {
		if (!game)	return;
		
		Card card = GetComponent<Card>();
		if (card) {
			card.player = isPlayer1 ? game.player1 : game.player2;
			dirty = true;
		}
		Mouse mouse = GetComponent<Mouse>();
		if (mouse) {
			mouse.player = isPlayer1 ? game.player1 : game.player2;
			dirty = true;
		}
		if (dirty)
			DestroyImmediate(this);
	}

	private void Update() {
		Awake();
	}
}
