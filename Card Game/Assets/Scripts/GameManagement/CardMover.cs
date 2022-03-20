using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMover : CardHolder
{
	public CardHolder moveTo;

	public override void DoUpdate() {
		if (holding && holding.targetable && moveTo) {
			//do update to boosts if first turn only
			if (GameController.firstTurn)
				((MonsterCard)holding).UpdateBoosts();

			if (!moveTo.holding) {
				moveTo.PutCard(holding);
			}
		}
	}
}
