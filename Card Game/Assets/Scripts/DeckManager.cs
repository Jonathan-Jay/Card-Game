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
	public Vector3 spawnOffset = Vector3.up * 0.25f;

	GameObject temp;
	CardData data;

    public Transform DrawCard() {
		if (deck.Count == 0)	return null;

		temp = Instantiate(cardPrefab.gameObject, transform.position + spawnOffset, transform.rotation);

		//not good because doesn't take into account the amt of cards per type, could always remove the amt system
		int index = Random.Range(0, deck.Count);
		data = deck[index].data;
		if (--deck[index].amt <= 0) {
			deck.RemoveAt(index);
			if (deck.Count == 0) {
				GetComponentInChildren<MeshRenderer>().material.color = Color.black;
			}
		}

		temp.GetComponent<Card>().SetData(data);
		data = null;

		return temp.transform;
	}
}
