using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class SynServer
{
	const int msgCodeSize = 3;
	public class Player
	{
		public Socket handler;
		public EndPoint remoteEP;
		public string username;
		public string status;
		public Player(Socket handler, string username) {
			this.handler = handler;
			this.remoteEP = (EndPoint)handler.RemoteEndPoint;
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


	static string GetName() {
		return "Joe";
	}

	public static void StartServer(int maxPlayers, IPAddress ip) {
		IPEndPoint localEP = new IPEndPoint(ip, 42069);

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
			tempHandler.Blocking = false;
			serverLobby.players.Add(new Player(tempHandler, GetName()));
		}
		catch (SocketException sockExcep) {
			//if error isn't blocking related, send
			if (sockExcep.SocketErrorCode == SocketError.WouldBlock) {
				//send the new player joining to all others?
				//also wait till the player responds with their username
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
		for (int i = 0; i < serverLobby.players.Count;) {
			Player player = serverLobby.players[i];

			try {
				recv = player.handler.Receive(buffer) - msgCodeSize;
				if (recv > 0) {
					//do something with it
					//Console.Write(ASCIIEncoding.ASCII.GetString(buffer, 0, recv));
					string code = Encoding.ASCII.GetString(buffer, 0, msgCodeSize);
					if (code == "MSG") {
						//create message
						byte[] start = Encoding.ASCII.GetBytes("MSG" + player.username + ": ");
						byte[] message = new byte[start.Length + recv];
						Buffer.BlockCopy(start, 0, message, 0, start.Length);
						Buffer.BlockCopy(buffer, msgCodeSize, message, start.Length, recv);

						foreach (Player other in serverLobby.players) {
							//ignore self
							//if (other == player) continue;
							player.handler.Send(message);
						}
					}
					else if (code == "LAP") {
						//left app?
						Console.WriteLine(player.username + " left the server");
						serverLobby.players.RemoveAt(i);
						continue;
					}
				}
			}
			catch (SocketException sockExcep) {
				//we can ignore this one
				if (sockExcep.SocketErrorCode != SocketError.WouldBlock) {
					Console.WriteLine(sockExcep.ToString());
				}
			}
			catch (Exception e) {
				Console.WriteLine(e.ToString());
			}
			++i;
		}

		return true;
	}

	static void CloseServer() {
		//close all handlers

		tempHandler.Shutdown(SocketShutdown.Both);
		tempHandler.Close();
	}

	public static int Main(string[] args) {
		StartServer(10, Dns.GetHostAddresses(Dns.GetHostName())[1]);
		while(RunServer());
		CloseServer();
		Console.ReadKey();
		return 0;
	}
}
