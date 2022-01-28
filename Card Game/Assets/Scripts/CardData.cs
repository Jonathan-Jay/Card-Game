using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "CardData", order = 0)]
public class CardData : ScriptableObject {
	public Texture2D cardArt;
	public int health;
	public int attack;
	public int cost;
}
