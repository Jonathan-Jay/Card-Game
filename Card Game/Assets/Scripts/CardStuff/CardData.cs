using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardData : ScriptableObject {
	public Texture2D cardArt;
	public int cost;
	public virtual bool CheckCost(GameController.PlayerData player) {
		return true;
	}
}
