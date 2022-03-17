using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class Client : MonoBehaviour
{
	public const int msgCodeSize = 3;
    public static byte[] buffer = new byte[512];
	public static Socket client;
	public static IPEndPoint server;
	public static string username {get; private set;} = "";

	[SerializeField] TMPro.TMP_Text usernameText;
	[SerializeField] TMPro.TMP_Text ipError;
	[SerializeField] TMPro.TMP_InputField ipInput;
	public float waitDuration = 5f;
	bool connecting = false;
	public static bool canStart { get; private set;} = false;
	[SerializeField] GameObject chatCanvas;
	[SerializeField] GameObject joinOnlineButton;
	[SerializeField] UnityEngine.UI.Button joinServerButton;
	[SerializeField] UnityEngine.UI.Button leaveServerButton;
	[SerializeField] TMPro.TMP_InputField textChat;
	[SerializeField] TMPro.TMP_Text lobbyName;
	[SerializeField] TMPro.TMP_Text lobbyError;
	[SerializeField] TextChat chat;
	[SerializeField] CameraController cam;
	[SerializeField] UITemplateList lobbyList;
	[SerializeField] UITemplateList playerList;
	bool inLobby = false;
	[SerializeField] UITemplateList inLobbyPlayerList;
	int recv;

	private void Start() {
		if (!canStart) {
			leaveServerButton.gameObject.SetActive(false);
			chatCanvas.SetActive(false);
			if (joinOnlineButton)
				joinOnlineButton.SetActive(false);
		}
		else {
			leaveServerButton.gameObject.SetActive(true);
			chatCanvas.SetActive(true);
			if (joinOnlineButton)
				joinOnlineButton.SetActive(true);
		}
	}

	public void TryConnect() 
    {
		//dont allow empty
		if (ipInput.text == "")	return;

		if (!connecting)
			StartCoroutine(ConnectionAttempt(ipInput.text));
	}

	public void LeaveServer() {
		if (!canStart)	return;

		usernameText.text = "";
		joinOnlineButton.SetActive(false);
		leaveServerButton.gameObject.SetActive(false);
		joinServerButton.gameObject.SetActive(true);
		ipInput.interactable = true;
		ipError.text = "Left";
		if (chatCanvas.activeInHierarchy)
			chatCanvas.SetActive(false);
		Close();
		canStart = false;
	}

	IEnumerator ConnectionAttempt(string ipText) {
		connecting = true;
		ipError.text = "Connecting...";
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
			ipInput.text = "";
			ipError.text = "Inputed ip Invalid";
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
				ipInput.text = "";
				ipError.text = "Inputed ip Invalid";
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
		ipInput.interactable = false;
		joinServerButton.interactable = false;

		bool connected = false;
		for (float counter = 0; counter < waitDuration; counter += Time.deltaTime) {
        	try {
				//because client.Connected don't work lol
				recv = client.Receive(buffer) - msgCodeSize;
				if (recv >= 0) {
					username = Encoding.ASCII.GetString(buffer, msgCodeSize, recv);
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

		joinServerButton.interactable = true;
		if (connected) {
			//connected is true
			ipError.text = "Connected";
			chatCanvas.SetActive(true);
			joinOnlineButton.SetActive(true);
			leaveServerButton.gameObject.SetActive(true);
			joinServerButton.gameObject.SetActive(false);

			yield return new WaitForEndOfFrame();
			canStart = true;
		}
		else {
			ipError.text = "Failed";
			ipInput.interactable = true;
			client = null;
		}
		connecting = false;
    }

	public static void SendMessage(byte[] byteMsg) {
		client.SendTo(byteMsg, server);
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

	public static void StartGame() {
		client.SendTo(Encoding.ASCII.GetBytes("SRT"), server);
	}

	public static void Close() {
		if (!canStart)	return;

		//make it stall
		client.Blocking = true;
		client.SendTo(Encoding.ASCII.GetBytes("LAP"), server);
		//release the resource
		client.Shutdown(SocketShutdown.Both);
		client.Close();
	}

	private void Update() {
		//only if client is existing
		if (!canStart)	return;

		try {
			recv = client.Receive(buffer) - msgCodeSize;
			if (recv >= 0) {
				//get code
				string code = Encoding.ASCII.GetString(buffer, 0, msgCodeSize);
				if (code == "MSG") {
					chat.UpdateChat(Encoding.ASCII.GetString(buffer, msgCodeSize, recv));
				}
				else if (code == "CNM") {
					//changed username
					username = Encoding.ASCII.GetString(buffer, msgCodeSize, recv);
					usernameText.text = username;
				}
				else if (code == "PIN") {
					//update all the players
					string message = Encoding.ASCII.GetString(buffer, msgCodeSize, recv);

					if (inLobby)
						inLobbyPlayerList.CreateProfile(message);
					else
						playerList.CreateProfile(message);
				}
				else if (code == "LIN") {
					string message = Encoding.ASCII.GetString(buffer, msgCodeSize, recv);

					lobbyList.CreateProfile(message);
				}
				else if (code == "DTY") {
					if (inLobby) {
						inLobbyPlayerList.Clear();
					}
					else {
						playerList.Clear();
						lobbyList.Clear();
					}
				}
				else if (code == "CLB") {
					string message = Encoding.ASCII.GetString(buffer, msgCodeSize, recv);
					//these are error codes
					lobbyError.text = message;
					//the join lobby code will come later
				}
				else if (code == "JLB") {
					//lobby data will be sent later, just change lobby name for now
					lobbyName.text = Encoding.ASCII.GetString(buffer, msgCodeSize, recv);

					//if joining a lobby, move the camera up one if in index 1
					if (cam.index == 1) {
						cam.IncrementIndex(false);
						inLobby = true;
					}
				}
				else if (code == "LLB") {
					//leave lobby, so reset name
					lobbyName.text = "";

					//if leaving lobby, decrement camera back to index 1
					if (cam.index == 2) {
						cam.DecrementIndex(false);
						inLobby = false;
					}
				}
				else if (code == "SRT") {
					//load game
					ServerManager.localMultiplayer = false;
					SceneController.ChangeScene("SampleScene");
				}
			}
		}
		catch (SocketException sock) {
			if (sock.SocketErrorCode != SocketError.WouldBlock) {
				Debug.Log(sock.ToString());
			}
		}
	}
}
