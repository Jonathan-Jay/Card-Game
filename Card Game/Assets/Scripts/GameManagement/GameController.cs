using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{	
	public int rowCount;
	[SerializeField] bool generateField = true;
	[SerializeField] CardHolder cardAttackerPrefab;
	[SerializeField] CardHolder cardMoverPrefab;
	[SerializeField] PressEventButton turnEndButtonPrefab;
	[SerializeField] Vector3 bellPos = Vector3.left;
	[SerializeField] DeckManager deckPrefab;
	[SerializeField] Vector3 deckPos = Vector3.right;
	[SerializeField] PlayerDataListener playerListenerPrefab;
	[SerializeField] Vector3 playerListenerPos = Vector3.right;
	[SerializeField] float horizontalSeperation;
	[SerializeField] float verticalSeperation;
	[SerializeField] float moverSeperation;

	public int startingHandSize = 4;
	public int startingMana = 1;
	public int maxMana = 5;
	public int cardsPerTurn = 1;
	public int minCardsInHand = 3;
	public int maxCardsInHand = 40;
	public int fatigueDmg = 1;
	public PlayerData player1;
	public PlayerData player2;
	public event System.Action turnEnded;
	public event System.Action<PlayerData> playerWon;

    void Awake() {
		if (generateField)
        	Generate();
    }

	public void StartGame(bool p1Starts, bool renderFirst, bool renderSecond) {
		PlayerData first = p1Starts ? player1 : player2;
		PlayerData second = p1Starts ? player2 : player1;

		//update player
		first.Init(startingMana, cardsPerTurn);
		second.Init(startingMana, cardsPerTurn);

		//update bells
		player1.turnEndButton.pressed += DoPlayer1Turn;
		player2.turnEndButton.pressed += DoPlayer2Turn;

		//shuffle decks
		first.deck.ShuffleDeck();
		second.deck.ShuffleDeck();

		//fill hands
		first.deck.AutoDrawCards(startingHandSize, 0.25f, renderFirst);
		second.deck.AutoDrawCards(startingHandSize, 0.25f, renderSecond);
	}

	void DoPlayer1Turn() {
		StartCoroutine(Doturn(player1, player2));
	}
	void DoPlayer2Turn() {
		StartCoroutine(Doturn(player2, player1));
	}

	bool firstTurn = true;
	WaitForSeconds turnDelay = new WaitForSeconds(0.25f);
	WaitForSeconds phaseDelay = new WaitForSeconds(0.5f);

	//return damage taken by opposing player
	IEnumerator Doturn(PlayerData current, PlayerData opposing) {
		current.hand.input.DeActivateAll();

		MonsterCard temp = null;
		int counter = 0;
		//move cards
		foreach (CardHolder tile in current.backLine) {
			if (tile.holding && tile.holding.targetable)
				temp = (MonsterCard)tile.holding;

			tile.DoUpdate();

			//only delay if was holding a card
			if (temp && !tile.holding) {
				++counter;
				yield return turnDelay;
			}
			temp = null;
		}

		if (!firstTurn) {
			if (counter > 0) {
				//extra delay if there were moved cards
				yield return phaseDelay;
				counter = 0;
			}

			//attack cards
			foreach (CardHolder tile in current.field) {
				if (tile.holding && tile.holding.targetable)
					temp = (MonsterCard)tile.holding;

				tile.DoUpdate();

				//only delay if has a card and can attack
				if (temp && temp.currAttack > 0) {
					++counter;
					yield return turnDelay;
				}
				temp = null;
			}
		}
		else {
			firstTurn = false;
			//update boosts without attacking
			foreach (CardHolder tile in current.field) {
				temp = (MonsterCard)tile.holding;
				if (temp && temp.targetable) {
					temp.UpdateBoosts();
				}
			}
		}

		if (counter > 0) {
			//extra delay
			yield return phaseDelay;
		}

		//perform boost update for defending cards
		foreach(CardHolder tile in opposing.field) {
			temp = (MonsterCard)tile.holding;
			if (temp && temp.targetable) {
				temp.UpdateBoosts();
			}
		}
		foreach(CardHolder tile in opposing.backLine) {
			temp = (MonsterCard)tile.holding;
			if (temp && temp.targetable) {
				temp.UpdateBoosts();
			}
		}

		current.TurnEnd(maxMana, cardsPerTurn, minCardsInHand, maxCardsInHand);
		current.hand.input.ActivateAll();

		//possibly deactivate if the player won
		if (opposing.currentHP > 0) {
			turnEnded?.Invoke();
		}
		else {
			//what happens when a player wins
			playerWon?.Invoke(current);
			yield break;
		}

		//check the fatigue of the current player
		if (opposing.FatigueCheck(fatigueDmg)) {
			//what happens if opponent dies on first turn
			playerWon?.Invoke(current);
		}
	}

	//display current turn?
	/*
	private void OnEnable() {
		turnEnded += TurnEnd;
	}

	private void OnDisable() {
		turnEnded -= TurnEnd;
	}

	void TurnEnd() {
		
	}*/

	void Generate() {
		//clear current field
		Clear();

		Vector3 offset = Vector3.right * horizontalSeperation * rowCount * 0.5f;

		//spawn the deck
		Transform temp = null;
		temp = Instantiate(deckPrefab.gameObject, transform).transform;
		temp.localPosition = -(deckPos + (deckPos.x > 0 ? offset : -offset));
		temp.localRotation = Quaternion.Euler(0f, 180f, 0f);
		player2.deck = temp.GetComponent<DeckManager>();
		player2.deck.player = player2;
		//temp.gameObject.tag = "Player2";

		temp = Instantiate(deckPrefab.gameObject, transform).transform;
		temp.localPosition = deckPos + (deckPos.x > 0 ? offset : -offset);
		temp.localRotation = Quaternion.identity;
		player1.deck = temp.GetComponent<DeckManager>();
		player1.deck.player = player1;
		//temp.gameObject.tag = "Player1";



		//spawn turn buttons
		temp = Instantiate(turnEndButtonPrefab.gameObject, transform).transform;
		temp.localPosition = -(bellPos + (bellPos.x > 0 ? offset : -offset));
		temp.localRotation = Quaternion.Euler(0f, 180f, 0f);
		player2.turnEndButton = temp.GetComponent<PressEventButton>();
		player2.turnEndButton.player = player2;
		//temp.gameObject.tag = "Player2";

		temp = Instantiate(turnEndButtonPrefab.gameObject, transform).transform;
		temp.localPosition = bellPos + (bellPos.x > 0 ? offset : -offset);
		temp.localRotation = Quaternion.identity;
		player1.turnEndButton = temp.GetComponent<PressEventButton>();
		player1.turnEndButton.player = player1;
		//temp.gameObject.tag = "Player1";



		//spawn the health and mana things
		temp = Instantiate(playerListenerPrefab.gameObject, transform).transform;
		temp.GetComponent<PlayerDataListener>().SetTarget(player2);
		temp.localPosition = -(playerListenerPos + (playerListenerPos.x > 0 ? offset : -offset));
		temp.localRotation = Quaternion.Euler(0f, 180f, 0f);
		//temp.gameObject.tag = "Player2";

		temp = Instantiate(playerListenerPrefab.gameObject, transform).transform;
		temp.GetComponent<PlayerDataListener>().SetTarget(player1);
		temp.localPosition = playerListenerPos + (playerListenerPos.x > 0 ? offset : -offset);
		temp.localRotation = Quaternion.identity;
		//temp.gameObject.tag = "Player1";

		offset = Vector3.zero;
		offset.z = verticalSeperation * 0.5f;
		offset.x = horizontalSeperation * (rowCount - 1) * -0.5f;

		for (int i = 0; i < rowCount; ++i) {
			//instantiated to have matching row count
			temp = Instantiate(cardAttackerPrefab.gameObject, transform).transform;
			temp.localPosition = offset;
			temp.localRotation = Quaternion.Euler(0f, 180f, 0f);
			//temp.gameObject.tag = "Player2";
			player2.field.Add(temp.GetComponent<CardAttacker>());
			player2.field[i].index = i;
			player2.field[i].playerData = player2;
			player2.field[i].opposingData = player1;

			//instantiated to have matching row count
			temp = Instantiate(cardMoverPrefab.gameObject, transform).transform;
			temp.localPosition = offset + Vector3.forward * moverSeperation;
			temp.localRotation = Quaternion.Euler(0f, 180f, 0f);
			//temp.gameObject.tag = "Player2";
			player2.backLine.Add(temp.GetComponent<CardMover>());
			player2.backLine[i].index = i;
			player2.backLine[i].playerData = player2;
			player2.backLine[i].opposingData = player1;
			player2.backLine[i].moveTo = player2.field[i];
			offset.z *= -1f;


			//player 1
			temp = Instantiate(cardAttackerPrefab.gameObject, transform).transform;
			temp.localPosition = offset;
			temp.localRotation = Quaternion.identity;
			//temp.gameObject.tag = "Player1";
			player1.field.Add(temp.GetComponent<CardAttacker>());
			player1.field[i].index = i;
			player1.field[i].playerData = player1;
			player1.field[i].opposingData = player2;

			temp = Instantiate(cardMoverPrefab.gameObject, transform).transform;
			temp.localPosition = offset + Vector3.back * moverSeperation;
			temp.localRotation = Quaternion.identity;
			//temp.gameObject.tag = "Player1";
			player1.backLine.Add(temp.GetComponent<CardMover>());
			player1.backLine[i].index = i;
			player1.backLine[i].playerData = player1;
			player1.backLine[i].opposingData = player2;
			player1.backLine[i].moveTo = player1.field[i];
			offset.z *= -1f;
			offset.x += horizontalSeperation;
		}
	}

	void Clear() {
		//consider killing all cards as well
		player1.field.Clear();
		player2.field.Clear();
	}
}
