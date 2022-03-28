using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
	public Client client;
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

	SkinnedMeshRenderer p1bell;
	SkinnedMeshRenderer p2bell;
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
		//force p1Turn for now
		p1turn = true;

		p1bell = game.player1.turnEndButton.GetComponentInChildren<SkinnedMeshRenderer>();
		p2bell = game.player2.turnEndButton.GetComponentInChildren<SkinnedMeshRenderer>();

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
				p2mouse.GetComponent<KeypressCamController>().IgnoreInput(true);

				game.player2.turnEndButton.enabled = false;
				p2bell.material.color = Color.grey;
			}
			else {
				p1cam.enabled = false;
				p1mouse.disabled = true;
				p1mouse.GetComponent<KeypressCamController>().IgnoreInput(true);
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
			StartCoroutine(DelayedStart(2f));
		}
		//online game
		else {
			//get these and maybe add the name or smt?
			//game.player1.GetComponentInChildren<TMPro.TMP_InputField>();
			//game.player2.GetComponentInChildren<TMPro.TMP_InputField>();

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

			bool isP1 = CheckIfClient(game.player1, false);
			bool isP2 = CheckIfClient(game.player2, false);

			//don't work lol
			//p1mouse.disabledAnimationMode = !isP1;
			//p2mouse.disabledAnimationMode = !isP2;

			if (isP1 || isP2) {
				leaveButton.SetActive(false);
				leaveLobbyButton.SetActive(false);

				//render first if
				//we're p1 and it's p1's turn or p2 and p2's turn
				//render second if
				//we're p1 and it's p2's turn or p2 and p1's turn

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
			}
			//disable some stuff or smt
			else {
				concedeButton.SetActive(false);

				//add the ability to swap cams
				updateFunc += SwapCams;

				//disable all inputs
				p1mouse.disabled = true;
				p2mouse.disabled = true;

				//for now it's camera spectator
				p2cam.enabled = false;
			}

			//slightly shorter delay
			StartCoroutine(OnlineStart(1.5f, isP1, isP2));
		}

		//make sure those cams are working
		LookAt.ForceUpdateCamera?.Invoke();
	}

	private void OnDestroy() {
		//if networked, recalc rand seed, using a few just in case
		if (!localMultiplayer) {
			Random.InitState(Time.frameCount + ((int)System.DateTime.Now.ToBinary()));
			Random.InitState(Random.Range(0, 3333) * Random.Range(0, 3333) + Random.Range(0, 3333));
			Random.InitState(Random.Range(0, 100000));
		}
		//void when exiting (probably automatic, but whatevs)
		game = null;
	}

	//works for both online and local i guess
	IEnumerator DelayedStart(float delay) {
		yield return new WaitForSeconds(delay);
		game.LocalGameStart(p1turn);
		yield return new WaitForSeconds((game.startingHandSize + 2) * 0.25f);
		if (p1turn) {
			//game.player1.deck.FirstDraw(CheckIfClient(game.player1, true));
			game.player1.deck.FirstDraw(true);
			p2mouse.ActivateEssentials();
		}
		else {
			game.player2.deck.FirstDraw(true);
			p1mouse.ActivateEssentials();
		}
	}

	IEnumerator OnlineStart(float delay, bool isP1, bool isP2) {

		//do game start stuff, shuffle only your own deck, else wait to receive either
		
		//this is for sending the deck if you wanna do it that way, instead we'll jsut seed it lol
		#region grabage
		/*
		byte[] start = System.Text.Encoding.ASCII.GetBytes("DCK");
		
		byte[] p1Deck = null;
		byte[] p2Deck = null;
		if (isP1) {
			game.StartPlayerTurn(game.player1, null);
			//get the deck as a byte array
			p1Deck = new byte[sizeof(int) * game.player1.deck.deck.Count + start.Length];
			System.Buffer.BlockCopy(start, 0, p1Deck, 0, start.Length);
			System.Buffer.BlockCopy(game.player1.deck.deck.ToArray(), 0,
					p1Deck, start.Length, p1Deck.Length - start.Length);
		}
		if (isP2) {
			game.StartPlayerTurn(game.player2, null);
			//get the deck as a byte array
			p2Deck = new byte[sizeof(int) * game.player2.deck.deck.Count + start.Length];
			System.Buffer.BlockCopy(start, 0, p2Deck, 0, start.Length);
			System.Buffer.BlockCopy(game.player2.deck.deck.ToArray(), 0,
					p2Deck, start.Length, p2Deck.Length - start.Length);
		}

		//helper function that decripts message?, nah
		bool ReceiveData(string code, byte[] message, ref byte[] dest) {
			//check code to be safe
			string test = System.Text.Encoding.ASCII.GetString(message, 0, code.Length);
			//correct player
			if (code != test)	return false;
			
			//check the actual code now
			test = System.Text.Encoding.ASCII.GetString(message, code.Length, Client.msgCodeSize);
			//didn't receive deck data, that also means something is terribly wrong lol
			if (test != "DCK")	return false;

			//it's funny how i put this like this lol
			dest = new byte[Client.gameCodeSize - Client.msgCodeSize];

			//should probably have some sort of safety, but you can do that later
			System.Buffer.BlockCopy(message, code.Length + Client.msgCodeSize, dest, 0, dest.Length);

			return true;
		}

		if (p1Deck == null) {
			//see if we receive from p1
			void ReceiveFromP1(byte[] message) {
				if (ReceiveData(Client.player1Code, message, ref p1Deck)) {
					//create player state thingy
					int[] deck = new int[p1Deck.Length / sizeof(int)];

					System.Buffer.BlockCopy(p1Deck, 0, deck, 0, p1Deck.Length);

					game.StartPlayerTurn(game.player1, deck);

					//self unsubscribing
					client.gameCodeReceived -= ReceiveFromP1;
				}
			}

			client.gameCodeReceived += ReceiveFromP1;
		}
		if (p2Deck == null) {
			//see if we receive from p2
			void ReceiveFromP2(byte[] message) {
				if (ReceiveData(Client.player2Code, message, ref p2Deck)) {
					int[] deck = new int[p2Deck.Length / sizeof(int)];

					System.Buffer.BlockCopy(p2Deck, 0, deck, 0, p2Deck.Length);

					game.StartPlayerTurn(game.player2, deck);

					//self unsubscribing
					client.gameCodeReceived -= ReceiveFromP2;
				}
			}

			client.gameCodeReceived += ReceiveFromP2;
		}

		//wait a bit for some making sure others are connected
		if (isP1 || isP2)
			yield return new WaitForSeconds(delay);

		//send the deck to the cloud
		if (isP1)
			Client.SendGameData(p1Deck);
		if (isP2)
			Client.SendGameData(p2Deck);

		//receive deck data
		while (p1Deck == null || p2Deck == null) {
			yield return Card.eof;
		}

		//wait a bit for some making sure others got their data
		yield return new WaitForSeconds(delay);*/
		#endregion

		int seed = -1;
		if (!isP1) {

			void GetSeed(byte[] message) {
				string code = Client.player1Code;

				//check code to be safe
				string test = System.Text.Encoding.ASCII.GetString(message, 0, code.Length);
				//not correct player... uh maybe we can just ignore this lol
				//if (code != test)	return;

				//check the actual code now
				test = System.Text.Encoding.ASCII.GetString(message, code.Length, Client.msgCodeSize);
				//didn't receive seed data, that also means something is terribly wrong lol
				if (test != "SED")	return;

				test = System.Text.Encoding.ASCII.GetString(message,
						code.Length + Client.msgCodeSize, Client.gameCodeSize - Client.msgCodeSize);

				seed = int.Parse(test.Substring(0, test.IndexOf(Client.spliter)));

				client.gameCodeReceived -= GetSeed;
			}

			client.gameCodeReceived += GetSeed;

			while (true) {
				yield return Card.eof;
				//receive random seed
				if (seed > 0)	break;
			}
		}
		//is p1, send the code to everyone else
		else {
			//wait a bit, hopefully it's enough
			yield return Client.DesyncCompensation;

			//generate the seed
			seed = Random.Range(0, 1000000);
			Client.SendGameData(System.Text.Encoding.ASCII.GetBytes("SED" + seed.ToString() + Client.spliter));

			//might end up waiting more, but for the best lol
			yield return new WaitForSeconds(delay);
		}
		Random.InitState(seed);

		//wait a bit
		yield return new WaitForSeconds(delay);
		
		//calc the decks now
		game.StartPlayerTurn(game.player1, null);
		game.StartPlayerTurn(game.player2, null);

		//now we make players draw cards
		game.StartDrawCards(isP1, isP2);

		void InputSend(string code, Transform hit, Mouse mouse) {
			//avoid the uh oh stinkies
			//if (mouse.whoopsies)	return;

			//the obvious
			if (!hit || !hit.CompareTag("Interactable")) return;

			string message = "";
			//figure out what we hit
			//first figure out type
			Card cardTest = null;
			CardMover moverTest = null;
			CardAttacker attackerTest = null;
			PressEventButton buttonTest = null;
			int index = -1;

			if (hit.TryGetComponent<Card>(out cardTest)) {
				index = cardTest.player.heldCards.IndexOf(cardTest);
				if (index >= 0)
					message = "CRD" + GetPlayerCode(cardTest.player) + index.ToString() + Client.spliter;
			}
			else if (hit.TryGetComponent<CardMover>(out moverTest)) {
				index = moverTest.playerData.backLine.IndexOf(moverTest);
				if (index >= 0)
					message = "CMV" + GetPlayerCode(moverTest.playerData) + index.ToString() + Client.spliter;
			}
			else if (hit.TryGetComponent<CardAttacker>(out attackerTest)) {
				index = attackerTest.playerData.field.IndexOf(attackerTest);
				if (index >= 0)
					message = "CAT" + GetPlayerCode(attackerTest.playerData) + index.ToString() + Client.spliter;
			}
			else if (hit.TryGetComponent<PressEventButton>(out buttonTest)) {
				if (buttonTest.player == mouse.player)
					message = "BUT" + buttonTest.name + Client.spliter;
			}

			//if none of the above
			if (message == "")	return;

			Client.SendGameData(System.Text.Encoding.ASCII.GetBytes(code + message));
		}

		//subscribe to the proper events for sending purposes
		//hover event is better as UDP
		if (isP1) {
			void P1ClickSend(Transform hit) {
				InputSend("CLK", hit, p1mouse);
			}
			p1mouse.clickEvent += P1ClickSend;

			void P1ReleaseSend(Transform hit) {
				InputSend("REL", hit, p1mouse);
			}
			p1mouse.releaseEvent += P1ReleaseSend;
		}
		if (isP2) {
			void P2ClickSend(Transform hit) {
				InputSend("CLK", hit, p2mouse);
			}
			p2mouse.clickEvent += P2ClickSend;

			void P2ReleaseSend(Transform hit) {
				InputSend("REL", hit, p2mouse);
			}
			p2mouse.releaseEvent += P2ReleaseSend;
		}

		//subscribe the gameEvent stuff
		client.gameCodeReceived += ReadCodes;

		yield return new WaitForSeconds((game.startingHandSize + 2) * 0.25f);
		if (p1turn) {
			game.player1.deck.FirstDraw(CheckIfClient(game.player1, false));
			p2mouse.ActivateEssentials();
		}
		else {
			game.player2.deck.FirstDraw(CheckIfClient(game.player2, false));
			p1mouse.ActivateEssentials();
		}
	}

	void ReadCodes(byte[] message) {
		//get the code
		string code = System.Text.Encoding.ASCII.GetString(message, 0, Client.player1Code.Length);

		Mouse mouse = null;

		if (code == Client.player1Code)
			mouse = p1mouse;
		if (code == Client.player2Code)
			mouse = p2mouse;

		if (mouse == null)	return;

		//if an input change
		if (code == "INP") {
			string msg = System.Text.Encoding.ASCII.GetString(message, Client.player1Code.Length, 5);
			//get the animation mode code
			code = msg.Substring(0, 3);

			if (code == "ANM") {
				if (msg.Substring(3, 2) == "ON")
					mouse.ActivateAnimationMode();
				else
					mouse.DeactivateAnimationMode();
			}
			else if (code == "SPL") {
				if (msg.Substring(3, 2) == "ON")
					mouse.ActivateSpellMode();
				else
					mouse.DeactivateSpellMode();
			}
			return;
		}

		Transform hit = FindTheObject(message, mouse);

		if (hit == null)	return;

		code = System.Text.Encoding.ASCII.GetString(message, code.Length, Client.msgCodeSize);

		if (code == "CLK") {
			mouse.ForwardClickEvent(hit);
		}
		else if (code == "REL") {
			mouse.ForwardReleaseEvent(hit);
		}
	}

	Transform FindTheObject(byte[] message, Mouse mouse) {
		//if too small, ignore
		if (message.Length < 5)	return null;
		Transform temp = null;
		PlayerData player = mouse.player;

		string input = System.Text.Encoding.ASCII.GetString(message,
				Client.player1Code.Length + Client.msgCodeSize, Client.gameCodeSize - Client.msgCodeSize);

		string code = input.Substring(0, Client.msgCodeSize);

		int index = -1;

		if (code == "CRD") {
			//make sure player is correct
			code = input.Substring(0, Client.msgCodeSize + Client.player1Code.Length);

			if (code.Substring(Client.msgCodeSize) != GetPlayerCode(player)) {
				//this is how you get the opposing lol
				player = player.field[0].opposingData;
			}

			//get the card index
			index = int.Parse(input.Substring(code.Length, input.IndexOf(Client.spliter) - code.Length));
			temp = player.heldCards[index].transform;
		}
		else if (code == "CMV") {
			//make sure player is correct
			code = input.Substring(0, Client.msgCodeSize + Client.player1Code.Length);

			if (code.Substring(Client.msgCodeSize) != GetPlayerCode(player)) {
				//this is how you get the opposing lol
				player = player.field[0].opposingData;
			}

			//get the mover index
			index = int.Parse(input.Substring(code.Length, input.IndexOf(Client.spliter) - code.Length));
			temp = player.backLine[index].transform;
		}
		else if (code == "CAT") {
			//make sure player is correct
			code = input.Substring(0, Client.msgCodeSize + Client.player1Code.Length);

			if (code.Substring(Client.msgCodeSize) != GetPlayerCode(player)) {
				//this is how you get the opposing lol
				player = player.field[0].opposingData;
			}
			
			//get the mover index
			index = int.Parse(input.Substring(code.Length, input.IndexOf(Client.spliter) - code.Length));
			temp = player.field[index].transform;
		}
		else if (code == "BUT") {
			//get the mover index
			string tempName = input.Substring(code.Length, input.IndexOf(Client.spliter) - code.Length);
			foreach (PressEventButton button in FindObjectsOfType<PressEventButton>()) {
				if (button.player == player && button.name == tempName) {
					temp = button.transform;
				}
			}
		}

		return temp;
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
				p1mouse.GetComponent<KeypressCamController>().IgnoreInput(true);

				p2cam.enabled = true;
				p2mouse.disabled = false;
				p2mouse.GetComponent<KeypressCamController>().IgnoreInput(false);
			}
			else {
				p1cam.enabled = true;
				p1mouse.disabled = false;
				p1mouse.GetComponent<KeypressCamController>().IgnoreInput(false);

				p2cam.enabled = false;
				p2mouse.disabled = true;
				p2mouse.GetComponent<KeypressCamController>().IgnoreInput(true);
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
				p1cam.GetComponent<KeypressCamController>().IgnoreInput(true);
				p2cam.enabled = true;
				p2cam.GetComponent<KeypressCamController>().IgnoreInput(false);
			}
			else {
				p1cam.enabled = true;
				p1cam.GetComponent<KeypressCamController>().IgnoreInput(false);
				p2cam.enabled = false;
				p2cam.GetComponent<KeypressCamController>().IgnoreInput(true);
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

			//delay this if it's the player's turn end, makes sure the other doesnt double press
			p2mouse.DeActivateEssentials();
			game.player2.deck.FirstDraw(CheckIfClient(game.player2, false));
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
			p2mouse.DeActivateAll();
			p2mouse.ActivateEssentials();
			game.player2.turnEndButton.enabled = false;
			p2bell.material.color = Color.grey;

			//delay this if it's the player's turn end, makes sure the other doesnt double press
			p1mouse.DeActivateEssentials();
			game.player1.deck.FirstDraw(CheckIfClient(game.player1, false));

			game.player1.turnEndButton.enabled = true;
			p1bell.material.color = defaultBellCol;

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
	public static bool CheckIfClient(PlayerData player, bool localReturn) {
		//if no player, check for client
		if (player == null) {
			return p1Index == Client.playerId || p2Index == Client.playerId;
		}

		if (localMultiplayer)	return localReturn;

		//always return true if no game
		if (game == null)	return true;

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

	public static string GetPlayerCode(PlayerData player) {
		//always return nothing if no game
		if (game == null) return "";

		if (player == game.player1)	return Client.player1Code;
		if (player == game.player2)	return Client.player2Code;
		return "";
	}

	//does as the name implies, only shows to players
	void ShowLeaveButton(PlayerData winner) {
		if (CheckIfClient(null, false)) {
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
