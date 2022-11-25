using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ServerManager : MonoBehaviour
{
	public Client client;
	public GameController gameCon;
	public static GameController game;
	public Mouse p1mouse;
	public Mouse p2mouse;
	private Camera p1cam;
	private Camera p2cam;
	public static bool localMultiplayer = true;

	//store the current turn
	public static bool p1turn = true;
	public static int p1Index = -1;
	public static int p2Index = -1;

	public InputAction pauseButton;
	public InputAction swapCam;

	[SerializeField]	GameObject pauseScreen;
	[SerializeField]	GameObject networkedPauseButtons;
	[SerializeField]	GameObject leaveButton;
	[SerializeField]	GameObject leaveLobbyButton;
	[SerializeField]	GameObject concedeButton;

	[SerializeField]	GameObject winObj;
	[SerializeField]	TMPro.TMP_Text winText;

	SkinnedMeshRenderer p1bell;
	SkinnedMeshRenderer p2bell;
	Color defaultBellCol;

	private void Awake() {
		//assign game to static var
		game = gameCon;

		winObj.SetActive(false);
		pauseScreen.SetActive(false);

		p1cam = p1mouse.GetComponent<Camera>();
		p2cam = p2mouse.GetComponent<Camera>();
	}

	private void OnEnable() {
		pauseButton.Enable();
		swapCam.Enable();
	}

	private void OnDisable() {
		pauseButton.Disable();
		swapCam.Disable();
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
			//has to do lots of input controlling
			pauseButton.started += ctx => {
				pauseScreen.SetActive(!pauseScreen.activeInHierarchy);

				//toggle the mice, currently jsut toggle the active one
				if (lookingAtP1) {
					p1mouse.SetDisabled(pauseScreen.activeInHierarchy);
					p1mouse.GetComponent<KeypressCamController>().IgnoreInput(pauseScreen.activeInHierarchy);
				}
				else {
					p2mouse.SetDisabled(pauseScreen.activeInHierarchy);
					p2mouse.GetComponent<KeypressCamController>().IgnoreInput(pauseScreen.activeInHierarchy);
				}
			};

			swapCam.started += ctx => {
				//dont allow camera swapping if paused (you technically dont need to pause in local multi tho)
				if (!pauseScreen.activeInHierarchy) {
					//toggle cameras
					if (lookingAtP1) {
						lookingAtP1 = false;
						if (!localCamTarget) {
							p1mouse.SetDisabled(true);
							p1mouse.GetComponent<KeypressCamController>().IgnoreInput(true);

							localCamTarget = p1mouse.GetComponent<CameraController>();
							StartCoroutine(LocalCamSwapTransition());
						}
						localCamTarget.ForceTransition(p2cam.GetComponent<CameraController>().GetCurrent());
					}
					else {
						lookingAtP1 = true;
						if (!localCamTarget) {
							p2mouse.SetDisabled(true);
							p2mouse.GetComponent<KeypressCamController>().IgnoreInput(true);

							localCamTarget = p2mouse.GetComponent<CameraController>();
							StartCoroutine(LocalCamSwapTransition());
						}
						localCamTarget.ForceTransition(p1cam.GetComponent<CameraController>().GetCurrent());
					}
				}
			};

			//this one is local only as it hides faces and handles pausing differently
			game.turnEnded += LocalTurnEndPlayerChange;
			game.playerWon += LocalWinner;

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
				//for cam switching
				lookingAtP1 = false;

				p1cam.enabled = false;
				p1mouse.disabled = true;
				p1mouse.GetComponent<KeypressCamController>().IgnoreInput(true);
				p2cam.enabled = true;
				p2mouse.disabled = false;

				game.player1.turnEndButton.enabled = false;
				p1bell.material.color = Color.grey;
			}
			networkedPauseButtons.SetActive(false);

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
			pauseButton.started += ctx => {
				pauseScreen.SetActive(!pauseScreen.activeInHierarchy);

				//toggle the mice, toggle the player's (figure out which is theirs)
				if (p1Index == Client.playerId) {
					p1mouse.SetDisabled(pauseScreen.activeInHierarchy);
					p1mouse.GetComponent<KeypressCamController>().IgnoreInput(pauseScreen.activeInHierarchy);
				}
				else if (p2Index == Client.playerId) {
					p2mouse.SetDisabled(pauseScreen.activeInHierarchy);
					p2mouse.GetComponent<KeypressCamController>().IgnoreInput(pauseScreen.activeInHierarchy);
				}
			};

			game.playerWon += OnlineWinner;
			game.playerWon += ShowLeaveButton;

			client.udpEvent += UdpUpdate;
			client.winnerEvent += MakeWinner;

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
				swapCam.Disable();

				void SendWin(PlayerData winner) {
					if (CheckIfClient(winner, false)) {
						Client.SendStringMessage("WIN");
					}
				}
				game.playerWon += SendWin;
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
					StartCoroutine(SendUdpData(p1mouse.mouseObject));
				}
				//is p2
				else {
					p1cam.enabled = false;
					p1mouse.disabled = true;
					p2cam.enabled = true;
					p2mouse.disabled = false;
					StartCoroutine(SendUdpData(p2mouse.mouseObject));
				}
			}
			//disable some stuff or smt
			else {
				concedeButton.SetActive(false);

				//add the ability to swap cams
				swapCam.started += ctx => {
					//made for spectators
					if (!pauseScreen.activeInHierarchy) {
						//toggle cameras
						if (p1cam.enabled) {
							p1cam.enabled = false;
							p1cam.GetComponent<KeypressCamController>().IgnoreInput(true);
							p2cam.enabled = true;
							p2cam.GetComponent<KeypressCamController>().IgnoreInput(false);

							localCamTarget = p2cam.GetComponent<CameraController>();
							localCamTarget.ForceTransition(p1cam.transform);
							localCamTarget.Snap();
							localCamTarget.ForceTransition(null);
						}
						else {
							p1cam.enabled = true;
							p1cam.GetComponent<KeypressCamController>().IgnoreInput(false);
							p2cam.enabled = false;
							p2cam.GetComponent<KeypressCamController>().IgnoreInput(true);

							localCamTarget = p1cam.GetComponent<CameraController>();
							localCamTarget.ForceTransition(p2cam.transform);
							localCamTarget.Snap();
							localCamTarget.ForceTransition(null);
						}
						LookAt.ForceUpdateCamera?.Invoke();
					}	
				};

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
			//yield return new WaitForSeconds(delay);
		}
		Random.InitState(seed);

		//wait a bit
		yield return new WaitForSeconds(delay);
		
		//calc the decks now
		game.StartPlayerTurn(game.player1, null);
		game.StartPlayerTurn(game.player2, null);

		//now we make players draw cards
		game.StartDrawCards(isP1, isP2);

		void InputSend(string code, Transform hit, Mouse mouse, bool checkTag) {
			//avoid the uh oh stinkies
			//if (mouse.whoopsies)	return;

			//the obvious, but also make sure release always sends
			if (checkTag && (!hit || !hit.CompareTag("Interactable"))) return;

			string message = "";
			
			//figure out what we hit
			if (hit) {
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
					if (buttonTest.player == mouse.player || buttonTest.player == null)
						message = "BUT" + buttonTest.id.ToString() + Client.spliter;
				}

				//if none of the above, dont send (since we did hit something)
				if (checkTag && (message == ""))	return;
			}

			Client.SendGameData(System.Text.Encoding.ASCII.GetBytes(code + message));
		}

		//subscribe to the proper events for sending purposes
		//hover event is better as UDP
		if (isP1) {
			void P1ClickSend(Transform hit) {
				InputSend("CLK", hit, p1mouse, true);
			}
			p1mouse.clickEvent += P1ClickSend;

			void P1ReleaseSend(Transform hit) {
				if (p1mouse.holding)
					InputSend("REL", hit, p1mouse, false);
			}
			p1mouse.releaseEvent += P1ReleaseSend;

			Transform tempTrans = null;

			void P1HovSend(Transform hit) {
				if (tempTrans != hit) {
					//if ((tempTrans && tempTrans.CompareTag("Interactable"))
					//	|| (hit && hit.CompareTag("Interactable")))

					//basically a check if hovering over a card
					if ((tempTrans && tempTrans.GetComponent<Card>())
						|| (hit && hit.GetComponent<Card>()))
					{
						InputSend("HOV", hit, p1mouse, false);
					}
					tempTrans = hit;
				}
			}
			p1mouse.hoverEvent += P1HovSend;
		}
		if (isP2) {
			void P2ClickSend(Transform hit) {
				InputSend("CLK", hit, p2mouse, true);
			}
			p2mouse.clickEvent += P2ClickSend;

			void P2ReleaseSend(Transform hit) {
				if (p2mouse.holding)
					InputSend("REL", hit, p2mouse, false);
			}
			p2mouse.releaseEvent += P2ReleaseSend;

			Transform tempTrans = null;

			void P2HovSend(Transform hit) {
				if (tempTrans != hit) {
					//if ((tempTrans && tempTrans.CompareTag("Interactable"))
					//	|| (hit && hit.CompareTag("Interactable")))

					//basically a check if hovering over a card
					if ((tempTrans && tempTrans.GetComponent<Card>())
						|| (hit && hit.GetComponent<Card>()))
					{
						InputSend("HOV", hit, p2mouse, false);
					}
					tempTrans = hit;
				}
			}
			p2mouse.hoverEvent += P2HovSend;
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

	//would've used a byte pointer if default delegates handled that
	void ReadCodes(byte[] message) {
		//get the code
		string code = System.Text.Encoding.ASCII.GetString(message, 0, Client.player1Code.Length);

		Mouse mouse = null;

		if (code == Client.player1Code)
			mouse = p1mouse;
		if (code == Client.player2Code)
			mouse = p2mouse;

		if (mouse == null)	return;

		code = System.Text.Encoding.ASCII.GetString(message, code.Length, Client.msgCodeSize);

		//if an input change
		if (code == "INP") {
			string msg = System.Text.Encoding.ASCII.GetString(message,
					Client.player1Code.Length + Client.msgCodeSize, 5);
			
			//get the animation mode code
			code = msg.Substring(0, 3);

			if (code == "ANM") {
				if (msg.Substring(3, 2) == "ON")
					mouse.ActivateAnimationMode(false);
				else
					mouse.DeactivateAnimationMode(false);
			}
			else if (code == "SPL") {
				if (msg.Substring(3, 2) == "ON")
					mouse.ActivateSpellMode(false);
				else
					mouse.DeactivateSpellMode(false, false);
			}
			return;
		}

		Transform hit = FindTheObject(message, mouse);

		//actually helpful we receive null
		//if (hit == null)	return;

		if (code == "CLK") {
			mouse.ForwardClickEvent(hit);
		}
		else if (code == "REL") {
			mouse.ForwardReleaseEvent(hit);
		}
		else if (code == "HOV") {
			mouse.ForwardHoverEvent(hit);
		}
	}

	Transform FindTheObject(in byte[] message, Mouse mouse) {
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
			int tempId = int.Parse(input.Substring(code.Length, input.IndexOf(Client.spliter) - code.Length));
			foreach (PressEventButton button in FindObjectsOfType<PressEventButton>()) {
				if (button.id == tempId && (button.player == player || button.player == null)) {
					temp = button.transform;
				}
			}
		}

		return temp;
	}

	const float udpDelay = 0.2f;
	int[] tempId = new int[1];
	float[] tempPos = new float[6];
	const int posArrSize = sizeof(float) * 6;
	Vector3 tempPosVec = Vector3.zero;
	Vector3 tempVeloVec = Vector3.zero;

	//same
	void UdpUpdate(byte[] message) {
		//get the id
		System.Buffer.BlockCopy(message, 0, tempId, 0, sizeof(int));

		//don't accept your own messages
		if (tempId[0] == Client.playerId) {
			return;
		}

		System.Buffer.BlockCopy(message, sizeof(int), tempPos, 0, posArrSize);

		//store the position
		tempPosVec.x = tempPos[0];
		tempPosVec.y = tempPos[1];
		tempPosVec.z = tempPos[2];

		//store the velocity
		tempVeloVec.x = tempPos[3];
		tempVeloVec.y = tempPos[4];
		tempVeloVec.z = tempPos[5];

		//we need to test with AWS again
		//Debug.Log("p1 is " + p1Index + ", p2 is " + p2Index + " we received " + tempId[0]);

		//if p1
		if (tempId[0] == p1Index)
			p1mouse.MoveMouse(tempPosVec, tempVeloVec, udpDelay);
		//if p2
		else if (tempId[0] == p2Index)
			p2mouse.MoveMouse(tempPosVec, tempVeloVec, udpDelay);
	}

	IEnumerator SendUdpData(Transform target) {
		float waitTime = udpDelay;
		WaitForSeconds delay = new WaitForSeconds(waitTime);

		//so we can scale velo
		waitTime = 1f / waitTime;

		tempId[0] = Client.playerId;

		byte[] message = new byte[sizeof(int) + posArrSize];
		//store the id
		System.Buffer.BlockCopy(tempId, 0, message, 0, sizeof(int));

		Vector3 prevPos = target.position;
		Vector3 velo = Vector3.zero;
		bool doubleCheck = true;

		yield return Card.eof;
		//as long as the target exists
		while (target) {
			if (target.position != prevPos || doubleCheck) {
				if (target.position == prevPos)
					doubleCheck = false;
				else if (!doubleCheck)
					doubleCheck = true;

				tempPos[0] = target.position.x;
				tempPos[1] = target.position.y;
				tempPos[2] = target.position.z;

				//instantanious velo, consider doing average velo? nah
				velo = (target.position - prevPos) * waitTime;

				tempPos[3] = velo.x;
				tempPos[4] = velo.y;
				tempPos[5] = velo.z;

				System.Buffer.BlockCopy(tempPos, 0, message, sizeof(int), posArrSize);
				Client.SendUDP(message);

				prevPos = target.position;
			}
			yield return delay;
		}
	}

	void MakeWinner(string playerCode) {
		if (playerCode == Client.player1Code) {
			game.MakeWinner(game.player1);
		}
		else if (playerCode == Client.player2Code) {
			game.MakeWinner(game.player2);
		}
	}

	bool lookingAtP1 = true;
	CameraController localCamTarget = null;
	void LocalMulti() {
	}

	IEnumerator LocalCamSwapTransition() {
		float moveSpeed = localCamTarget.moveSpeed;
		localCamTarget.moveSpeed *= 2.5f;
		yield return Card.eof;
		//we really jsut need to wait till it's done
		while (localCamTarget.transitioning) {
			yield return Card.eof;
		}

		if (lookingAtP1) {
			p1cam.enabled = true;
			p2cam.enabled = false;

			p1mouse.disabled = false;
			p1mouse.GetComponent<KeypressCamController>().IgnoreInput(false);
		}
		else {
			p1cam.enabled = false;
			p2cam.enabled = true;

			p2mouse.disabled = false;
			p2mouse.GetComponent<KeypressCamController>().IgnoreInput(false);
		}
		LookAt.ForceUpdateCamera?.Invoke();

		//release it
		localCamTarget.moveSpeed = moveSpeed;
		localCamTarget.ForceTransition(null);
		localCamTarget.Snap();
		localCamTarget = null;
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

	void OnlineWinner(PlayerData winner) {
		LocalWinner(winner);
	}

	void LocalWinner(PlayerData winner) {
		winObj.SetActive(true);
		winText.text = winner.name + " won the game";
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
