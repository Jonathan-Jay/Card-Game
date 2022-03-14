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
			p2mouse.ActivateEssentials();
		}
		else {
			p1mouse.ActivateEssentials();
			p2mouse.ActivateAll();
		}
		game.turnEnded += TurnEndPlayerChange;
		//*/
	}

	//late inits
	private void Start() {
		//disable the other player
		if (p1turn) {
			p1cam.enabled = true;
			p1mouse.disabled = false;
			p2cam.enabled = false;
			p2mouse.disabled = true;

			game.player2.turnEndButton.enabled = false;
		}
		else {
			p1cam.enabled = false;
			p1mouse.disabled = true;
			p2cam.enabled = true;
			p2mouse.disabled = false;

			game.player1.turnEndButton.enabled = false;
		}

		if (localMultiplayer) {
			updateFunc = LocalMulti;
			//show all cards
			//game.StartGame(p1turn, true, true);
			StartCoroutine(DelayedStart());
		}
	}

	IEnumerator DelayedStart() {
		yield return new WaitForSeconds(2f);
		game.StartGame(p1turn, true, true);
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
			p1mouse.ActivateEssentials();
			game.player1.turnEndButton.enabled = false;

			p2mouse.DeActivateEssentials();
			p2mouse.ActivateAll();
			game.player2.turnEndButton.enabled = true;
		}
		else {
			p1turn = true;
			p1mouse.DeActivateEssentials();
			p1mouse.ActivateAll();
			game.player1.turnEndButton.enabled = true;

			p2mouse.DeActivateAll();
			p2mouse.ActivateEssentials();
			game.player2.turnEndButton.enabled = false;
		}
	}
}
