using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Spell", menuName = "CardData/SpellData", order = 0)]
public class SpellData : CardData {
	public bool canTargetOpponentHolders = true;
	public bool canTargetSelfHolders = false;
	public bool canTargetOpponent = false;
	public bool canTargetSelf = false;
	public Func<Card, Card> targetting = DefaultTargetting;
	public Action<Card, Action<Card>> activate = DefaultActivation;
	public Action<Card> ability = DefaultAbility;

	//if target is self, this should be null
	public void CastSpell(Card target) {
		//select the target
		activate.Invoke(target, ability);
	}



	//to stop errors lol
	//cast rays or something to perform rays
	static public Card DefaultTargetting(Card current) {
		return current;
	}

	//just calls the ability, could use this for like multi-target stuff
	static public void DefaultActivation(Card target, Action<Card> ability) {
		ability.Invoke(target);
	}

	//does nothing, consider casting card to monster
	static public void DefaultAbility(Card target) {
		
	}
}
