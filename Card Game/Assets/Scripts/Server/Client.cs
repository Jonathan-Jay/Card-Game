using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class Client : MonoBehaviour
{
	public const string gameSceneName = "GameScene";
	public const char spliter = '\t';
	public string notificationColour = "yellow";
	public const int msgCodeSize = 3;
	public const char terminator = '\r';
    public static byte[] recBuffer = new byte[512];
	public static Socket client;
	public static IPEndPoint server;
	public static string username {get; private set;} = "";

	public float waitDuration = 5f;
	bool connecting = false;
	public static bool canStart { get; private set;} = false;
	public static bool inGame { get; private set;} = false;
	public static int playerId {get; private set;} = -1;

	static string lobbyName = "";
	static bool inLobby = false;
	[SerializeField] TMPro.TMP_Text usernameText;
	[SerializeField] GameObject chatCanvas;
	[SerializeField] TMPro.TMP_InputField textChat;
	[SerializeField] TextChat chat;
	int recv;

	//true if connected, false if not
	public event Action<bool> connectedEvent;
	//bool for if functioning, second for failure message
	public event Action<bool, string> connectingEvent;
	public event Action leaveServerEvent;
	//true for currently in lobby
	public event Action<bool, string> updatePlayerList;
	public event Action<string> updateLobbyList;
	public event Action dirty;
	public event Action<string> lobbyError;
	//true if in lobby, string for lobby name
	public event Action<bool, string> joinedLobby;

	private void Start() {
		//not online yet
		if (!canStart) {
			chatCanvas.SetActive(false);
			//connected event
			connectedEvent?.Invoke(false);
		}
		//is online
		else {
			usernameText.text = username;

			chatCanvas.SetActive(true);
			//disconnected event
			connectedEvent?.Invoke(true);
		}

		//send to lobby if in a lobby already
		if (inLobby) {
			joinedLobby?.Invoke(inLobby, lobbyName);
			//refresh list somehow
			client.SendTo(Encoding.ASCII.GetBytes("DTY"), server);
		}
	}

	public void LeaveServer() {
		if (!canStart)	return;

		leaveServerEvent?.Invoke();
		
		usernameText.text = "";
		chat.UpdateChat("<color=red>Left The Server</color>");
		if (chatCanvas.activeInHierarchy)
			chatCanvas.SetActive(false);
		Close();
		canStart = false;
	}

	public void TryConnect(string ip) {
		if (!connecting)
			StartCoroutine(ConnectionAttempt(ip));
	}

	IEnumerator ConnectionAttempt(string ipText) {
		connecting = true;

		//maybe we'll need
		//connectingEvent?.Invoke(true, "Connecting...");

        //Setup our end point (server)
        try {
            //IPAddress ip = Dns.GetHostAddresses("mail.bigpond.com")[0];
            IPAddress ip = IPAddress.Parse(ipText);
            server = new IPEndPoint(ip, 42069);
            //create out client socket 
            client = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			client.Blocking = false;
		}
		catch (Exception) {
			//Debug.Log(e.ToString());
			connectingEvent?.Invoke(false, "Inputed ip Invalid");

			connecting = false;
		}

		//if broke
		if (!connecting)	yield break;

		try {
			client.Connect(server);
		}
		catch (SocketException SockExc) {
			if (SockExc.SocketErrorCode != SocketError.WouldBlock) {
				//Debug.Log(SockExc.ToString());
				connectingEvent?.Invoke(false, "Inputed ip Invalid");

				connecting = false;
			}
		}
		catch (Exception e) {
			Debug.Log(e.ToString());

			connecting = false;
		}

		//if broke
		if (!connecting)	yield break;

		//enough time has passed we can turn these off
		connectingEvent?.Invoke(true, "Connecting...");

		bool connected = false;
		for (float counter = 0; counter < waitDuration; counter += Time.deltaTime) {
        	try {
				//because client.Connected don't work lol
				recv = client.Receive(recBuffer);
				if (recv > 0) {
					//do the first test manually
					username = Encoding.ASCII.GetString(recBuffer, 0, recv);

					//format is id/username, where / is the spliter
					int index = username.IndexOf(spliter);
					playerId = int.Parse(username.Substring(0, index));
					username = username.Substring(index + 1);

					//send it whatever might be after the name
					index = username.IndexOf(terminator);
					if (index > 1) {
						byte[] testMessage = new byte[recv - index - 1];
						Buffer.BlockCopy(recBuffer, index + 1, testMessage, 0, testMessage.Length);

						TestMessage(testMessage, testMessage.Length);

						//then get the real username
						username = username.Substring(0, index);
					}
					usernameText.text = username;
				}
				connected = true;
				break;
			}
			catch (SocketException SockExc) {
				if (SockExc.SocketErrorCode != SocketError.WouldBlock) {
					//this works, the message gets annoying
					//Debug.Log(SockExc.ToString());
					connecting = false;
				}
			}
			yield return new WaitForEndOfFrame();
		}

		//joinServerButton.interactable = true;
		if (connected) {
			connectingEvent?.Invoke(true, "Connected");
			connectedEvent?.Invoke(true);

			//connected is true
			chatCanvas.SetActive(true);

			yield return new WaitForEndOfFrame();
			canStart = true;
		}
		else {
			connectingEvent?.Invoke(false, "Failed");

			client = null;
		}
		connecting = false;
    }

	public void SendTextChatMessage() {
		byte[] msg = Encoding.ASCII.GetBytes("MSG" + textChat.text);
		client.SendTo(msg, server);
		textChat.text = "";
	}

	public static void ChangeUserName(TMPro.TMP_InputField username) {
		if (username.text == "")	return;

		client.SendTo(Encoding.ASCII.GetBytes("CNM" + username.text), server);
	}

	public static void CreateLobby(TMPro.TMP_InputField input) {
		if (input.text == "")	return;

		client.SendTo(Encoding.ASCII.GetBytes("CLB" + input.text), server);
	}

	public static void JoinLobby(int index) {
		client.SendTo(Encoding.ASCII.GetBytes("JLB" + index), server);
	}

	public static void LeaveLobby() {
		client.SendTo(Encoding.ASCII.GetBytes("LLB"), server);
	}

	public static void TryJoinPlayer(string message) {
		client.SendTo(Encoding.ASCII.GetBytes("JNP" + message), server);
	}

	public static void LeavePlayer() {
		client.SendTo(Encoding.ASCII.GetBytes("LVP"), server);
	}

	public static void StartGame() {
		if (!inGame) {
			client.SendTo(Encoding.ASCII.GetBytes("SRT"), server);
		}
	}

	public static void ExitGame() {
		if (canStart) {
			if (inGame)
				client.SendTo(Encoding.ASCII.GetBytes("EXT"), server);
		}
		else {
			//if not online, treat like normal
			ServerManager.localMultiplayer = true;
			SceneController.ChangeScene("Main Menu");
		}
	}

	public static void Concede() {
		if (inGame && ServerManager.CheckIfClient(null)) {
			client.SendTo(Encoding.ASCII.GetBytes("CND"), server);
		}
	}

	public static void Close() {
		if (!canStart)	return;

		//make it stall
		//client.Blocking = true;
		client.SendTo(Encoding.ASCII.GetBytes("LAP"), server);

		//reset player id
		playerId = -1;

		//release the resource
		client.Shutdown(SocketShutdown.Both);
		client.Close();
	}

	private void Update() {
		//only if client is existing
		if (!canStart)	return;
		Debug.Log("y");

		try {
			recv = client.Receive(recBuffer);
			if (recv > 0) {
				TestMessage(recBuffer, recv);
			}
		}
		catch (SocketException sock) {
			if (sock.SocketErrorCode != SocketError.WouldBlock) {
				Debug.Log(sock.ToString());
			}
		}
	}

	//apparently not necessary
	//string textBuffer = "";

	void TestMessage(byte[] buffer, int size) {
		//textBuffer += message;
		string textBuffer = Encoding.ASCII.GetString(buffer, 0, size);
		
		int index = textBuffer.IndexOf(terminator);
		//in cases of overflow
		while (index > 1) {
			//check if words
			ParseMessage(textBuffer.Substring(0, 3),
				Encoding.ASCII.GetBytes(textBuffer.Substring(0, index)), index - msgCodeSize);

			//get rid of everything
			textBuffer = textBuffer.Substring(index + 1);
			index = textBuffer.IndexOf(terminator);
		}
	}

	void ParseMessage(string code, byte[] buffer, int size) {
		//get code
		if (code == "MSG") {
			chat.UpdateChat(Encoding.ASCII.GetString(buffer, msgCodeSize, size));
		}
		else if (code == "NTF") {
			chat.UpdateChat("<color=" + notificationColour + ">"
				+ Encoding.ASCII.GetString(buffer, msgCodeSize, size) + "</color>");
		}
		
		//possibly move these all into a not in game section
		//could also be in a seperate class, or at least moving the above to seperate classes
		else if (code == "CNM") {
			//changed username
			username = Encoding.ASCII.GetString(buffer, msgCodeSize, size);
			usernameText.text = username;
		}
		else if (code == "PIN") {
			//update all the players
			//string message = Encoding.ASCII.GetString(buffer, msgCodeSize, size);
			//updatePlayerList?.Invoke(inLobby, message);
			updatePlayerList?.Invoke(inLobby, Encoding.ASCII.GetString(buffer, msgCodeSize, size));
		}
		else if (code == "LIN") {
			//string message = Encoding.ASCII.GetString(buffer, msgCodeSize, size);

			updateLobbyList?.Invoke(Encoding.ASCII.GetString(buffer, msgCodeSize, size));
		}
		else if (code == "DTY") {
			dirty?.Invoke();
		}
		else if (code == "CLB") {
			//string message = Encoding.ASCII.GetString(buffer, msgCodeSize, size);
			//these are error codes
			lobbyError?.Invoke(Encoding.ASCII.GetString(buffer, msgCodeSize, size));
			//the join lobby code will come later
		}
		else if (code == "JLB") {
			//if joining a lobby
			inLobby = true;

			lobbyName = Encoding.ASCII.GetString(buffer, msgCodeSize, size);

			joinedLobby?.Invoke(inLobby, lobbyName);
		}
		
		//can happen in gameplay
		else if (code == "LLB") {
			//if leaving lobby, decrement camera back to index 1
			inLobby = false;

			lobbyName = "";

			joinedLobby?.Invoke(inLobby, lobbyName);
		}
		
		//shouldnt happen in gameplay, etc.
		else if (code == "SRT") {
			//format is player1id/player2id
			string ids = Encoding.ASCII.GetString(buffer, msgCodeSize, size);
			
			int spliterIndex = ids.IndexOf(spliter);
			ServerManager.p1Index = int.Parse(ids.Substring(0, spliterIndex));
			ServerManager.p2Index = int.Parse(ids.Substring(spliterIndex + 1));
			
			//load game
			inGame = true;
			ServerManager.localMultiplayer = false;
			SceneController.ChangeScene(gameSceneName);
		}
		
		//consider seperating once we add gameplay loop
		else if (code == "EXT") {
			//load menu and reset multiplayer flag
			inGame = false;
			ServerManager.localMultiplayer = true;
			SceneController.ChangeScene("Main Menu");
			//also send dirty flag
			client.SendTo(Encoding.ASCII.GetBytes("DTY"), server);
		}
	}

	private void OnApplicationQuit() {
		Close();
	}
}
