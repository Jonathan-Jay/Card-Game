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

	public CardDataMultiplier[] deckOptions = new CardDataMultiplier[0];
	Stack<int> deck = new Stack<int>();
	[SerializeField]	Transform cardStack;
	[SerializeField]	BoxCollider col;
	[SerializeField]	float cardHeight;

	public Card cardPrefab;
	public MonsterCard monsterPrefab;
	public SpellCard spellPrefab;
	[SerializeField]	Vector3 spawnOffset = Vector3.up * 0.1f;
	[SerializeField]	Vector3 spawnRotation = Vector3.left * 90f;
	[SerializeField]	Vector3 defaultOffset = Vector3.up * 0.05f + Vector3.back * 0.5f;
	[SerializeField]	Vector3 defaultRotation = Vector3.forward * 180f;
	public PlayerData player;

	GameObject temp;
	CardData data;
	Card card;

	public void ShuffleDeck() {
		//only shuffle empty decks?
		//if (deck.Count > 0)	return;
		//deck.Clear();

		//if deckoptions is full of 0s, nothing happens
		int count = 0, index = 0;
		//empty deck, remaining cards stored in deckOptions
		deck.Clear();
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

		//fix the height
		cardStack.localScale = Vector3.one + Vector3.down * (1f - cardHeight * deck.Count);
		col.size = col.size + col.size.y * Vector3.down + Vector3.up * cardHeight * deck.Count;
		col.center = Vector3.up * cardHeight * deck.Count * 0.5f;
	}

    public Transform DrawCard(bool renderFace = true, bool faceDown = false, bool ignoreDrawLimit = false) {
		//dont draw if empty
		if (deck.Count == 0)	return null;
		//if not ignoreing draw limit, check canDraw
		if (!ignoreDrawLimit) {
			if (player.canDraw == 0)	return null;

			--player.canDraw;
			//do drawCard thing
			player.DrewCard();
		}

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
			cardStack.GetComponentInChildren<MeshRenderer>().material.color = Color.black;
		}
		else {
			cardStack.localScale = Vector3.one + Vector3.down * (1f - cardHeight * deck.Count);
			col.size = col.size + col.size.y * Vector3.down + Vector3.up * cardHeight * deck.Count;
			col.center = Vector3.up * cardHeight * deck.Count * 0.5f;
		}

		Vector3 pos = transform.position;
		Quaternion rot = transform.rotation;
		//use default rot
		if (faceDown) {
			pos += rot * defaultOffset + Vector3.up * col.size.y;
			rot *= Quaternion.Euler(defaultRotation);
		} else {
			pos += rot * spawnOffset + Vector3.up * col.size.y;
			rot *= Quaternion.Euler(spawnRotation);
		}

		if (data.GetType().Equals(typeof(MonsterData))) {
			temp = Instantiate(monsterPrefab.gameObject, pos, rot);
		}
		else if (data.GetType().Equals(typeof(SpellData))) {
			temp = Instantiate(spellPrefab.gameObject, pos, rot);
		}
		else {
			temp = Instantiate(cardPrefab.gameObject, pos, rot);
		}
		card = temp.GetComponent<Card>();
		card.SetData(data);
		card.player = player;

		if (renderFace) {
			card.RenderFace();
		}

		data = null;
		card = null;

		return temp.transform;
	}

	//automatically adds cards to the hand
	public void AutoDrawCards(int amt, float delay, bool renderFace = true) {
		if (deck.Count == 0)	return;
		//add cards to hand
		StartCoroutine(AutomaticallyDrawCards(amt, new WaitForSeconds(delay), renderFace));
	}

	IEnumerator AutomaticallyDrawCards(int amt, WaitForSeconds delay, bool renderFace) {
		Transform trans;
		for (int i = 0; i < amt; ++i) {
			//always ignore drawLimit and draw cards facing down
			trans = DrawCard(renderFace, true, true);
			//just in case
			if (trans) {
				trans.SetParent(player.hand.transform, true);

				//use hand manager to get target position and rotation maybe, or do this in there
				StartCoroutine(AnimateCard(trans, Vector3.zero, Quaternion.identity));
			}
			//break if empty deck
			if (deck.Count == 0) break;

			yield return delay;
		}
	}

	IEnumerator AnimateCard(Transform card, Vector3 targetPos, Quaternion targetRot) {
		card.gameObject.layer = player.hand.input.ignoredLayer;

		float returnSpeed = 2f;
		float rotSpeed =135f;
		card.GetComponent<Rigidbody>().isKinematic = true;

		while (card != null && (card.localPosition != targetPos || card.localRotation != targetRot)) {
			card.localPosition = Vector3.MoveTowards(card.localPosition, targetPos,
					returnSpeed * Time.deltaTime);
			card.localRotation = Quaternion.RotateTowards(card.localRotation, targetRot,
					rotSpeed * Time.deltaTime);
			yield return Card.eof;
		}

		if (card != null)
			card.gameObject.layer = player.hand.input.cardLayer;
	}
}
