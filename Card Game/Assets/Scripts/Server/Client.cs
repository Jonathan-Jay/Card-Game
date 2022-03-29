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
	//dont forget message code is added to game code, currently up to 40 bytes
	public const int gameCodeSize = 40 + msgCodeSize;
	public const char terminator = '\r';
	public const string player1Code = "P1";
	public const string player2Code = "P2";
    public static byte[] recBuffer = new byte[512];
	public static Socket client;
	public static Socket udpClient;
	public static IPEndPoint server;
	public static IPEndPoint udpServer;
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
	public event Action tableSeatUpdated;
	public event Action<byte[]> gameCodeReceived;
	public event Action<byte[]> udpEvent;

	public static WaitForSeconds DesyncCompensation = new WaitForSeconds(0.5f);

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
			tableSeatUpdated?.Invoke();
			//refresh list somehow
			//client.SendTo(Encoding.ASCII.GetBytes("DTY"), server);
			SendStringMessage("DTY");
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

		IPAddress ip = null;

        //Setup our end point (server)
        try {
            //IPAddress ip = Dns.GetHostAddresses("mail.bigpond.com")[0];
            ip = IPAddress.Parse(ipText);
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

					//send garbage for refreshing data, it'll now be through the udp command
					//client.SendTo(Encoding.ASCII.GetBytes("DTY"), server);

					//we need to get the index of the terminator
					int index2 = username.IndexOf(terminator, ++index);

					//get the index check for if there is more to the message
					int index3 = username.IndexOf(terminator, index2 + 1) - index2;

					//now we can trim username from the spliter to terminator
					username = username.Substring(index, index2 - index);
					usernameText.text = username;

					//send whatever might be after the name
					if (index3 > 1) {
						byte[] testMessage = new byte[recv - index3];
						Array.Copy(recBuffer, index2 + 1, testMessage, 0, testMessage.Length);

						TestMessage(testMessage, testMessage.Length);
					}
				}

				//now we can create our udp socket and send it to the server
				udpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

				//start searching from this port onwards
				int udpPort = 4200;
				EndPoint remote = null;
				while (!connected && udpPort < 5000) {
					remote = new IPEndPoint(IPAddress.Any, ++udpPort);
					try {
						udpClient.Bind(remote);
						udpClient.Blocking = false;

						connected = true;
					}
					catch (SocketException sockExcep) {
						if (sockExcep.SocketErrorCode != SocketError.AddressAlreadyInUse) {
							Debug.Log(sockExcep.ToString());
						}
					}
					catch (Exception e) {
						Debug.Log(e.ToString());
					}
				}

				//if it broke, it'd hit this
				if (udpPort < 5000) {
					//send the udpPort now
					//client.SendTo(Encoding.ASCII.GetBytes("UDP" + udpPort.ToString()), server);
					SendStringMessage("UDP" + udpPort.ToString());
				}

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
		if (textChat.text == "")	return;

		//client.SendTo(Encoding.ASCII.GetBytes("MSG" + textChat.text), server);
		SendStringMessage("MSG" + textChat.text);
		textChat.text = "";
	}

	public static void SendStringMessage(string message) {
		client.SendTo(Encoding.ASCII.GetBytes(message + terminator), server);
	}

	public static void ChangeUserName(TMPro.TMP_InputField username) {
		if (username.text == "" || username.text == Client.username)	return;

		//client.SendTo(Encoding.ASCII.GetBytes("CNM" + username.text), server);
		SendStringMessage("CNM" + username.text);
	}

	public static void CreateLobby(TMPro.TMP_InputField input) {
		if (input.text == "")	return;

		//client.SendTo(Encoding.ASCII.GetBytes("CLB" + input.text), server);
		SendStringMessage("CLB" + input.text);
	}

	public static void JoinLobby(int index) {
		//client.SendTo(Encoding.ASCII.GetBytes("JLB" + index), server);
		SendStringMessage("JLB" + index);
	}

	public static void LeaveLobby() {
		//client.SendTo(Encoding.ASCII.GetBytes("LLB"), server);
		SendStringMessage("LLB");
	}

	public static void JoinPlayer(bool player1) {
		if (player1)
			//client.SendTo(Encoding.ASCII.GetBytes("JNP" + player1Code), server);
			SendStringMessage("JNP" + player1Code);
		else
			//client.SendTo(Encoding.ASCII.GetBytes("JNP" + player2Code), server);
			SendStringMessage("JNP" + player2Code);
	}

	public static void LeavePlayer() {
		//client.SendTo(Encoding.ASCII.GetBytes("LVP"), server);
		SendStringMessage("LVP");
	}

	public static void StartGame() {
		if (!inGame)
			//client.SendTo(Encoding.ASCII.GetBytes("SRT"), server);
			SendStringMessage("SRT");
	}

	public static void ExitGame() {
		if (canStart) {
			if (inGame)
				//client.SendTo(Encoding.ASCII.GetBytes("EXT"), server);
				SendStringMessage("EXT");
		}
		else {
			//if not online, treat like normal
			ServerManager.localMultiplayer = true;
			SceneController.ChangeScene("Main Menu");
		}
	}

	static byte[] gameStartCode = Encoding.ASCII.GetBytes("COD");
	public static void SendGameData(in byte[] data) {
		//for now jsut send it all, trimed to max size
		byte[] message = new byte[gameStartCode.Length + gameCodeSize];

		//add the start
		Buffer.BlockCopy(gameStartCode, 0, message, 0, gameStartCode.Length);
		//add the actual message
		Buffer.BlockCopy(data, 0, message, gameStartCode.Length, Mathf.Min(data.Length, gameCodeSize));

		client.SendTo(message, server);
	}

	public static void SendUDP(in byte[] data) {
		udpClient.SendTo(data, udpServer);
	}

	public static void Concede() {
		if (inGame && ServerManager.CheckIfClient(null, false)) {
			//client.SendTo(Encoding.ASCII.GetBytes("CND"), server);
			SendStringMessage("CND");
		}
	}

	public static void Close() {
		if (!canStart)	return;

		//make it stall
		//client.Blocking = true;
		//client.SendTo(Encoding.ASCII.GetBytes("LAP"), server);
		SendStringMessage("LAP");

		//reset player id
		playerId = -1;

		//release the resource
		client.Shutdown(SocketShutdown.Both);
		client.Close();
	}

	private void Update() {
		//only if client is existing
		if (!canStart)	return;

		try {
			if (client.Available > 0) {
				recv = client.Receive(recBuffer);
				if (recv > 0) {
					TestMessage(recBuffer, recv);
				}
			}
		}
		catch (SocketException sock) {
			if (sock.SocketErrorCode != SocketError.WouldBlock) {
				Debug.Log(sock.ToString());
			}
		}
		
		try {
			if (udpClient.Available > 0) {
				recv = udpClient.Receive(recBuffer);
				if (recv > 0) {
					byte[] message = new byte[recv];
					Array.Copy(recBuffer, message, recv);
					udpEvent?.Invoke(message);
				}
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

	void TestMessage(in byte[] buffer, int size) {
		//textBuffer += message;
		string textBuffer = Encoding.ASCII.GetString(buffer, 0, size);

		//in cases of overflow
		if (textBuffer.Length < 3)	return;

		//for going through bytes
		int index = textBuffer.IndexOf(terminator);
		int compoundIndex = 0;

		string code = textBuffer.Substring(0, 3);

		while (index > 1 || code == "COD") {
			if (code != "COD") {
				byte[] message = new byte[index - msgCodeSize];
				Array.Copy(buffer, compoundIndex + msgCodeSize, message, 0, index - msgCodeSize);
				//check if words
				ParseMessage(code, message, index - msgCodeSize);
			}
			//it's a game code
			else {
				//playerCode.length only works because of ascii encoding, consider thinking of that

				//send the message plus the terminator
				byte[] message = new byte[player1Code.Length + gameCodeSize];
				Array.Copy(buffer, compoundIndex + msgCodeSize, message, 0, message.Length);

				gameCodeReceived?.Invoke(message);

				//undo possibly terminator
				index = message.Length + msgCodeSize - 1;
				//reset this
				code = "";
			}

			//update compound index and termninator
			compoundIndex += index + 1;

			if (compoundIndex >= size) {
				break;
			}

			//get rid of everything below the thing
			textBuffer = Encoding.ASCII.GetString(buffer, compoundIndex, size - compoundIndex);

			index = textBuffer.IndexOf(terminator);

			//only retrieve code if it's longer'
			if (textBuffer.Length > 2) {
				code = textBuffer.Substring(0, 3);
			}
		}
	}

	//buffer is without code
	void ParseMessage(string code, in byte[] buffer, int size) {
		//could go back to else if if you wanna remove some depending on if in game or not
		
		switch (code) {
			case "MSG": {
				chat.UpdateChat(Encoding.ASCII.GetString(buffer, 0, size));
				break;
			}
			case "NTF": {
				chat.UpdateChat("<color=" + notificationColour + ">"
					+ Encoding.ASCII.GetString(buffer, 0, size) + "</color>");
				break;
			}
			case "CNM": {
				//changed username
				username = Encoding.ASCII.GetString(buffer, 0, size);
				usernameText.text = username;
				break;
			}
			//menu related
			case "PIN": {
				//update all the players
				//string message = Encoding.ASCII.GetString(buffer, msgCodeSize, size);
				//updatePlayerList?.Invoke(inLobby, message);
				updatePlayerList?.Invoke(inLobby, Encoding.ASCII.GetString(buffer, 0, size));
				break;
			}
			case "LIN": {
				//string message = Encoding.ASCII.GetString(buffer, msgCodeSize, size);

				updateLobbyList?.Invoke(Encoding.ASCII.GetString(buffer, 0, size));
				break;
			}
			case "DTY": {
				dirty?.Invoke();
				break;
			}
			case "CLB": {
				//string message = Encoding.ASCII.GetString(buffer, msgCodeSize, size);
				//these are error codes
				lobbyError?.Invoke(Encoding.ASCII.GetString(buffer, 0, size));
				//the join lobby code will come later
				break;
			}
			case "JLB": {
				//if joining a lobby (also receive current players)
				inLobby = true;

				lobbyName = Encoding.ASCII.GetString(buffer, 0, size);
				
				int index = lobbyName.IndexOf(spliter);
				ServerManager.p1Index = int.Parse(lobbyName.Substring(0, index));

				lobbyName = lobbyName.Substring(index + 1);
				index = lobbyName.IndexOf(spliter);
				ServerManager.p2Index = int.Parse(lobbyName.Substring(0, index));
				
				//get the udp port for sending stuff
				lobbyName = lobbyName.Substring(index + 1);
				index = lobbyName.IndexOf(spliter);
				udpServer = new IPEndPoint(server.Address, int.Parse(lobbyName.Substring(0, index)));

				lobbyName = lobbyName.Substring(index + 1);
				joinedLobby?.Invoke(inLobby, lobbyName);

				tableSeatUpdated?.Invoke();
				break;
			}
			case "JNP": {
				//this player joined the table
				string message = Encoding.ASCII.GetString(buffer, 0, size);
				int index = message.IndexOf(spliter);
				//get the index
				int id = int.Parse(message.Substring(0, index));

				//now you can trim the message
				message = message.Substring(index + 1);

				if (message == player1Code) {
					//we now have the id of Player1
					ServerManager.p1Index = id;
				}
				else if (message == player2Code) {
					//we now have the id of Player2
					ServerManager.p2Index = id;
				}

				tableSeatUpdated?.Invoke();

				//otherwise wtf happened
				break;
			}
			case "LVP": {
				//this player left the table
				string message = Encoding.ASCII.GetString(buffer, 0, size);

				if (message == player1Code) {
					//we now have the id of Player1
					ServerManager.p1Index = -1;
				}
				else if (message == player2Code) {
					//we now have the id of Player2
					ServerManager.p2Index = -1;
				}

				tableSeatUpdated?.Invoke();

				//otherwise wtf happened
				break;
			}
			case "LLB": {
				//if leaving lobby, decrement camera back to index 1
				inLobby = false;

				//clear this
				if (udpServer != null)
					udpServer = null;

				lobbyName = "";

				joinedLobby?.Invoke(inLobby, lobbyName);
				break;
			}
			case "SRT": {
				//format is player1id/player2id
				//string ids = Encoding.ASCII.GetString(buffer, msgCodeSize, size);

				//int spliterIndex = ids.IndexOf(spliter);
				//ServerManager.p1Index = int.Parse(ids.Substring(0, spliterIndex));
				//ServerManager.p2Index = int.Parse(ids.Substring(spliterIndex + 1));
				//dont need these

				//load game
				inGame = true;
				ServerManager.localMultiplayer = false;
				SceneController.ChangeScene(gameSceneName);
				break;
			}
			//more game related
			case "EXT": {
				//load menu and reset multiplayer flag
				inGame = false;
				ServerManager.localMultiplayer = true;
				SceneController.ChangeScene("Main Menu");
				//also send dirty flag
				//client.SendTo(Encoding.ASCII.GetBytes("DTY"), server);
				SendStringMessage("DTY");
				break;
			}
		}
	}

	private void OnApplicationQuit() {
		Close();
	}
}
