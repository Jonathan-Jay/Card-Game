using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
	[System.Serializable]
	public class PlayerData {
		public int currentHP = 20;
		public int maxHP = 20;
		public int currentMana = 1;
		public int maxMana = 5;

		public List<CardHolder> field = new List<CardHolder>();
	}
	
	public int rowCount;
	[SerializeField] bool generateField = true;
	[SerializeField] CardHolder cardHolderPrefab;
	[SerializeField] PressEventButton turnEndButtonPrefab;
	[SerializeField] DeckManager deckPrefab;
	[SerializeField] Vector3 bellPos = Vector3.left;
	[SerializeField] Vector3 deckPos = Vector3.right;
	[SerializeField] float horizontalSeperation;
	[SerializeField] float verticalSeperation;

	public int maxMana = 5;
	public PlayerData player1 = new PlayerData();
	public PlayerData player2 = new PlayerData();


    // Start is called before the first frame update
    void Start()
    {
		if (generateField)
        	Generate();
    }

	void DoPlayer1Turn() {
		player2.currentHP -= Doturn(player1);;
	}
	void DoPlayer2Turn() {
		player1.currentHP -= Doturn(player2);
	}

	//return damage taken by opposing player
	int Doturn(PlayerData current) {
		int total = 0;
		foreach (CardHolder tile in current.field) {
			total += tile.DoUpdate();
		}
		current.currentMana = current.maxMana = Mathf.Clamp(++current.maxMana, 0, maxMana);

		return total;
	}

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

		Vector3 offset = Vector3.zero;
		offset.z = verticalSeperation * 0.5f;
		offset.x = horizontalSeperation * (rowCount - 1) * -0.5f;
		
		//spawn the deck (currently lets any player draw a card from either deck)
		Transform temp = null;
		temp = Instantiate(deckPrefab.gameObject, transform).transform;
		temp.localPosition = -deckPos;
		temp.localRotation = Quaternion.Euler(0f, 180f, 0f);
		//temp.gameObject.tag = "Player2";

		temp = Instantiate(deckPrefab.gameObject, transform).transform;
		temp.localPosition = deckPos;
		temp.localRotation = Quaternion.identity;
		//temp.gameObject.tag = "Player1";


		//spawn turn buttons
		temp = Instantiate(turnEndButtonPrefab.gameObject, transform).transform;
		temp.localPosition = -bellPos;
		temp.localRotation = Quaternion.Euler(0f, 180f, 0f);
		temp.GetComponent<PressEventButton>().pressed += DoPlayer2Turn;
		//temp.gameObject.tag = "Player2";

		temp = Instantiate(turnEndButtonPrefab.gameObject, transform).transform;
		temp.localPosition = bellPos;
		temp.localRotation = Quaternion.identity;
		temp.GetComponent<PressEventButton>().pressed += DoPlayer1Turn;
		//temp.gameObject.tag = "Player1";


		for (int i = 0; i < rowCount; ++i) {
			//instantiated to have matching row count
			temp = Instantiate(cardHolderPrefab.gameObject, transform).transform;
			temp.localPosition = offset;
			temp.localRotation = Quaternion.Euler(0f, 180f, 0f);
			//temp.gameObject.tag = "Player2";
			player2.field.Add(temp.GetComponent<CardHolder>());
			player2.field[i].index = i;
			player2.field[i].playerTag = "Player2";
			player2.field[i].playerData = player2;
			player2.field[i].opposingData = player1;
			offset.z *= -1f;

			temp = Instantiate(cardHolderPrefab.gameObject, transform).transform;
			temp.localPosition = offset;
			temp.localRotation = Quaternion.identity;
			//temp.gameObject.tag = "Player1";
			player1.field.Add(temp.GetComponent<CardHolder>());
			player1.field[i].index = i;
			player1.field[i].playerTag = "Player1";
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
