using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Spell", menuName = "CardData/SpellData", order = 0)]
public class SpellData : CardData {
	public bool canTargetOpponentHolders;
	public bool canTargetSelfHolders;
	public bool canTargetOpponent;
	public bool canTargetSelf;
	public Func<Card> targetting;
	public Action<Card, Action<int>> activate;
	public Action<int> ability;

	//if target is self, this should be null
	public void CastSpell(Card target) {
		//select the target
		activate.Invoke(target, ability);
	}
}
