using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class SynServer
{
	const int msgCodeSize = 4;
	const char terminator = '\0';
	static int sleepLength = 0;
	static byte[] dirtyMsg;
	static byte[] startMsg;

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

	public class Lobby
	{
		//keep so we send updates to the correct people
		public List<Player> players;
		//cause why not
		public string name;
		public string password;
		public int playerCount = 0;
		public Lobby(string name, string password = "") {
			players = new List<Player>();
			this.name = name;
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
	//cause why not
	const int maxLobbies = 5;

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

		dirtyMsg = Encoding.ASCII.GetBytes(terminator + "DTY");
		startMsg = Encoding.ASCII.GetBytes(terminator + "SRT");
		return true;
	}

	public static bool StandardTest(string code, Player player, Lobby lobby, int recv, ref bool dirty) {
		if (code == terminator + "MSG") {
			if (player.status != "chatting") {
				player.status = "chatting";
				dirty = true;
			}

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

			return true;
		}
		else if (code == terminator + "CNM") {
			string name = Encoding.ASCII.GetString(buffer, msgCodeSize, recv);
			if (name != player.username) {
				player.username = name;
				player.handler.SendTo(Encoding.ASCII.GetBytes(terminator + "CNM" + name), player.remoteEP);
				dirty = true;
			}

			return true;
		}

		return false;
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
			string defaultName = GetName();

			tempHandler.SendTo(Encoding.ASCII.GetBytes(terminator + "MSG" + defaultName), clientEP);
			Player player = new Player(tempHandler, defaultName);
			serverLobby.players.Add(player);

			byte[] join = Encoding.ASCII.GetBytes(terminator + "MSG" + defaultName + " joined the server");
			foreach (Player other in serverLobby.players) {
				//send to all players that user joined
				if (player == other) continue;
				other.handler.SendTo(join, other.remoteEP);
			}
			//send garbage to slow it down or smt?
			System.Threading.Thread.Sleep(sleepLength);
			tempHandler.SendTo(join, clientEP);
			System.Threading.Thread.Sleep(sleepLength * 2);
			tempHandler.SendTo(dirtyMsg, clientEP);
			System.Threading.Thread.Sleep(sleepLength);
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
					if (StandardTest(code, player, serverLobby, recv, ref dirty)) {
						//means it got completed
					}
					else if (code == terminator + "CLB") {
						string name = Encoding.ASCII.GetString(buffer, msgCodeSize, recv);
						//create a lobby with these stats if possible
						if (lobbies.Count >= maxLobbies) {
							//send error code or smt
							player.handler.SendTo(Encoding.ASCII.GetBytes(terminator + "CLBMax Number Of Lobbies"), player.remoteEP);
						}
						else {
							//create lobby if name avaible, then signal player to join it
							bool exists = false;
							for (int l = 0; l < lobbies.Count; ++l) {
								if (lobbies[l].name == name) {
									//invalid, dont let them
									exists = true;
									player.handler.SendTo(Encoding.ASCII.GetBytes(terminator + "CLBLobby already exists"), player.remoteEP);
									break;
								}
							}
							if (!exists) {
								int index = lobbies.Count;
								lobbies.Add(new Lobby(name));
								dirty = true;

								player.handler.SendTo(Encoding.ASCII.GetBytes(terminator + "JLB"
									+ name), player.remoteEP);

								//make the player join the lobby
								serverLobby.players.RemoveAt(i);
								lobbies[index].players.Add(player);
								player.status = "In Lobby";
								continue;
							}
						}
					}
					else if (code == terminator + "JLB") {
						string message = Encoding.ASCII.GetString(buffer, msgCodeSize, recv);
						if (char.IsDigit(message[0])) {
							//make them join the lobby if it's valid
							int index = int.Parse(message.Substring(0));
							if (index < lobbies.Count) {
								//they don't need the index, the index doesn't really matter
								player.handler.SendTo(Encoding.ASCII.GetBytes(terminator + "JLB"
									+ lobbies[index].name), player.remoteEP);

								dirty = true;

								serverLobby.players.RemoveAt(i);
								lobbies[index].players.Add(player);
								player.status = "In Lobby";
								continue;
							}
						}
							}
					else if (code == terminator + "LAP") {
						//left app?
						Console.WriteLine(player.username + " left the server");
						byte[] left = Encoding.ASCII.GetBytes(terminator + "MSG" + player.username + " left the server");

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

			//do lobby stuff
			bool ldirty = false;
			for (int i = 0; i < lobby.players.Count;) {
				Player player = lobby.players[i];

				try {
					recv = player.handler.Receive(buffer) - msgCodeSize;
					if (recv >= 0) {
						//do something with it
						string code = Encoding.ASCII.GetString(buffer, 0, msgCodeSize);
						Console.WriteLine(code + " " + lobby.name);
						if (StandardTest(code, player, lobby, recv, ref ldirty)) {
							//maybe will have a use idk
						}
						else if (code == terminator + "SRT") {
							//starting game, send all players into game, can also probably ignore dirty tags
							foreach (Player other in lobby.players) {
								other.status = "Gaming";
								other.handler.SendTo(startMsg, other.remoteEP);
							}
							ldirty = false;
						}
						else if (code == terminator + "LLB") {
							//left the lobby, move them back
							serverLobby.players.Add(player);
							lobby.players.RemoveAt(i);
							//send them the fact that they did
							player.handler.SendTo(Encoding.ASCII.GetBytes(terminator + "LLB"), player.remoteEP);
							continue;
						}
						else if (code == terminator + "LAP") {
							//left app?
							Console.WriteLine(player.username + " left the server");
							byte[] left = Encoding.ASCII.GetBytes(terminator + "MSG" + player.username + " left the server");

							lobby.players.RemoveAt(i);
							foreach (Player other in lobby.players) {
								//send to all players that user left
								other.handler.SendTo(left, other.remoteEP);
							}
							player.status = "Waiting";
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

			//check for players that left and stuff
			if (lobby.players.Count == 0) {
				//all players left, close the lobby
				Console.WriteLine("Lobby " + lobby.name + " deleted");
				lobbies.RemoveAt(j);
				dirty = true;
				continue;
			}

			//if the player number changed or dirty
			if (ldirty || lobby.playerCount != lobby.players.Count) {
				lobby.playerCount = lobby.players.Count;
				Console.WriteLine("Dirty " + lobby.name);

				System.Threading.Thread.Sleep(sleepLength * 2);
				foreach (Player player in lobby.players) {
					player.handler.SendTo(dirtyMsg, player.remoteEP);
				}
				foreach (Player other in lobby.players) {
					//update playerlist in the lobby
					System.Threading.Thread.Sleep(sleepLength);
					foreach (Player player in lobby.players) {
						player.handler.SendTo(Encoding.ASCII.GetBytes(terminator + "PIN"
							+ other.status + "$" + other.username), player.remoteEP);
					}
				}
				dirty = true;
				ldirty = false;
			}
			++j;
		}

		//update lobby ui with new info
		if (dirty || serverLobby.playerCount != serverLobby.players.Count) {
			//sleep for a moment
			//System.Threading.Thread.Sleep(10);

			Console.WriteLine("Dirty server");
			serverLobby.playerCount = serverLobby.players.Count;

			System.Threading.Thread.Sleep(sleepLength * 2);
			foreach (Player player in serverLobby.players) {
				player.handler.SendTo(dirtyMsg, player.remoteEP);
			}
			for (int i = 0; i < lobbies.Count; ++i) {
				System.Threading.Thread.Sleep(sleepLength);
				//send lobby list
				foreach (Player player in serverLobby.players) {
					System.Threading.Thread.Sleep(sleepLength);
					player.handler.SendTo(Encoding.ASCII.GetBytes(terminator + "LIN"
						+ lobbies[i].playerCount + "$" + i + lobbies[i].name), player.remoteEP);
				}
			}
			foreach (Player other in serverLobby.players) {
				System.Threading.Thread.Sleep(sleepLength);
				//send player data
				foreach (Player player in serverLobby.players) {
					System.Threading.Thread.Sleep(sleepLength);
					player.handler.SendTo(Encoding.ASCII.GetBytes(terminator + "PIN"
						+ other.status + "$" + other.username), player.remoteEP);
				}
			}
			dirty = false;
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
