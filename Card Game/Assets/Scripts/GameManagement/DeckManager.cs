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
	public Stack<int> deck = new Stack<int>();
	//public int[] viewableDeck;

	[SerializeField]	Transform cardStack;
	[SerializeField]	BoxCollider col;
	[SerializeField]	TMPro.TMP_Text text;
	[SerializeField]	float cardHeight;

	public MonsterCard monsterPrefab;
	public SpellCard spellPrefab;
	[SerializeField]	Vector3 spawnOffset = Vector3.up * 0.1f;
	[SerializeField]	Vector3 spawnRotation = Vector3.left * 90f;
	[SerializeField]	Vector3 defaultOffset = Vector3.up * 0.05f + Vector3.back * 0.5f;
	[SerializeField]	Vector3 defaultRotation = Vector3.forward * 180f;
	public PlayerData player;

	CardData data;
	Card card;

	[SerializeField]	bool autoShuffle = false;
	[SerializeField]	List<int> riggedDeck = new List<int>();

	AudioQueue audioPlayer;
	private void Start() {
		audioPlayer = GetComponent<AudioQueue>();
		if (autoShuffle) {
			ShuffleDeck();
		}
		else if (riggedDeck.Count > 0) {
			List<int> counter = new List<int>(deckOptions.Length);
			foreach (CardDataMultiplier data in deckOptions) {
				counter.Add(data.amt);
			}
			for (int i = riggedDeck.Count - 1; i >= 0; --i) {
				//attempt to fill the deck, but not more than it has
				if (--counter[riggedDeck[i]] >= -1)
					deck.Push(riggedDeck[i]);
			}
		}
		//don't store in memory
		riggedDeck = null;
	}

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
		col.center = cardStack.localPosition + Vector3.up * cardHeight * deck.Count * 0.5f;
		text.transform.localPosition = cardStack.localPosition + Vector3.up * (cardHeight * deck.Count + 0.025f);
		text.text = deck.Count.ToString();

		//viewableDeck = deck.ToArray();
	}

	//order should be left = top, right = bottom
	public void FromArray(int[] arr) {
		//for debugging purposes
		//for (int i = 0; i < arr.Length; ++i) {
		for (int i = arr.Length - 1; i >= 0; --i) {
			deck.Push(arr[i]);
		}

		//fix the height
		cardStack.localScale = Vector3.one + Vector3.down * (1f - cardHeight * deck.Count);
		col.size = col.size + col.size.y * Vector3.down + Vector3.up * cardHeight * deck.Count;
		col.center = cardStack.localPosition + Vector3.up * cardHeight * deck.Count * 0.5f;
		text.transform.localPosition = cardStack.localPosition + Vector3.up * (cardHeight * deck.Count + 0.025f);
		text.text = deck.Count.ToString();

		//viewableDeck = deck.ToArray();
	}

	public Transform DrawCard(bool renderFace = true, bool faceDown = false, bool ignoreDrawLimit = false) {
		//dont draw if empty
		if (deck.Count == 0)	return null;
		//if not ignoring draw limit, check canDraw
		if (!ignoreDrawLimit) {
			if (player.canDraw == 0)	return null;

			--player.canDraw;
			//do drawCard thing
			player.DrawCard();
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
			col.center = cardStack.localPosition + Vector3.up * cardHeight * deck.Count * 0.5f;
			text.transform.localPosition = cardStack.localPosition + Vector3.up * (cardHeight * deck.Count + 0.025f);
		}
		text.text = deck.Count.ToString();

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
			card = Instantiate(monsterPrefab, pos, rot);
		}
		else if (data.GetType().Equals(typeof(SpellData))) {
			card = Instantiate(spellPrefab, pos, rot);
		}
		else {
			//invalid
			card = null;
			data = null;

			return null;
		}
		card.SetData(data);
		card.player = player;

		player.AddCard(card);

		if (renderFace) {
			card.RenderFace();
		}

		data = null;

		//play sound
		audioPlayer?.PlayRandom();

		return card.transform;
	}

	public static WaitForSeconds pointFive = new WaitForSeconds(0.4f);
	public void FirstDraw(bool renderFace) {
		if (player.canDraw == 0) {
			player.hand.input.ActivateAll();
			return;
		}

		//give ability to click stuff
		//player.hand.input.DeActivateDeck();
		//player.hand.input.ActivateAll();

		if (deck.Count == 0)	return;
		StartCoroutine(AutomaticallyDrawCards(player.canDraw, pointFive, false, renderFace, true));
	}

	//automatically adds cards to the hand
	public void AutoDrawCards(int amt, float delay, bool renderFace = true) {
		if (deck.Count == 0)	return;
		//add cards to hand
		StartCoroutine(AutomaticallyDrawCards(amt, new WaitForSeconds(delay), true, renderFace, false));
	}

	IEnumerator AutomaticallyDrawCards(int amt, WaitForSeconds delay,
		bool ignoreDrawLimit, bool renderFace, bool activateInputs)
	{
		if (!ignoreDrawLimit)
			yield return delay;
		
		Transform trans;
		for (int i = 0; i < amt; ++i) {
			//always ignore drawLimit and draw cards facing down
			trans = DrawCard(renderFace, true, ignoreDrawLimit);
			//just in case
			//if (trans) {
				trans?.GetComponent<Card>().CallBackCard();
			//}
			//break if empty deck
			if (deck.Count == 0) break;

			yield return delay;
		}

		//activate inputs on first draw
		if (activateInputs) {
			player.hand.input.ActivateAll();
		}
	}
}
