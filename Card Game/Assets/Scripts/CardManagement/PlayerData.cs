using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour {
	public string playerTag = "";
	public int currentHP = 20;
	public int maxHP = 20;
	public int currentMana = 1;
	public int maxMana = 1;
	public HandManager hand = null;
	public List<CardHolder> field = new List<CardHolder>();

	//sends old value
	public event System.Action<int> healthUpdated;
	//sends old value
	public event System.Action<int> manaUpdated;

	public void Init()
	{
		healthUpdated?.Invoke(0);
		manaUpdated?.Invoke(0);
	}

	//works when inverted
	public void TakeDamage(int amt)
	{
		currentHP -= amt;
		healthUpdated?.Invoke(currentHP + amt);
	}

	//return true if player has enough mana
	public bool ReduceMana(int amt)
	{
		//can't reduce below 0
		if (currentMana >= amt)
		{
			int oldMana = currentMana;
			currentMana -= amt;
			if (currentMana > maxMana)
				currentMana = maxMana;
			//to subscribe to the event they need a reference to it, so dont need to send current
			manaUpdated?.Invoke(oldMana);
			return true;
		}
		return false;
	}

	public void IncreaseMaxMana(int newMax, bool refillMana = true)
	{
		maxMana = newMax;
		if (refillMana)
		{
			int oldMana = currentMana;
			currentMana = maxMana;
			if (oldMana != currentMana)
				manaUpdated?.Invoke(oldMana);
		}
	}
}
