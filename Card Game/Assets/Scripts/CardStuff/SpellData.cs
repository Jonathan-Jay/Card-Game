using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//helps shorten things
using AbilityFunc = System.Action<Card, SpellData>;
using ActivationFunc = System.Action<GameController.PlayerData, int, System.Action<Card, SpellData>, SpellData>;

[CreateAssetMenu(fileName = "Spell", menuName = "CardData/SpellData", order = 0)]
public class SpellData : CardData {
	public System.Func<Card, RaycastHit, Card> targetting = DefaultTargetting;
	public ActivationFunc activate = DirectActivation;
	public AbilityFunc ability = DirectAbility;
	public string cardDescription = "I Forgor :Skull:";
	public int actionParameter1;
	public int actionParameter2;
	public int abilityParameter1;
	public int abilityParameter2;

	public override bool CheckCost(GameController.PlayerData player) {
		if (player.currentMana >= cost) {
			player.currentMana -= cost;
			return true;
		}
		return false;
	}

	//if target is self, this should be null
	public void CastSpell(GameController.PlayerData target, int index) {
		//select the target
		activate.Invoke(target, index, ability, this);
	}



	//to stop errors lol
	//just returns self
	static public Card DefaultTargetting(Card current, RaycastHit hit) {
		return current;
	}

	//just calls the ability once on the card target
	static public void DirectActivation(GameController.PlayerData target, int index,
		AbilityFunc ability, SpellData spell)
	{
		ability.Invoke(target.field[index].holding, spell);
	}

	//deals abilityParameter1 once
	static public void DirectAbility(Card target, SpellData spell) {
		if (target != null) {
			((MonsterCard)target).TakeDamage(spell.abilityParameter1);
		}
	}

#region targettingOptions
	static public Card TargetPlayer(Card current, RaycastHit hit) {
		return current;
	}
#endregion

#region ActivationOptions
	//call the ability actionParameter1 times
	static public void RepeatedActivation(GameController.PlayerData target, int index,
		AbilityFunc ability, SpellData spell)
	{
		for (int i = 0; i < spell.actionParameter1; ++i) {
			ability.Invoke(target.field[index].holding, spell);
		}
	}

	//target a random card index times
	static public void RandomizedActivation(GameController.PlayerData target, int index,
		AbilityFunc ability, SpellData spell)
	{
		for (int i = 0; i < index;) {
			int j = Random.Range(0, target.field.Count);
			if (target.field[index].holding) {
				ability.Invoke(target.field[index].holding, spell);
				++i;
			}
		}
	}
#endregion

#region AbilityOptions
	//between abillityParameter1 inclusive and abilityParamter2 inclusive
	static public void RandomDamage(Card target, SpellData spell) {
		if (target != null) {
			((MonsterCard)target).TakeDamage(UnityEngine.Random.Range(
				spell.abilityParameter1, spell.abilityParameter2 + 1));
		}
	}
#endregion

}
