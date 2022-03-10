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

	public List<CardDataMultiplier> deck = new List<CardDataMultiplier>();
	public Card cardPrefab;
	public MonsterCard monsterPrefab;
	public SpellCard spellPrefab;
	public Vector3 spawnOffset = Vector3.up * 0.25f;
	public Vector3 spawnRotation = Vector3.left * 90f;
	public PlayerData player;

	GameObject temp;
	CardData data;
	Card card;

    public Transform DrawCard() {
		if (deck.Count == 0)	return null;

		//not good because doesn't take into account the amt of cards per type, could always remove the amt system
		int index = Random.Range(0, deck.Count);
		data = deck[index].data;
		if (--deck[index].amt <= 0) {
			deck.RemoveAt(index);
			if (deck.Count == 0) {
				GetComponentInChildren<MeshRenderer>().material.color = Color.black;
			}
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
