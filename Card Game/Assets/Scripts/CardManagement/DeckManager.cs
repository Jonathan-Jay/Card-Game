using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
	[System.Serializable]
	public class CardDataMultiplier {
		public int amt;
		public CardData data;
	}

	public List<CardDataMultiplier> deckOptions = new List<CardDataMultiplier>();
	Stack<int> deck = new Stack<int>();
	public Card cardPrefab;
	public MonsterCard monsterPrefab;
	public SpellCard spellPrefab;
	public Vector3 spawnOffset = Vector3.up * 0.25f;
	public Vector3 spawnRotation = Vector3.left * 90f;
	public PlayerData player;

	GameObject temp;
	CardData data;
	Card card;

	private void Start() {
		ShuffleDeck();
	}

	void ShuffleDeck() {
		//only shuffle empty decks?
		//if (deck.Count > 0)	return;
		//deck.Clear();

		//if deckoptions is full of 0s, nothing happens
		int count = 0, index = 0;
		List<int> cards = new List<int>();
		foreach (CardDataMultiplier data in deckOptions) {
			count += data.amt;
			for (int i = 0; i < data.amt; ++i) {
				cards.Add(index);
			}
			++index;
		}
		for (int i = 0; i < count; ++i) {
			index = Random.Range(0, cards.Count);
			deck.Push(cards[index]);
			cards.RemoveAt(index);
		}
	}

    public Transform DrawCard() {
		if (deck.Count == 0)	return null;
		/*if (deckOptions.Count == 0)	return null;

		//not good because doesn't take into account the amt of cards per type, could always remove the amt system
		int index = Random.Range(0, deckOptions.Count);
		data = deckOptions[index].data;
		if (--deckOptions[index].amt <= 0) {
			deckOptions.RemoveAt(index);
			if (deckOptions.Count == 0) {
				GetComponentInChildren<MeshRenderer>().material.color = Color.black;
			}
		}*/
		int index = deck.Pop();
		data = deckOptions[index].data;
		//if you shuffle what's remaining
		--deckOptions[index].amt;

		if (deck.Count == 0) {
			GetComponentInChildren<MeshRenderer>().material.color = Color.black;
		}

		if (data.GetType().Equals(typeof(MonsterData))) {
			temp = Instantiate(monsterPrefab.gameObject, transform.position + spawnOffset, transform.rotation * Quaternion.Euler(spawnRotation));
		}
		else if (data.GetType().Equals(typeof(SpellData))) {
			temp = Instantiate(spellPrefab.gameObject, transform.position + spawnOffset, transform.rotation * Quaternion.Euler(spawnRotation));
		}
		else {
			temp = Instantiate(cardPrefab.gameObject, transform.position + spawnOffset, transform.rotation * Quaternion.Euler(spawnRotation));
		}
		card = temp.GetComponent<Card>();
		card.SetData(data);
		card.player = player;

		data = null;
		card = null;

		return temp.transform;
	}
}
