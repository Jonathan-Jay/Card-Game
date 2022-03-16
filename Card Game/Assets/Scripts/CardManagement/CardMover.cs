using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMover : CardHolder
{
	public CardHolder moveTo;

	public override int DoUpdate() {
		if (holding && moveTo) {
			if (!moveTo.holding) {
				moveTo.PutCard(holding);
			}
		}
		return 0;
	}
}
