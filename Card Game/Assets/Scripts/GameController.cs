using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
	public int rowCount;
	public CardHolder cardHolderPrefab;
	public PressEventButton turnEndButtonPrefab;
	[SerializeField] Vector3 bellPos = Vector3.left;
	public DeckManager deckPrefab;
	[SerializeField] Vector3 deckPos = Vector3.right;
	[SerializeField] float horizontalSeperation;
	[SerializeField] float verticalSeperation;
	List<CardHolder> player1Field = new List<CardHolder>();
	List<CardHolder> player2Field = new List<CardHolder>();


	public int player1HP = 100;
	public int player2HP = 100;


    // Start is called before the first frame update
    void Start()
    {
        Generate();
    }

	void DoPlayer1Turn() {
		player2HP -= Doturn(true);
	}
	void DoPlayer2Turn() {
		player1HP -= Doturn(false);
	}

	//return damage taken by opposing player
	int Doturn(bool player1) {
		List<CardHolder> opposing = player1 ? player2Field : player1Field;
		int total = 0;
		foreach (CardHolder tile in (player1 ? player1Field : player2Field)) {
			total += tile.DoUpdate(opposing);
		}
		return total;
	}

	//returns true on success
	bool AddCard(int index, bool player1, MonsterCard card) {
		if (player1) {
			return player1Field[index].PutCard(card);
		}
		else {
			return player2Field[index].PutCard(card);
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
			player2Field.Add(temp.GetComponent<CardHolder>());
			player2Field[i].index = i;
			player2Field[i].playerTag = "Player2";
			offset.z *= -1f;

			temp = Instantiate(cardHolderPrefab.gameObject, transform).transform;
			temp.localPosition = offset;
			temp.localRotation = Quaternion.identity;
			//temp.gameObject.tag = "Player1";
			player1Field.Add(temp.GetComponent<CardHolder>());
			player1Field[i].index = i;
			player1Field[i].playerTag = "Player1";
			offset.z *= -1f;
			offset.x += horizontalSeperation;
		}
	}

	void Clear() {
		//consider killing all cards as well
		player1Field.Clear();
		player2Field.Clear();
	}
}
