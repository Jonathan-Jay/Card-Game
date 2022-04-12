using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour {
	public string playerTag;
	public int currentHP = 20;
	public int maxHP = 20;
	public int currentMana = 1;
	public int maxMana = 1;
	public HandManager hand;
	public List<CardAttacker> field = new List<CardAttacker>();
	public List<CardMover> backLine = new List<CardMover>();
	public DeckManager deck;
	public PressEventButton turnEndButton;
	public int canDraw = 0;

	public List<Card> heldCards {get; private set;} = new List<Card>();

	//sends old value
	public event System.Action<int> healthUpdated;
	//sends old value
	public event System.Action<int> manaUpdated;
	public event System.Action drawCard;
	public event System.Action startOfTurn;
	public event System.Action endOfTurn;

	public AudioQueue damageAudioPlayer;
	public AudioQueue healAudioPlayer;

	private void Awake() {
		healthUpdated += TakeDamageSound;
	}

	public void Init(int startingMana, int cardsPerTurn) {
		currentMana = startingMana;
		canDraw = cardsPerTurn;
		
		healthUpdated?.Invoke(0);
		manaUpdated?.Invoke(0);
		drawCard?.Invoke();
	}

	public void AddCard(Card card) {
		heldCards.Add(card);
	}

	public void RemoveCard(Card card) {
		heldCards.Remove(card);

		if (card.placement) {
			lastPlayedIndex = card.placement.index;
		}
	}

	public int lastPlayedIndex = -1;

	public void DrawCard() {
		drawCard?.Invoke();
	}

	public void StartOfTurn() {
		startOfTurn?.Invoke();
	}

	//do whatever a player does when their turn ends
	public void TurnEnd(int maxMana, int cardsPerTurn, int requiredCards, int maxCards) {
		IncreaseMaxMana(Mathf.Clamp(++this.maxMana, 0, maxMana));
		
		//we do this even if the deck is empty in case the player bugs the game out
		canDraw = Mathf.Min(cardsPerTurn, deck.deck.Count);

		//we store the owned cards in cardsheld
		if (heldCards.Count < requiredCards) {
			canDraw = Mathf.Min(requiredCards - heldCards.Count + cardsPerTurn, deck.deck.Count);
		}

		//drawing too many cards, canDraw is the difference
		if (heldCards.Count + canDraw > maxCards) {
			//if they bug this out
			canDraw = Mathf.Max(maxCards - heldCards.Count, 0);
		}

		endOfTurn?.Invoke();

		//clamp this
		drawCard?.Invoke();
	}

	//returns true if they died
	public bool FatigueCheck(int fatigueDmg) {
		//only fatigue if empty deck
		if (canDraw == 0 && deck.deck.Count == 0) {
			TakeDamage(fatigueDmg);

			return currentHP <= 0;
		}
		return false;
	}

	//works when inverted
	public void TakeDamage(int amt) {
		currentHP -= amt;
		healthUpdated?.Invoke(currentHP + amt);
	}

	//clamps at 0
	public void StealMana(int amt) {
		int oldMana = currentMana;
		currentMana = Mathf.Max(currentMana - amt, 0);
		manaUpdated?.Invoke(oldMana);
	}
	
	//return true if player has enough mana
	public bool ReduceMana(int amt) {
		//can't reduce below 0
		if (currentMana >= amt) {
			int oldMana = currentMana;
			currentMana -= amt;
			if (currentMana > maxMana)
				currentMana = maxMana;
			//to subscribe to the event they need a reference to it, so dont need to send current
			manaUpdated?.Invoke(oldMana);
			return true;
		}
		return false;
	}

	public void IncreaseMaxMana(int newMax, bool refillMana = true) {
		maxMana = newMax;
		if (refillMana) {
			int oldMana = currentMana;
			currentMana = maxMana;
			if (oldMana != currentMana)
				manaUpdated?.Invoke(oldMana);
		}
	}

	void TakeDamageSound(int prev) {
		//prev is bigger, we lost health
		if (prev > currentHP) {
			healAudioPlayer?.Play();
		}
		//curr is bigger, we healed
		else if (currentHP > prev) {
			damageAudioPlayer?.Play();
		}
		//can add another else for hp didnt change (aka a dud)
	}
}
