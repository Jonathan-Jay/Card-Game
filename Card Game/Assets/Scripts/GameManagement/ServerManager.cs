using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
	public GameController gameCon;
	public static GameController game;
	public Mouse p1mouse;
	public Mouse p2mouse;
	private Camera p1cam;
	private Camera p2cam;
	private event System.Action updateFunc;
	public static bool localMultiplayer = true;

	//store the current turn
	public static bool p1turn = true;
	public static int p1Index = -1;
	public static int p2Index = -1;

	[SerializeField]	KeyCode swapCam;
	[SerializeField]	KeyCode pauseButton;
	[SerializeField]	GameObject pauseScreen;
	[SerializeField]	GameObject networkedPauseButtons;
	[SerializeField]	GameObject leaveButton;
	[SerializeField]	GameObject leaveLobbyButton;
	[SerializeField]	GameObject concedeButton;

	MeshRenderer p1bell;
	MeshRenderer p2bell;
	Color defaultBellCol;

	private void Awake() {
		//assign game to static var
		game = gameCon;

		pauseScreen.SetActive(false);

		p1cam = p1mouse.GetComponent<Camera>();
		p2cam = p2mouse.GetComponent<Camera>();
	}

	//late inits
	private void Start() {
		p1bell = game.player1.turnEndButton.GetComponentInChildren<MeshRenderer>();
		p2bell = game.player2.turnEndButton.GetComponentInChildren<MeshRenderer>();

		defaultBellCol = p1bell.material.color;

		//this is required for both online and local
		game.playerWon += RemoveInputs;

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
			//make sure leave lobby button is here
			//also the concede button
			//networkedPauseButtons.SetActive(true);

			//real difference is it does less stuff
			game.turnEnded += OnlineTurnEndPlayerChange;

			//this one kinda just handles pausing tbh
			updateFunc += OnlineMulti;
			game.playerWon += ShowLeaveButton;

			if (p1turn) {
				game.player2.turnEndButton.enabled = false;
				p2bell.material.color = Color.grey;
			}
			else {
				game.player1.turnEndButton.enabled = false;
				p1bell.material.color = Color.grey;
			}

			if (CheckIfClient(null)) {
				leaveButton.SetActive(false);
				leaveLobbyButton.SetActive(false);

				//render first if
				//we're p1 and it's p1's turn or p2 and p2's turn
				//render second if
				//we're p1 and it's p2's turn or p2 and p1's turn

				bool isP1 = CheckIfClient(game.player1);
				bool isP2 = CheckIfClient(game.player2);

				if (isP1) {
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

				//slightly longer delay
				StartCoroutine(DelayedStart(3f,
					(isP1 && p1turn) || (isP2 && !p1turn),
					(isP1 && !p1turn) || (isP2 && p1turn)
				));
			}
			//disable some stuff or smt
			else {
				concedeButton.SetActive(false);

				//add the ability to swap cams
				updateFunc += SwapCams;

				//disable all inputs
				p1mouse.disabled = true;
				p2mouse.disabled = true;
				p2cam.enabled = false;

				//don't render any cards
				StartCoroutine(DelayedStart(3f, false, false));
			}
		}
	}

	private void OnDestroy() {
		//void when exiting (probably automatic, but whatevs)
		game = null;
	}

	//works for both online and local i guess
	IEnumerator DelayedStart(float delay, bool renderFirst, bool renderSecond) {
		yield return new WaitForSeconds(delay);
		game.StartGame(p1turn, renderFirst, renderSecond);
		yield return new WaitForSeconds((game.startingHandSize + 2) * 0.25f);
		if (p1turn) {
			game.player1.deck.FirstDraw(CheckIfClient(game.player1));
			p2mouse.ActivateEssentials();
			//p1mouse.ActivateAll();
			/*if (game.player1.canDraw > 0) {
				p1mouse.ActivateDeck();
			}
			else {
				p1mouse.ActivateAll();
			}*/
		}
		else {
			game.player2.deck.FirstDraw(CheckIfClient(game.player2));
			p1mouse.ActivateEssentials();
			//p2mouse.ActivateAll();
			/*if (game.player2.canDraw > 0) {
				p2mouse.ActivateDeck();
			}
			else {
				p2mouse.ActivateAll();
			}*/
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
			if (p1Index == Client.playerId) {
				p1mouse.disabled = pauseScreen.activeInHierarchy;
				p1mouse.GetComponent<KeypressCamController>().IgnoreInput(pauseScreen.activeInHierarchy);
			}
			else if (p2Index == Client.playerId) {
				p2mouse.disabled = pauseScreen.activeInHierarchy;
				p2mouse.GetComponent<KeypressCamController>().IgnoreInput(pauseScreen.activeInHierarchy);
			}
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

	void SwapCams() {
		//made for spectators
		if (Input.GetKeyDown(swapCam)) {
			//toggle cameras
			if (p1cam.enabled) {
				p1cam.enabled = false;
				p2cam.enabled = true;
			}
			else {
				p1cam.enabled = true;
				p2cam.enabled = false;
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
			game.player2.deck.FirstDraw(CheckIfClient(game.player2));
			//if (game.player2.canDraw > 0)
			//	p2mouse.ActivateDeck();
			//else
			//	p2mouse.ActivateAll();

			game.player2.turnEndButton.enabled = true;
			p2bell.material.color = defaultBellCol;

			//dont need to disable cards
		}
		else {
			p1turn = true;
			p1mouse.DeActivateEssentials();
			game.player1.deck.FirstDraw(CheckIfClient(game.player1));
			//if (game.player1.canDraw > 0)
			//	p1mouse.ActivateDeck();
			//else
			//	p1mouse.ActivateAll();

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
			//if (game.player2.canDraw > 0)
			//	p2mouse.ActivateDeck();
			//else
			//	p2mouse.ActivateAll();

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

			game.player2.deck.FirstDraw(true);
		}
		else {
			p1turn = true;
			p1mouse.DeActivateEssentials();
			//if (game.player1.canDraw > 0)
			//	p1mouse.ActivateDeck();
			//else
			//	p1mouse.ActivateAll();

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
			game.player1.deck.FirstDraw(true);
		}
	}

	//always returns true if local game
	//if player is null, returns if player is either
	public static bool CheckIfClient(PlayerData player) {
		if (localMultiplayer)	return true;

		//always return true if no game
		if (game == null)	return true;

		//read the comment above
		if (player == null) {
			return p1Index == Client.playerId || p2Index == Client.playerId;
		}

		//check if the index matches the player index
		if (p1Index == Client.playerId) {
			return player == game.player1;
		}
		if (p2Index == Client.playerId) {
			return player == game.player2;
		}
		//not even a player
		return false;
	}

	//does as the name implies, only shows to players
	void ShowLeaveButton(PlayerData winner) {
		if (CheckIfClient(null)) {
			leaveButton.SetActive(true);
			concedeButton.SetActive(false);
		}
	}

	//also shows all cards
	void RemoveInputs(PlayerData winner) {
		//meaning currently p1 has inputs
		if (p1turn) {
			p1mouse.DeActivateAll();
			p1mouse.ActivateEssentials();

			game.player1.turnEndButton.enabled = false;
		}
		else {
			p2mouse.DeActivateAll();
			p2mouse.ActivateEssentials();

			game.player2.turnEndButton.enabled = false;
		}
		foreach (Card card in FindObjectsOfType<Card>()) {
			card.RenderFace();
		}
	}
}
