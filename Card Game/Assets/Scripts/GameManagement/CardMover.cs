using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMover : CardHolder
{
	public CardHolder moveTo;

	public override void DoUpdate() {
		if (holding && holding.targetable && moveTo) {
			//check if you can move the card
			if (!moveTo.holding) {
				moveTo.PutCard(holding);
			}
			else {
				//if card stays still, then update boosts
				((MonsterCard)holding).UpdateBoosts();
			}
		}
	}
}
