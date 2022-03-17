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
	[SerializeField] TextChat chat;
	int recv;

	private void Start() {
		if (!canStart) {
			leaveServerButton.gameObject.SetActive(false);
			chatCanvas.SetActive(false);
			if (joinOnlineButton)
				joinOnlineButton.SetActive(false);
		}
	}

	public void TryConnect() 
    {
		//dont allow empty
		if (ipInput.text == "")	return;

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
		catch (Exception e) {
			Debug.Log(e.ToString());
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
				Debug.Log(SockExc.ToString());
				ipError.text = "Inputed ip Invalid";
				connecting = false;
			}
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

	public static void Close() {
		if (!canStart)	return;

		//release the resource
		client.SendTo(Encoding.ASCII.GetBytes("LAP"), server);
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
			}
		}
		catch (SocketException sock) {
			if (sock.SocketErrorCode != SocketError.WouldBlock) {
				Debug.Log(sock.ToString());
			}
		}
	}
}
