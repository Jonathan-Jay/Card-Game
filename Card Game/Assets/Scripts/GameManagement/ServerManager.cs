using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
	public GameController game;
	public Mouse p1mouse;
	public Mouse p2mouse;
	private Camera p1cam;
	private Camera p2cam;
	private event System.Action updateFunc;
	public static bool localMultiplayer = true;

	//store the current turn
	public static bool p1turn = true;
	[SerializeField]	KeyCode swapCam;
	[SerializeField]	KeyCode pauseButton;
	[SerializeField]	GameObject pauseScreen;
	[SerializeField]	GameObject networkedPauseButtons;

	MeshRenderer p1bell;
	MeshRenderer p2bell;
	Color defaultBellCol;

	private void Awake() {
		pauseScreen.SetActive(false);

		p1cam = p1mouse.GetComponent<Camera>();
		p2cam = p2mouse.GetComponent<Camera>();
	}

	//late inits
	private void Start() {
		p1bell = game.player1.turnEndButton.GetComponentInChildren<MeshRenderer>();
		p2bell = game.player2.turnEndButton.GetComponentInChildren<MeshRenderer>();

		defaultBellCol = p1bell.material.color;

		if (localMultiplayer) {
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
			networkedPauseButtons.SetActive(false);

			//has to do lots of input controlling
			updateFunc += LocalMulti;
			//this one is local only as it hides faces and handles pausing differently
			game.turnEnded += LocalTurnEndPlayerChange;
			
			//show all cards
			//game.StartGame(p1turn, true, true);
			StartCoroutine(DelayedStart(2f, true, false));
		}
		//online game
		else {
			//should be defaulted to true
			//networkedPauseButtons.SetActive(true);

			//real difference is it does less stuff
			updateFunc += OnlineTurnEndPlayerChange;
			//this one kinda just handles pausing tbh
			game.turnEnded += OnlineMulti;
			
			//render first if
			//we're p1 and it's p1's turn or p2 and p2's turn
			//render second if
			//we're p1 and it's p2's turn or p2 and p1's turn
			bool isP1 = CheckIfClient(game.player1);
			bool isP2 = CheckIfClient(game.player2);

			//slightly longer delay
			StartCoroutine(DelayedStart(3f,
				(isP1 && p1turn) || (isP2 && !p1turn),
				(isP1 && !p1turn) || (isP2 && p1turn)
			));
		}
	}

	//works for both online and local i guess
	IEnumerator DelayedStart(float delay, bool renderFirst, bool renderSecond) {
		yield return new WaitForSeconds(delay);
		game.StartGame(p1turn, renderFirst, renderSecond);
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

	void Update() {
		//this is all it does lol
		updateFunc?.Invoke();
	}

	void OnlineMulti() {
		//pause menu jazz
		if (Input.GetKeyDown(pauseButton)) {
			pauseScreen.SetActive(!pauseScreen.activeInHierarchy);

			//toggle the mice, toggle the player's (figure out which is theirs)
			//if (p1cam.enabled) {
				//p1mouse.disabled = pauseScreen.activeInHierarchy;
				//p1mouse.GetComponent<KeypressCamController>().IgnoreInput(pauseScreen.activeInHierarchy);
			//}
		}

		//nothing else?, possibly add online only keypresses
	}

	void LocalMulti() {
		//pause menu jazz
		if (Input.GetKeyDown(pauseButton)) {
			pauseScreen.SetActive(!pauseScreen.activeInHierarchy);

			//toggle the mice, currently jsut toggle the active one
			if (p1cam.enabled) {
				p1mouse.disabled = pauseScreen.activeInHierarchy;
				p1mouse.GetComponent<KeypressCamController>().IgnoreInput(pauseScreen.activeInHierarchy);
			}
			else {
				p2mouse.disabled = pauseScreen.activeInHierarchy;
				p2mouse.GetComponent<KeypressCamController>().IgnoreInput(pauseScreen.activeInHierarchy);
			}
		}

		//dont allow camera swapping if paused (you technically dont need to pause in local multi tho)
		if (!pauseScreen.activeInHierarchy && Input.GetKeyDown(swapCam)) {
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

	void OnlineTurnEndPlayerChange() {
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

			//dont need to disable cards
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

			//dont need to disable cards
		}
	}
	
	void LocalTurnEndPlayerChange() {
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

	//always returns true if local game
	public static bool CheckIfClient(PlayerData player) {
		if (localMultiplayer)	return true;

		//get the player's index or smt, then use that
		//TODO: know what's the player index thingymagiger
		int playerIndex = 0;

		if (playerIndex == 0) {
			return player.playerTag.Equals("Player1");
		}
		else if (playerIndex == 1) {
			return player.playerTag.Equals("Player2");
		}
		//not even a player
		else {
			return false;
		}
	}
}
