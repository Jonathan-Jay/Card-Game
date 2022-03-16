using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class Client : MonoBehaviour
{
	const int msgCodeSize = 3;
    byte[] buffer = new byte[512];
	Socket client;
	IPEndPoint server;
	[SerializeField] TMPro.TMP_Text ipError;
	[SerializeField] TMPro.TMP_InputField ipInput;
	public float waitDuration = 5f;
	public bool connecting = false;
	public bool canStart = false;
	[SerializeField] GameObject chatCanvas;
	[SerializeField] TMPro.TMP_InputField textChat;
	[SerializeField] TextChat chat;
	int recv;

	public void TryConnect() 
    {
		//dont allow empty
		if (ipInput.text == "")	return;

		//dont if connecting
		if (connecting) return;

		StartCoroutine(ConnectionAttempt(ipInput.text));
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

		bool connected = false;
		for (float counter = 0; counter < waitDuration; counter += Time.deltaTime) {
        	try {
				recv = client.Receive(buffer);
				connected = true;
				break;
			}
			catch (SocketException SockExc) {
				if (SockExc.SocketErrorCode != SocketError.WouldBlock) {
					Debug.Log(SockExc.ToString());
					connecting = false;
				}
			}
			yield return new WaitForEndOfFrame();
		}

		if (connected) {
			//connected is true
			ipError.text = "Connected";
			chatCanvas.SetActive(true);
			yield return new WaitForEndOfFrame();
			canStart = true;
		}
		else {
			ipError.text = "Failed";
			client = null;
		}
		connecting = false;
    }

	public void SendTextChatMessage() {
		byte[] msg = System.Text.ASCIIEncoding.ASCII.GetBytes("MSG" + textChat.text);
		client.SendTo(msg, server);
		textChat.text = "";
	}

	private void OnDestroy() {
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
				chat.UpdateChat(Encoding.ASCII.GetString(buffer, msgCodeSize, recv));
			}
		}
		catch (SocketException sock) {
			if (sock.SocketErrorCode != SocketError.WouldBlock) {
				Debug.Log(sock.ToString());
			}
		}
	}
}
