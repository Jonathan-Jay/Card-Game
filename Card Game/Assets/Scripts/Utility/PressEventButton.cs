using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PressEventButton : MonoBehaviour
{
	public PlayerData player;
	//could set it automatically, but don't need to lol
	public int id = 0;
	public event Action pressed;

	public void Press() {
		pressed?.Invoke();
	}
}
