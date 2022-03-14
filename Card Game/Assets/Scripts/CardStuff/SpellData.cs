using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//helps shorten things
public delegate void AbilityFunc(PlayerData target, int index, SpellData spell);
public delegate void ActivationFunc(PlayerData target, int index, AbilityFunc ability, SpellData spell);
public delegate PlayerData TargettingFunc(PlayerData current, PlayerData opposing,
	ref int index, ref UnityEngine.RaycastHit hit);

[CreateAssetMenu(fileName = "Spell", menuName = "CardData/SpellData", order = 0)]
public class SpellData : CardData {
	public TargettingFunc targetting;
	[HideInInspector]
	public TargettingOptions targettingOption;
	public ActivationFunc activate;
	[HideInInspector]
	public ActivationOptions activateOption;
	public AbilityFunc ability;
	[HideInInspector]
	public AbilityOptions abilityOption;
	public string cardDescription = "I Forgor :Skull:";
	public int actionParameter1;
	public int actionParameter2;
	public int abilityParameter1;
	public int abilityParameter2;

	public override bool CheckCost(PlayerData player) {
		return player.ReduceMana(cost);
	}
	public override void Init() {
		targetting = GetTargetting(targettingOption);
		activate = GetActivation(activateOption);
		ability = GetAbility(abilityOption);
	}

	//if target is self, this should be null
	public void CastSpell(PlayerData target, int index) {
		//select the target
		activate.Invoke(target, index, ability, this);
	}

	/*
	big space to show that there's lots of differences in this code
	*/

	#region spell enums
	public enum TargettingOptions
	{
		OpposingCard,
		OpposingField,
		SelfField,
		OpposingPlayer,
		PlayerSelf,
		TargetAny,
		TargetAnyPlayer,
		TargetAnyOpposing,
	}
	static public TargettingFunc GetTargetting(TargettingOptions choice) {
		switch (choice) {
			default:
				return TargetOpposingCard;
			case TargettingOptions.OpposingField:
				return TargetOpposingField;
			case TargettingOptions.SelfField:
				return TargetSelfField;
			case TargettingOptions.OpposingPlayer:
				return TargetPlayer;
			case TargettingOptions.PlayerSelf:
				return TargetSelfPlayer;
			case TargettingOptions.TargetAny:
				return TargetAnyCard;
			case TargettingOptions.TargetAnyPlayer:
				return TargetAnyPlayerCard;
			case TargettingOptions.TargetAnyOpposing:
				return TargetAnyOpposingCard;
		}
	}

	public enum ActivationOptions
	{
		Direct,
		Repeated,
		Randomized,
	}
	static public ActivationFunc GetActivation(ActivationOptions choice) {
		switch (choice) {
			default:
				return DirectActivation;
			case ActivationOptions.Repeated:
				return RepeatedActivation;
			case ActivationOptions.Randomized:
				return RandomizedActivation;
		}
	}

	public enum AbilityOptions
	{
		Direct,
		RandomDamage,
	}
	static public AbilityFunc GetAbility(AbilityOptions choice) {
		switch (choice) {
			default:
				return DirectAbility;
			case AbilityOptions.RandomDamage:
				return RandomDamage;
		}
	}
	#endregion

	/*
	big space to show that there's lots of differences in this code
	*/

	#region TargettingOptions
	//return opposing card
	static public PlayerData TargetOpposingCard(PlayerData current,
		PlayerData opposing, ref int index, ref RaycastHit hit)
	{
		if (!(opposing.field[index].holding && opposing.field[index].holding.targetable)) {
			index = -2;
		}
		
		return opposing;
	}

	//return random card on the field
	static public PlayerData TargetOpposingField(PlayerData current,
		PlayerData opposing, ref int index, ref RaycastHit hit)
	{
		List<int> valids = new List<int>();
		index = -2;
		for (int i = opposing.field.Count - 1; i >= 0; --i) {
			if (opposing.field[i].holding && opposing.field[i].holding.targetable) {
				valids.Add(i);
			}
		}
		if (valids.Count > 0) {
			index = valids[Random.Range(0, valids.Count)];
		}

		return opposing;
	}

	//return random card on the field
	static public PlayerData TargetSelfField(PlayerData current,
		PlayerData opposing, ref int index, ref RaycastHit hit)
	{
		List<int> valids = new List<int>();
		index = -2;
		for (int i = current.field.Count - 1; i >= 0; --i) {
			if (current.field[i].holding && current.field[i].holding.targetable) {
				valids.Add(i);
			}
		}
		if (valids.Count > 0) {
			index = valids[Random.Range(0, valids.Count)];
		}

		return current;
	}

