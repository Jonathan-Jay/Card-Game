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
	public Func<Card, RaycastHit, Card> targetting = DefaultTargetting;
	public Action<Card, Action<Card, int, int>, int, int, int> activate = DirectActivation;
	public Action<Card, int, int> ability = DirectAbility;
	public int actionParameter = 0;
	public int abilityParameter1 = 0;
	public int abilityParameter2 = 0;

	//if target is self, this should be null
	public void CastSpell(Card target) {
		//select the target
		activate.Invoke(target, ability, actionParameter, abilityParameter1, abilityParameter2);
	}



	//to stop errors lol
	//just returns self
	static public Card DefaultTargetting(Card current, RaycastHit hit) {
		return current;
	}

	//just calls the ability once on the target
	static public void DirectActivation(Card target, Action<Card, int, int> ability,
			int actionParameter, int abilityParameter1, int abilityParameter2) {
		ability.Invoke(target, abilityParameter1, abilityParameter2);
	}

	//deals abilityParameter1 once
	static public void DirectAbility(Card target, int abilityParameter1, int abilityParameter2) {
		if (target != null) {
			((MonsterCard)target).TakeDamage(abilityParameter1);
		}
	}

#region targettingOptions

#endregion

#region ActivationOptions
	//call the ability actionParameter times
	static public void RepeatedActivation(Card target, Action<Card, int, int> ability,
			int actionParameter, int abilityParameter1, int abilityParameter2) {
		for (int i = 0; i < actionParameter; ++i) {
			ability.Invoke(target, abilityParameter1, abilityParameter2);
		}
	}
#endregion

#region AbilityOptions
	//between abillityParameter1 inclusive and abilityParamter2 inclusive
	static public void RandomDamage(Card target, int abilityParameter1, int abilityParameter2) {
		if (target != null) {
			((MonsterCard)target).TakeDamage(UnityEngine.Random.Range(
				abilityParameter1, abilityParameter2 + 1));
		}
	}
#endregion

}
