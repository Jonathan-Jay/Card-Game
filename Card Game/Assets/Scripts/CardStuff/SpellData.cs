using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//helps shorten things
public delegate PlayerData TargettingFunc(PlayerData current, PlayerData opposing,
	ref int index, ref Transform hit);
public delegate void ActivationFunc(SpellCard caster, PlayerData target, int index, SpellData spell);
public delegate void AbilityFunc(PlayerData target, int index, SpellData spell);

[CreateAssetMenu(fileName = "Spell", menuName = "CardData/SpellData", order = 0)]
public class SpellData : CardData {
	public TargettingFunc targetting;
	public TargettingOptions targettingOption;
	public ActivationFunc activate;
	public ActivationOptions activateOption;
	public AbilityFunc ability;
	public AbilityOptions abilityOption;
	public string cardDescription = "I Forgor :Skull:";
	public int actionParameter1;
	public int actionParameter2;
	public int abilityParameter1;
	public int abilityParameter2;
	public int abilityParameter3;
	public SpellEffect effect;

	public override bool CheckCost(PlayerData player) {
		return player.ReduceMana(cost);
	}

	//set the spell funcs, makes things a little cleaner
	public override void Init() {
		targetting = GetTargetting(targettingOption);
		activate = GetActivation(activateOption);
		ability = GetAbility(abilityOption);
	}

	//if target is self, this should be null
	public void CastSpell(SpellCard caster, PlayerData target, int index) {
		//select the target
		activate.Invoke(caster, target, index, this);
	}

	/*
	big space to show that there's lots of differences in this code
	*/

	#region spell enums
	public enum TargettingOptions {
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

	public enum ActivationOptions {
		Direct,
		Repeated,
		Randomized,
		Everything
	}
	static public ActivationFunc GetActivation(ActivationOptions choice) {
		switch (choice) {
			default:
				return DirectActivation;
			case ActivationOptions.Repeated:
				return RepeatedActivation;
			case ActivationOptions.Randomized:
				return RandomizedActivation;
			case ActivationOptions.Everything:
				return EverythingActivation;
		}
	}

	public enum AbilityOptions {
		Direct,
		RandomDamage,
		Kill,
		Boost,
		StealMana,
		DrawCard,
	}
	static public AbilityFunc GetAbility(AbilityOptions choice) {
		switch (choice) {
			default:
				return DirectAbility;
			case AbilityOptions.RandomDamage:
				return RandomDamage;
			case AbilityOptions.Kill:
				return Kill;
			case AbilityOptions.Boost:
				return Boost;
			case AbilityOptions.StealMana:
				return StealMana;
			case AbilityOptions.DrawCard:
				return DrawCards;
		}
	}
	#endregion

	/*
	big space to show that there's lots of differences in this code
	*/

	#region TargettingOptions
	//return opposing card
	static public PlayerData TargetOpposingCard(PlayerData current,
		PlayerData opposing, ref int index, ref Transform hit)
	{
		if (!(opposing.field[index].holding && opposing.field[index].holding.targetable)) {
			index = -2;
		}
		
		return opposing;
	}

	//return random card on the field
	static public PlayerData TargetOpposingField(PlayerData current,
		PlayerData opposing, ref int index, ref Transform hit)
	{
		/*
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

		return opposing;*/

		//check if hitting something
		if (hit) {
			//check if holder
			CardHolder holder = hit.GetComponent<CardHolder>();

			//check if holding a card if it's a holder
			if (holder != null && holder.holding) {

				//if targettign self, return error code
				if (holder == current.backLine[index]) {
					index = -2;
					return current;
				}
				//if targetable and valid field, just use it i guess :shrug:
				if (holder.holding.targetable && current.field[holder.index] == holder) {
					index = holder.index;
					return holder.playerData;
				}
			}
		}

		return null;
	}

	//return random card on the field
	static public PlayerData TargetSelfField(PlayerData current,
		PlayerData opposing, ref int index, ref Transform hit)
	{
		/*
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
		return current;*/

		//check if hitting something
		if (hit) {
			//check if holder
			CardHolder holder = hit.GetComponent<CardHolder>();

			//check if holding a card if it's a holder
			if (holder != null && holder.holding) {

				//if targettign self, return error code
				if (holder == current.backLine[index]) {
					index = -2;
					return current;
				}
				//if targetable and valid field, just use it i guess :shrug:
				if (holder.holding.targetable && current.field[holder.index] == holder) {
					index = holder.index;
					return holder.playerData;
				}
			}
		}

		return null;
	}

	//return opposing player
	static public PlayerData TargetPlayer(PlayerData current,
		PlayerData opposing, ref int index, ref Transform hit)
	{
		index = -1;
		return opposing;
	}

	//return self player
	static public PlayerData TargetSelfPlayer(PlayerData current,
		PlayerData opposing, ref int index, ref Transform hit)
	{
		index = -1;
		return current;
	}