	//return opposing player
	static public PlayerData TargetPlayer(PlayerData current,
		PlayerData opposing, ref int index, ref RaycastHit hit)
	{
		index = -1;
		return opposing;
	}

	//return self player
	static public PlayerData TargetSelfPlayer(PlayerData current,
		PlayerData opposing, ref int index, ref RaycastHit hit)
	{
		index = -1;
		return current;
	}

	static public PlayerData TargetAnyCard(PlayerData current,
		PlayerData opposing, ref int index, ref RaycastHit hit)
	{
		//check if hitting something
		if (hit.transform) {
			//check if holder
			CardHolder holder = hit.transform.GetComponent<CardHolder>();
			//check if holding a card if it's a holder
			if (holder != null && holder.holding) {
				//if targettign self, return error code
				if (holder == current.field[index]) {
					index = -2;
					return current;
				}
				//if targetable, just use it i guess :shrug:
				if (holder.holding.targetable) {
					index = holder.index;
					return holder.playerData;
				}
			}
		}
		return null;
	}

	static public PlayerData TargetAnyPlayerCard(PlayerData current,
		PlayerData opposing, ref int index, ref RaycastHit hit)
	{
		//check if hitting something
		if (hit.transform) {
			//check if holder
			CardHolder holder = hit.transform.GetComponent<CardHolder>();
			//check if holding a card if it's a holder
			if (holder != null && holder.holding) {
				//if targettign self, return error code
				if (holder == current.field[index]) {
					index = -2;
					return current;
				}
				//if targetable and valid field, just use it i guess :shrug:
				if (holder.holding.targetable && holder.playerData == current) {
					index = holder.index;
					return holder.playerData;
				}
			}
		}
		return null;
	}

	static public PlayerData TargetAnyOpposingCard(PlayerData current,
		PlayerData opposing, ref int index, ref RaycastHit hit)
	{
		//check if hitting something
		if (hit.transform) {
			//check if holder
			CardHolder holder = hit.transform.GetComponent<CardHolder>();
			//check if holding a card if it's a holder
			if (holder != null && holder.holding) {
				//if targettign self, return error code
				if (holder == current.field[index]) {
					index = -2;
					return current;
				}
				//if targetable and valid field, just use it i guess :shrug:
				if (holder.holding.targetable && holder.playerData == opposing) {
					index = holder.index;
					return holder.playerData;
				}
			}
		}
		return null;
	}
	#endregion

	#region ActivationOptions
	//just calls the ability once on the card target or player if index < 0
	static public void DirectActivation(PlayerData target, int index,
		AbilityFunc ability, SpellData spell)
	{
		ability.Invoke(target, index, spell);
	}

	//call the ability actionParameter1 times
	static public void RepeatedActivation(PlayerData target, int index,
		AbilityFunc ability, SpellData spell)
	{
		for (int i = 0; i < spell.actionParameter1; ++i) {
			ability.Invoke(target, index, spell);
		}
	}

	//target a random card actionParameter1 times
	static public void RandomizedActivation(PlayerData target, int index,
		AbilityFunc ability, SpellData spell)
	{
		//doesn't work if targetting player
		if (index < 0)	return;

		//get number of cards
		int cards = 0;
		foreach (CardHolder holder in target.field) {
			if (holder.holding && holder.holding.targetable)
				++cards;
		}

		for (int i = 0; i < spell.actionParameter1 && cards > 0;) {
			int j = Random.Range(0, target.field.Count);
			if (target.field[j].holding && target.field[j].holding.targetable) {
				ability.Invoke(target, j, spell);
				if (!target.field[j].holding) {
					--cards;
				}
				++i;
			}
		}
	}
	#endregion

	#region AbilityOptions
	//deals abilityParameter1 once
	static public void DirectAbility(PlayerData target, int index, SpellData spell) {
		//targetting player
		if (index < 0) {
			target.TakeDamage(spell.abilityParameter1);
		}
		else if (target.field[index].holding) {
			if (target.field[index].holding.targetable)
				((MonsterCard)target.field[index].holding).TakeDamage(spell.abilityParameter1);
		}
	}

	//between abillityParameter1 inclusive and abilityParamter2 inclusive
	static public void RandomDamage(PlayerData target, int index, SpellData spell) {
		//targetting player
		if (index < 0) {
			target.TakeDamage(UnityEngine.Random.Range(
				spell.abilityParameter1, spell.abilityParameter2 + 1));
		}
		else if (target.field[index].holding) {
			if (target.field[index].holding.targetable)
				((MonsterCard)target.field[index].holding).TakeDamage(UnityEngine.Random.Range(
					spell.abilityParameter1, spell.abilityParameter2 + 1));
		}
	}
#endregion

}
