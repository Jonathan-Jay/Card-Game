using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class SynServer
{
	public struct Player
	{
		public Socket handler;
		public string username;
		public string status;
		public Player(Socket handler, string username) {
			this.handler = handler;
			this.username = username;
			this.status = "New";
		}
	}

	public struct Lobby
	{
		//keep so we send updates to the correct people
		public List<Player> players;
		//cause why not
		public string password;
		public Lobby(string password) {
			players = new List<Player>();
			this.password = password;
		}
	}
	static byte[] buffer = new byte[512];
	static Socket server;
	//when checking new players
	static Socket tempHandler;

	//using a lobby for the server for ease of use lol
	static Lobby serverLobby = new Lobby("");
	static List<Lobby> lobbies = new List<Lobby>();



	public static void StartServer(int maxPlayers, string localIP) {
		IPAddress ip = IPAddress.Parse(localIP);
		IPEndPoint localEP = new IPEndPoint(ip, 11111);

		server = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		server.Blocking = false;

		try {
			server.Bind(localEP);
			//how many people before it drops the rest
			server.Listen(maxPlayers);
		}
		catch (Exception e) {
			Console.WriteLine(e.ToString());
		}
	}

	static bool RunServer() {
		/*maintain the multiple different aspects of the server
		 * listen for new players
		 * manage each lobby
		*/

		//see if new player joining
		try {
			tempHandler = server.Accept();

			IPEndPoint clientEP = (IPEndPoint)tempHandler.RemoteEndPoint;
			//Print Client info (IP and PORT)
			Console.WriteLine("Client {0} connected at port {1}", clientEP.Address, clientEP.Port);
		}
		catch (SocketException sockExcep) {
			//if error isn't blocking related, send
			if (sockExcep.SocketErrorCode == SocketError.WouldBlock) {
				//send the new player joining to all others?
				//also wait till the player responds with their username
				serverLobby.players.Add(new Player(tempHandler, ""));
				tempHandler = null;
			}
			else {
				Console.WriteLine(sockExcep.ToString());
			}
		}
		catch (Exception e) {
			Console.WriteLine(e.ToString());
		}

		int recv;
		//listen to all players in the lobby
		foreach (Player player in serverLobby.players) {
			//get data, if empty, we can ignore
			recv = player.handler.Receive(buffer);
			if (recv > 0) {
				//do something with it
				Console.Write("test");
			}
		}

		return true;
	}

	static void CloseServer() {
		//close all handlers

		tempHandler.Shutdown(SocketShutdown.Both);
		tempHandler.Close();
	}

	public static int Main(string[] args) {
		StartServer(10, "127.0.0.1");
		while(RunServer());
		CloseServer();
		Console.ReadKey();
		return 0;
	}
}