	static public PlayerData TargetAnyCard(PlayerData current,
		PlayerData opposing, ref int index, ref Transform hit)
	{
		//check if hitting something
		if (hit) {
			//check if holder
			CardHolder holder = hit.GetComponent<CardHolder>();
			//check if holding a card if it's a holder
			if (holder != null && holder.holding) {
				//if targettign self, return error code
				if (holder == current.backLine[index]) {
					index = -2;
					return current;
				}
				//if targetable, just use it i guess :shrug:
				if (holder.holding.targetable) {
					//check the player, player or opposing?
					if (holder.playerData == current) {
						if (current.field[holder.index] == holder)
							index = holder.index;
						if (current.backLine[holder.index] == holder)
							index = holder.index + current.field.Count;
					}
					else {
						if (opposing.field[holder.index] == holder)
							index = holder.index;
						if (opposing.backLine[holder.index] == holder)
							index = holder.index + opposing.field.Count;
					}
					return holder.playerData;
				}
			}
		}
		return null;
	}

	static public PlayerData TargetAnyPlayerCard(PlayerData current,
		PlayerData opposing, ref int index, ref Transform hit)
	{
		//check if hitting something
		if (hit) {
			//check if holder
			CardHolder holder = hit.GetComponent<CardHolder>();
			
			//check if holding a card if it's a holder
			if (holder != null && holder.holding) {
				
				//if targettign self, return error code
				if (holder == current.backLine[index]) {
					index = -2;
					return current;
				}
				
				//if targetable and valid field, just use it i guess :shrug:
				if (holder.holding.targetable && holder.playerData == current) {
					if (current.field[holder.index] == holder)
						index = holder.index;
					if (current.backLine[holder.index] == holder)
						index = holder.index + current.field.Count;
					
					return holder.playerData;
				}
			}
		}
		return null;
	}

	static public PlayerData TargetAnyOpposingCard(PlayerData current,
		PlayerData opposing, ref int index, ref Transform hit)
	{
		//check if hitting something
		if (hit) {
			//check if holder
			CardHolder holder = hit.GetComponent<CardHolder>();
			//check if holding a card if it's a holder
			if (holder != null && holder.holding) {
				//if targettign self, return error code
				if (holder == current.backLine[index]) {
					index = -2;
					return current;
				}
				//if targetable and valid field, just use it i guess :shrug:
				if (holder.holding.targetable && holder.playerData == opposing) {
					if (opposing.field[holder.index] == holder)
						index = holder.index;
					if (opposing.backLine[holder.index] == holder)
						index = holder.index + opposing.field.Count;

					return holder.playerData;
				}
			}
		}
		return null;
	}
	#endregion

	#region ActivationOptions
	//just calls the ability once on the card target or player if index < 0
	static public void DirectActivation(SpellCard caster, PlayerData target, int index, SpellData spell) {
		//ability.Invoke(target, index, spell);
		Instantiate(spell.effect).PerformEffect(caster, target, index, 0);
		caster.SelfDestruct(spell.effect.GetCardDeathDelay(1));
	}

	//call the ability actionParameter1 times
	static public void RepeatedActivation(SpellCard caster, PlayerData target, int index, SpellData spell) {
		for (int i = 0; i < spell.actionParameter1; ++i) {
			//ability.Invoke(target, index, spell);
			Instantiate(spell.effect).PerformEffect(caster, target, index, i);
		}
		caster.SelfDestruct(spell.effect.GetCardDeathDelay(spell.actionParameter1));
	}

	//target a random card actionParameter1 times
	static public void RandomizedActivation(SpellCard caster, PlayerData target, int index, SpellData spell) {
		//doesn't work if targetting player
		if (index < 0)	return;

		//get number of cards
		int cards = 0;
		foreach (CardHolder holder in target.field) {
			if (holder.holding && holder.holding.targetable)
				++cards;
		}

		int i = 0;
		for (; i < spell.actionParameter1 && cards > 0;) {
			int j = Random.Range(0, target.field.Count);
			if (target.field[j].holding && target.field[j].holding.targetable) {
				//ability.Invoke(target, j, spell);
				Instantiate(spell.effect).PerformEffect(caster, target, j, i);
				if (!target.field[j].holding) {
					--cards;
				}
				++i;
			}
		}

		//kill the card
		caster.SelfDestruct(spell.effect.GetCardDeathDelay(i));
	}

	//targets everything, if activation is > 0, kill backrow too
	static public void EverythingActivation(SpellCard caster, PlayerData target, int index, SpellData spell) {
		int cardCount = 0;

		//target first
		foreach (CardHolder holder in target.field) {
			//ability.Invoke(target, index, spell);
			if (holder.holding && holder.holding.targetable) {
				Instantiate(spell.effect).PerformEffect(caster, target, holder.index, cardCount++);
			}
		}

		PlayerData opposing = target.field[0].opposingData;

		//then the opponent
		foreach (CardHolder holder in opposing.field) {
			//ability.Invoke(target, index, spell);
			if (holder.holding && holder.holding.targetable) {
				Instantiate(spell.effect).PerformEffect(caster, opposing, holder.index, cardCount++);
			}
		}

		if (spell.actionParameter1 > 0) {
			foreach (CardHolder holder in target.backLine) {
				//ability.Invoke(target, index, spell);
				if (holder.holding && holder.holding.targetable) {
					Instantiate(spell.effect).PerformEffect(caster, target, holder.index, cardCount++);
				}
			}

			foreach (CardHolder holder in opposing.backLine) {
				//ability.Invoke(target, index, spell);
				if (holder.holding && holder.holding.targetable) {
					Instantiate(spell.effect).PerformEffect(caster, opposing, holder.index, cardCount++);
				}
			}

		}
		//get back the spell mode thingy
		caster.SelfDestruct(spell.effect.GetCardDeathDelay(cardCount));
	}
	#endregion

