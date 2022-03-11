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

	public int maxMana = 5;
	public PlayerData player1;
	public PlayerData player2;
	public event System.Action turnEnded;

    void Awake() {
		if (generateField)
        	Generate();
		//get variables updated
    }

	void Start() {
		player1.Init();
		player2.Init();
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
		current.IncreaseMaxMana(Mathf.Clamp(++current.maxMana, 0, maxMana));

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

	//returns true on success
	bool AddCard(int index, bool isP1, MonsterCard card) {
		if (isP1) {
			return player1.field[index].PutCard(card);
		}
		else {
			return player2.field[index].PutCard(card);
		}
	}

	void Generate() {
		//clear current field
		Clear();

		Vector3 offset = Vector3.right * horizontalSeperation * rowCount * 0.5f;

		//spawn the deck (currently lets any player draw a card from either deck)
		Transform temp = null;
		temp = Instantiate(deckPrefab.gameObject, transform).transform;
		temp.localPosition = -(deckPos + (deckPos.x > 0 ? offset : -offset));
		temp.localRotation = Quaternion.Euler(0f, 180f, 0f);
		temp.GetComponent<DeckManager>().player = player2;
		//temp.gameObject.tag = "Player2";

		temp = Instantiate(deckPrefab.gameObject, transform).transform;
		temp.localPosition = deckPos + (deckPos.x > 0 ? offset : -offset);
		temp.localRotation = Quaternion.identity;
		temp.GetComponent<DeckManager>().player = player1;
		//temp.gameObject.tag = "Player1";


		//spawn turn buttons
		temp = Instantiate(turnEndButtonPrefab.gameObject, transform).transform;
		temp.localPosition = -(bellPos + (bellPos.x > 0 ? offset : -offset));
		temp.localRotation = Quaternion.Euler(0f, 180f, 0f);
		temp.GetComponent<PressEventButton>().player = player2;
		temp.GetComponent<PressEventButton>().pressed += DoPlayer2Turn;
		//temp.gameObject.tag = "Player2";

		temp = Instantiate(turnEndButtonPrefab.gameObject, transform).transform;
		temp.localPosition = bellPos + (bellPos.x > 0 ? offset : -offset);
		temp.localRotation = Quaternion.identity;
		temp.GetComponent<PressEventButton>().player = player1;
		temp.GetComponent<PressEventButton>().pressed += DoPlayer1Turn;
		//temp.gameObject.tag = "Player1";


		//spawn turn buttons
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
