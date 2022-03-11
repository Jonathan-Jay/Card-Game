using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PressEventButton : MonoBehaviour
{
	public event Action pressed;
	public PlayerData player;

	public void Press() {
		pressed.Invoke();
	}
}
