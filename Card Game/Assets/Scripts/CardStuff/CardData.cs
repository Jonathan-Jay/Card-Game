using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardData : ScriptableObject {
	public string cardName;
	public Texture2D cardArt;
	public int cost;
	public string flavourText;
	
	public virtual bool CheckCost(PlayerData player) {
		return true;
	}
	public virtual void Init() {}
}
