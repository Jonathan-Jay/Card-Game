using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{	
	public int rowCount;
	[SerializeField] bool generateField = true;
	[SerializeField] CardHolder cardHolderPrefab;
	[SerializeField] PressEventButton turnEndButtonPrefab;
	[SerializeField] Vector3 bellPos = Vector3.left;
	[SerializeField] DeckManager deckPrefab;
	[SerializeField] Vector3 deckPos = Vector3.right;
	[SerializeField] PlayerDataListener playerListenerPrefab;
	[SerializeField] Vector3 playerListenerPos = Vector3.right;
	[SerializeField] float horizontalSeperation;
	[SerializeField] float verticalSeperation;

	public int startingHandSize = 4;
	public int startingMana = 1;
	public int maxMana = 5;
	public int cardsPerTurn = 1;
	public PlayerData player1;
	public PlayerData player2;
	public event System.Action turnEnded;

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
		int dmg = Doturn(player1);
		if (dmg != 0)
			player2.TakeDamage(dmg);
	}
	void DoPlayer2Turn() {
		int dmg = Doturn(player2);
		if (dmg != 0)
			player1.TakeDamage(dmg);
	}

	//return damage taken by opposing player
	int Doturn(PlayerData current) {
		int total = 0;
		foreach (CardHolder tile in current.field) {
			total += tile.DoUpdate();
		}
		current.TurnEnd(maxMana, cardsPerTurn);

		turnEnded?.Invoke();

		return total;
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
			temp = Instantiate(cardHolderPrefab.gameObject, transform).transform;
			temp.localPosition = offset;
			temp.localRotation = Quaternion.Euler(0f, 180f, 0f);
			//temp.gameObject.tag = "Player2";
			player2.field.Add(temp.GetComponent<CardHolder>());
			player2.field[i].index = i;
			player2.field[i].playerData = player2;
			player2.field[i].opposingData = player1;
			offset.z *= -1f;

			temp = Instantiate(cardHolderPrefab.gameObject, transform).transform;
			temp.localPosition = offset;
			temp.localRotation = Quaternion.identity;
			//temp.gameObject.tag = "Player1";
			player1.field.Add(temp.GetComponent<CardHolder>());
			player1.field[i].index = i;
			player1.field[i].playerData = player1;
			player1.field[i].opposingData = player2;
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
