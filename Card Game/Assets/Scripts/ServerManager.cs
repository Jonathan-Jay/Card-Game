using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
	public GameController game;
	private Camera p1cam;
	public Mouse p1mouse;
	private Camera p2cam;
	public Mouse p2mouse;
	public bool localMultiplayer = false;
	public System.Action updateFunc;

	//store the current turn
	[SerializeField]
	bool p1turn = true;

	private void Awake() {
		p1cam = p1mouse.GetComponent<Camera>();
		p2cam = p2mouse.GetComponent<Camera>();
		
		//give them all the power
		/*
		p1mouse.ActivateAll();
		p2mouse.ActivateAll();
		/*/
		if (p1turn) {
			p1mouse.ActivateAll();
			p2mouse.ActivateCard();
		}
		else {
			p1mouse.ActivateCard();
			p2mouse.ActivateAll();
		}
		game.turnEnded += TurnEndPlayerChange;
		//*/
		
		//disable the other player
		if (p1turn) {
			p1cam.enabled = true;
			p1mouse.disabled = false;
			p2cam.enabled = false;
			p2mouse.disabled = true;
		}
		else {
			p1cam.enabled = false;
			p1mouse.disabled = true;
			p2cam.enabled = true;
			p2mouse.disabled = false;
		}

		if (localMultiplayer) {
			updateFunc = LocalMulti;
		}
	}

    // Update is called once per frame
    void Update()
    {
		updateFunc?.Invoke();
    }

	void LocalMulti() {
		if (Input.GetKeyDown(KeyCode.Backspace)) {
			//toggle cameras
			if (p1cam.enabled) {
				p1cam.enabled = false;
				p1mouse.disabled = true;
				p2cam.enabled = true;
				p2mouse.disabled = false;
			}
			else {
				p1cam.enabled = true;
				p1mouse.disabled = false;
				p2cam.enabled = false;
				p2mouse.disabled = true;
			}
			LookAt.ForceUpdateCamera?.Invoke();
		}
	}

	void TurnEndPlayerChange() {
		//check current player, then toggle them
		if (p1turn) {
			p1turn = false;
			p1mouse.DeActivateAll();
			p1mouse.ActivateCard();
			p2mouse.DeActivateCard();
			p2mouse.ActivateAll();
		}
		else {
			p1turn = true;
			p1mouse.DeActivateCard();
			p1mouse.ActivateAll();
			p2mouse.DeActivateAll();
			p2mouse.ActivateCard();
		}
	}
}