	#region AbilityOptions
	//deals abilityParameter1 once
	static public void DirectAbility(PlayerData target, int index, SpellData spell) {
		//targetting player
		if (index < 0) {
			target.TakeDamage(spell.abilityParameter1);
			return;
		}

		MonsterCard card = null;
		if (index >= target.field.Count) {
			if(target.backLine[index - target.field.Count].holding) {
				card = (MonsterCard)target.backLine[index - target.field.Count].holding;
				if (card && card.targetable)
					card.TakeDamage(spell.abilityParameter1);
			}
		}
		else if (target.field[index].holding) {
			card = (MonsterCard)target.field[index].holding;
			if (card && card.targetable) {
				card.TakeDamage(spell.abilityParameter1);

				//if it killed the card, move backline
				if (!card.placement) {
					target.backLine[index].DoUpdate();
				}
			}
		}
	}

	//between abillityParameter1 inclusive and abilityParamter2 inclusive
	static public void RandomDamage(PlayerData target, int index, SpellData spell) {
		//targetting player
		if (index < 0) {
			target.TakeDamage(UnityEngine.Random.Range(
				spell.abilityParameter1, spell.abilityParameter2 + 1));
			return;
		}

		MonsterCard card = null;
		if (index >= target.field.Count) {
			if(target.backLine[index - target.field.Count].holding) {
				card = (MonsterCard)target.backLine[index - target.field.Count].holding;
				if (card && card.targetable)
					card.TakeDamage(UnityEngine.Random.Range(
						spell.abilityParameter1, spell.abilityParameter2 + 1));
			}
		}
		else if (target.field[index].holding) {
			card = (MonsterCard)target.field[index].holding;
			if (card && card.targetable) {
				card.TakeDamage(UnityEngine.Random.Range(
						spell.abilityParameter1, spell.abilityParameter2 + 1));

				//if it killed the card, move backline
				if (!card.placement) {
					target.backLine[index].DoUpdate();
				}
			}
		}

	}

	//all you need is kill
	static public void Kill(PlayerData target, int index, SpellData spell) {
		if (index < 0) {
			target.TakeDamage(target.currentHP);
			return;
		}

		MonsterCard card = null;
		if (index >= target.field.Count) {
			if(target.backLine[index - target.field.Count].holding) {
				card = (MonsterCard)target.backLine[index - target.field.Count].holding;
				if (card && card.targetable)
					card.TakeDamage(card.currHealth);
			}
		}
		else if (target.field[index].holding) {
			card = (MonsterCard)target.field[index].holding;
			if (card && card.targetable) {
				card.TakeDamage(card.currHealth);
						
				//if it killed the card, move backline
				if (!card.placement) {
					target.backLine[index].DoUpdate();
				}
			}
		}
		
	}

	//parameter 1 is duration, parameter 2 is hp, parameter 3 is atk
	static public void Boost(PlayerData target, int index, SpellData spell) {
		//dont work on players lol
		if (index < 0)	return;

		MonsterCard card = null;
		if (index >= target.field.Count) {
			if(target.backLine[index - target.field.Count].holding) {
				card = (MonsterCard)target.backLine[index - target.field.Count].holding;
				if (card && card.targetable)
					card.Boost(new MonsterCard.TempEffect(
						spell.abilityParameter1, spell.abilityParameter2, spell.abilityParameter3));
			}
		}
		else if (target.field[index].holding) {
			card = (MonsterCard)target.field[index].holding;
			if (card && card.targetable) {
				card.Boost(new MonsterCard.TempEffect(
					spell.abilityParameter1, spell.abilityParameter2, spell.abilityParameter3));
						
				//if it killed the card, move backline, yes boosts can kill
				if (!card.placement) {
					target.backLine[index].DoUpdate();
				}
			}
		}
	}

	//parameter 1 is mana amount (clamped to 0)
	static public void StealMana(PlayerData target, int index, SpellData spell) {
		//dont work on cards lol
		if (index >= 0)	return;
		target.StealMana(spell.abilityParameter1);
	}

	//parameter 1 is card amount (clamped to 0)
	static public void DrawCards(PlayerData target, int index, SpellData spell) {
		//dont work on cards lol
		if (index >= 0)	return;
		
		//render if client matches
		target.deck.AutoDrawCards(spell.abilityParameter1, 0.25f, ServerManager.CheckIfClient(target, true));
	}
#endregion

}
