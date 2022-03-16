﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class SynServer
{
	const int msgCodeSize = 3;
	static byte[] joinMsg;
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

	static int num = 0;
	static string GetName() {
		return "NewUser" + (num++);
	}

	//return true on success
	public static bool StartServer(int maxPlayers, IPAddress ip) {
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
			return false;
		}

		joinMsg = Encoding.ASCII.GetBytes("JND");
		return true;
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
			tempHandler.Send(joinMsg);

			string defaultName = GetName();
			serverLobby.players.Add(new Player(tempHandler, defaultName));

			byte[] join = Encoding.ASCII.GetBytes("MSG" + defaultName + " joined the server");
			foreach (Player other in serverLobby.players) {
				//send to all players that user joined
				other.handler.SendTo(join, other.remoteEP);
			}
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

		bool dirty = false;
		int recv;
		//listen to all players in the lobby
		for (int i = 0; i < serverLobby.players.Count;) {
			Player player = serverLobby.players[i];

			try {
				recv = player.handler.Receive(buffer) - msgCodeSize;
				if (recv >= 0) {
					//do something with it
					//Console.Write(ASCIIEncoding.ASCII.GetString(buffer, 0, recv));
					string code = Encoding.ASCII.GetString(buffer, 0, msgCodeSize);
					Console.WriteLine(code);
					if (code == "MSG") {
						if (player.status != "chatting") {
							player.status = "chatting";
							dirty = true;
						}

						//create message
						byte[] start = Encoding.ASCII.GetBytes("MSG" + player.username + ": ");
						byte[] message = new byte[start.Length + recv];
						Buffer.BlockCopy(start, 0, message, 0, start.Length);
						Buffer.BlockCopy(buffer, msgCodeSize, message, start.Length, recv);

						foreach (Player other in serverLobby.players) {
							//don't ignore self
							//if (other == player) continue;
							other.handler.SendTo(message, other.remoteEP);
						}
					}
					else if (code == "LAP") {
						//left app?
						Console.WriteLine(player.username + " left the server");
						byte[] left = Encoding.ASCII.GetBytes("MSG" + player.username + " left the server");
						
						serverLobby.players.RemoveAt(i);
						foreach (Player other in serverLobby.players) {
							//send to all players that user left
							other.handler.SendTo(left, other.remoteEP);
						}
						dirty = true;
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

		//check all other lobbies
		for (int j = 0; j < lobbies.Count;) {
			Lobby lobby = lobbies[j];
			if (lobby.players.Count == 0) {
				//all players left, close the lobby
				lobbies.RemoveAt(j);
				dirty = true;
				continue;
			}
			//do lobby stuff
			bool ldirty = false;
			for (int i = 0; i < lobby.players.Count;) {
				Player player = lobby.players[i];

				try {
					recv = player.handler.Receive(buffer) - msgCodeSize;
					if (recv >= 0) {
						//do something with it
						string code = Encoding.ASCII.GetString(buffer, 0, msgCodeSize);
						Console.WriteLine(code);
						if (code == "MSG") {
							//create message
							byte[] start = Encoding.ASCII.GetBytes("MSG" + player.username + ": ");
							byte[] message = new byte[start.Length + recv];
							Buffer.BlockCopy(start, 0, message, 0, start.Length);
							Buffer.BlockCopy(buffer, msgCodeSize, message, start.Length, recv);

							foreach (Player other in lobby.players) {
								//don't ignore self
								//if (other == player) continue;
								other.handler.SendTo(message, other.remoteEP);
							}
						}
						else if (code == "LAP") {
							//left app?
							Console.WriteLine(player.username + " left the server");
							byte[] left = Encoding.ASCII.GetBytes("MSG" + player.username + " left the server");

							lobby.players.RemoveAt(i);
							foreach (Player other in lobby.players) {
								//send to all players that user left
								other.handler.SendTo(left, other.remoteEP);
							}
							ldirty = true;
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
			if (ldirty) {
				dirty = true;
				//update player list in the lobby

			}
			++j;
		}

		//update lobby ui with new info
		if (dirty) {
			//foreach (Player player in serverLobby.players) {
				//send lobbies and players in said lobbies
			//}
		}

		return true;
	}

	static void CloseServer() {
		//close all handlers

		tempHandler.Shutdown(SocketShutdown.Both);
		tempHandler.Close();
	}

	public static int Main(string[] args) {

		Console.Write("Type IP address (blank for host ip): ");
		IPAddress ip;
		string input = Console.ReadLine();
		if (input == "") {
			ip = Dns.GetHostAddresses(Dns.GetHostName())[1];
		}
		else {
			ip = IPAddress.Parse(input);
		}

		//if the ip fails
		if (!StartServer(10, ip)) {
			Console.WriteLine("Press any button to close the app...");
			Console.ReadKey();
			return -1;
		}
		Console.WriteLine("Server started on " + ip.ToString());
		while(RunServer());
		CloseServer();
		Console.WriteLine("Press any button to close the app...");
		Console.ReadKey();
		return 0;
	}
}
