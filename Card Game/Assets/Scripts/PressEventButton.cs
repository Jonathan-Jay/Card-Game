using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PressEventButton : MonoBehaviour
{
	public event Action pressed;
	public PlayerData player;
	public bool anyPlayer = false;

	public void Press() {
		pressed.Invoke();
	}
}
