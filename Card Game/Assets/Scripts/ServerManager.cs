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
	public bool p1turn = true;
	[SerializeField]	KeyCode swapCam;

	MeshRenderer p1bell;
	MeshRenderer p2bell;
	Color defaultBellCol;

	private void Awake() {
		p1cam = p1mouse.GetComponent<Camera>();
		p2cam = p2mouse.GetComponent<Camera>();
		
		//give them all the power
		/*
		p1mouse.ActivateAll();
		p2mouse.ActivateAll();
		/*/
		game.turnEnded += TurnEndPlayerChange;
		//*/
	}

	//late inits
	private void Start() {
		p1bell = game.player1.turnEndButton.GetComponentInChildren<MeshRenderer>();
		p2bell = game.player2.turnEndButton.GetComponentInChildren<MeshRenderer>();

		defaultBellCol = p1bell.material.color;

		//disable the other player
		if (p1turn) {
			p1cam.enabled = true;
			p1mouse.disabled = false;
			p2cam.enabled = false;
			p2mouse.disabled = true;

			game.player2.turnEndButton.enabled = false;
			p2bell.material.color = Color.grey;
		}
		else {
			p1cam.enabled = false;
			p1mouse.disabled = true;
			p2cam.enabled = true;
			p2mouse.disabled = false;

			game.player1.turnEndButton.enabled = false;
			p1bell.material.color = Color.grey;
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
		game.StartGame(p1turn, true, false);
		if (p1turn) {
			p2mouse.ActivateEssentials();
			if (game.player1.canDraw > 0) {
				p1mouse.ActivateDeck();
			}
			else {
				p1mouse.ActivateAll();
			}
		}
		else {
			p1mouse.ActivateEssentials();
			if (game.player2.canDraw > 0) {
				p2mouse.ActivateDeck();
			}
			else {
				p2mouse.ActivateAll();
			}
		}
	}

    // Update is called once per frame
    void Update()
    {
		updateFunc?.Invoke();
    }

	void LocalMulti() {
		if (Input.GetKeyDown(swapCam)) {
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
			p1bell.material.color = Color.grey;

			p2mouse.DeActivateEssentials();
			if (game.player2.canDraw > 0)
				p2mouse.ActivateDeck();
			else
				p2mouse.ActivateAll();

			game.player2.turnEndButton.enabled = true;
			p2bell.material.color = defaultBellCol;

			//disable all of p1's cards that aren't on the field and revel p2's cards
			foreach(Card card in FindObjectsOfType<Card>()) {
				if (!card.placement) {
					if (card.player == game.player1)
						card.HideFace();
					else
						card.RenderFace();
				}
			}
		}
		else {
			p1turn = true;
			p1mouse.DeActivateEssentials();
			if (game.player1.canDraw > 0)
				p1mouse.ActivateDeck();
			else
				p1mouse.ActivateAll();
			game.player1.turnEndButton.enabled = true;
			p1bell.material.color = defaultBellCol;

			p2mouse.DeActivateAll();
			p2mouse.ActivateEssentials();
			game.player2.turnEndButton.enabled = false;
			p2bell.material.color = Color.grey;

			//disable all of p2's cards that aren't on the field and revel p1's cards
			foreach (Card card in FindObjectsOfType<Card>()) {
				if (!card.placement) {
					if (card.player == game.player2)
						card.HideFace();
					else
						card.RenderFace();
				}
			}
		}
	}

	static public bool CheckIfClient(PlayerData player) {
		return false;
	}
}
