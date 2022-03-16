using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class Client : MonoBehaviour
{
    byte[] buffer = new byte[512];
	Socket client;
	IPEndPoint server;

	void Start() 
    {
        //Setup our end point (server)
        try
        {
            //IPAddress ip = Dns.GetHostAddresses("mail.bigpond.com")[0];
            IPAddress ip = IPAddress.Parse("35.169.14.57");
            server = new IPEndPoint(ip, 42069);
            //create out client socket 
            client = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
            	//attempted a connection
                client.Connect(server);
				client.Blocking = false;
            }
            catch (ArgumentNullException argExc)
            {
                Debug.Log(argExc.ToString());
            }
            catch (SocketException SockExc)
            {
				Debug.Log(SockExc.ToString());
            }
            catch (Exception e)
            {
				Debug.Log(e.ToString());
            }
        }
        catch (Exception e)
        {
			Debug.Log(e.ToString());
        }

    }

	[SerializeField]	TMPro.TMP_InputField textChat;
	public void SendTextChatMessage() {
		byte[] msg = System.Text.ASCIIEncoding.ASCII.GetBytes("MSG" + textChat.text);
		client.SendTo(msg, server);
		textChat.text = "";
	}

	private void OnDestroy() {
		//release the resource
		client.SendTo(Encoding.ASCII.GetBytes("LAP"), server);
		client.Shutdown(SocketShutdown.Both);
		client.Close();
	}

	[SerializeField]	TextChat chat;
	int recv;
	private void Update() {
		try {
			recv = client.Receive(buffer);
			if (recv > 0) {
				chat.UpdateChat(Encoding.ASCII.GetString(buffer, 3, recv));
			}
		}
		catch (SocketException sock) {
			if (sock.SocketErrorCode != SocketError.WouldBlock) {
				Debug.Log(sock.ToString());
			}
		}
	}
}
