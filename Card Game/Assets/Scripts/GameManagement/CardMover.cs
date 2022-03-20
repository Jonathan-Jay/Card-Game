using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMover : CardHolder
{
	public CardHolder moveTo;

	public override void DoUpdate() {
		if (holding && holding.targetable && moveTo) {
			if (!moveTo.holding) {
				moveTo.PutCard(holding);
			}

			//do update to boosts
			((MonsterCard)holding).UpdateBoosts();
		}
	}
}
