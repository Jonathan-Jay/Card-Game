using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
	public int rowCount;
	public GameObject cardHolderPrefab;
	[SerializeField] float horizontalSeperation;
	[SerializeField] float verticalSeperation;
	List<CardHolder> player1Field = new List<CardHolder>();
	List<CardHolder> player2Field = new List<CardHolder>();
	
    // Start is called before the first frame update
    void Start()
    {
        Generate();
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

	//returns true on sucess
	bool AddCard(int index, bool player1, Card card) {
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
		
		for (int i = 0; i < rowCount; ++i) {
			//instantiated to have matching row count
			player2Field.Add(Instantiate(cardHolderPrefab, offset, Quaternion.Euler(0f, 180f, 0f), transform)
				.GetComponent<CardHolder>());
			player2Field[i].index = i;
			offset.z *= -1f;

			player1Field.Add(Instantiate(cardHolderPrefab, offset, Quaternion.identity, transform)
				.GetComponent<CardHolder>());
			player1Field[i].index = i;
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
