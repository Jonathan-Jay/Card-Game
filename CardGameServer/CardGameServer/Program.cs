#define PRINT_TO_CONSOLE

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class SynServer
{
	const int msgCodeSize = 3;
	const char terminator = '\r';
	const char spliter = '\t';
	//static int sleepLength = 0;
	//static byte[] pingMsg;
	static byte[] dirtyMsg;
	static byte[] startMsg;
	static byte[] exitMsg;
	static byte[] leftLBMsg;

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

		//from: https://docs.microsoft.com/en-us/dotnet/api/system.threading.timer?view=net-6.0
		public System.Threading.Timer counter;
		public int playerCount = 0;
		public bool inGame = false;
		public Lobby(string name, string password = "") {
			players = new List<Player>();
			this.name = name;
			this.password = password;
			//30 second delay
			//counter = new System.Threading.Timer(Ping, players, 30000, 30000);
			//5 for testing
			//counter = new System.Threading.Timer(Ping, players, 5000, 5000);
		}

		//~Lobby() {
			//Console.WriteLine("Killed the timer");
			//counter.Dispose();
		//}

		//void Ping(Object test) {
			//Console.WriteLine("Pinged: " + name);
			//go through each player and ping
			//foreach (Player player in players) {
				//try {
					//player.handler.Send(pingMsg);
				//}
				//catch (Exception) {
					//just catch it
				//}
			//}
		//}
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

		//pingMsg = Encoding.ASCII.GetBytes(terminator.ToString());
		dirtyMsg = Encoding.ASCII.GetBytes("DTY" + terminator);
		startMsg = Encoding.ASCII.GetBytes("SRT" + terminator);
		leftLBMsg = Encoding.ASCII.GetBytes("LLB" + terminator);
		exitMsg = Encoding.ASCII.GetBytes("EXT" + terminator);
		return true;
	}

	public static bool StandardTest(string code, Player player, Lobby lobby, int recv, ref bool dirty) {
		if (code == "MSG") {
			if (lobby == serverLobby && player.status != "Chatting") {
				player.status = "Chatting";
				dirty = true;
			}

			//create message
			//byte[] start = Encoding.ASCII.GetBytes("MSG" + player.username + ": " + terminator);
			//byte[] message = new byte[start.Length + recv];
			//Buffer.BlockCopy(start, 0, message, 0, start.Length);
			//Buffer.BlockCopy(buffer, msgCodeSize, message, start.Length, recv);
			byte[] message = Encoding.ASCII.GetBytes("MSG" + player.username + ": "
				+ Encoding.ASCII.GetString(buffer, msgCodeSize, recv) + terminator);

			foreach (Player other in lobby.players) {
				//don't ignore self
				//if (other == player) continue;
				other.handler.SendTo(message, other.remoteEP);
			}

			return true;
		}
		else if (code == "CNM") {
			string name = Encoding.ASCII.GetString(buffer, msgCodeSize, recv);
			if (name != player.username) {
				player.handler.SendTo(Encoding.ASCII.GetBytes("CNM"
					+ name + terminator), player.remoteEP);

				byte[] message = Encoding.ASCII.GetBytes("NTF" + player.username
					+ " changed their name to " + name + terminator);

				foreach (Player other in lobby.players) {
					//don't ignore self
					//if (other == player) continue;
					other.handler.SendTo(message, other.remoteEP);
				}
				player.username = name;

				dirty = true;
			}

			return true;
		}
		else if (code == "DTY") {
			//player wants to get refreshed
			dirty = true;
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

			#if PRINT_TO_CONSOLE
			//Print Client info (IP and PORT)
			Console.WriteLine("Client {0} connected at port {1}", clientEP.Address, clientEP.Port);
			#endif

			tempHandler.Blocking = false;
			string defaultName = GetName();

			tempHandler.SendTo(Encoding.ASCII.GetBytes(defaultName + terminator), clientEP);
			Player player = new Player(tempHandler, defaultName);
			serverLobby.players.Add(player);

			byte[] join = Encoding.ASCII.GetBytes("NTF" + defaultName
					+ " joined the server" + terminator);
			foreach (Player other in serverLobby.players) {
				//send to all players that user joined
				//if (player == other) continue;
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

					#if PRINT_TO_CONSOLE
					Console.WriteLine(code);
					#endif

					if (StandardTest(code, player, serverLobby, recv, ref dirty)) {
						//means it got completed
					}
					else if (code == "CLB") {
						string name = Encoding.ASCII.GetString(buffer, msgCodeSize, recv);
						//create a lobby with these stats if possible
						if (lobbies.Count >= maxLobbies) {
							//send error code or smt
							player.handler.SendTo(Encoding.ASCII.GetBytes(
								"CLBMax Number Of Lobbies" + terminator), player.remoteEP);
						}
						else {
							//create lobby if name avaible, then signal player to join it
							bool exists = false;
							for (int l = 0; l < lobbies.Count; ++l) {
								if (lobbies[l].name == name) {
									//invalid, dont let them
									exists = true;
									player.handler.SendTo(Encoding.ASCII.GetBytes(
										"CLBLobby already exists" + terminator), player.remoteEP);
									break;
								}
							}
							if (!exists) {
								int index = lobbies.Count;
								lobbies.Add(new Lobby(name));

								player.handler.SendTo(Encoding.ASCII.GetBytes("JLB"
									+ name + terminator), player.remoteEP);

								//make the player join the lobby
								serverLobby.players.RemoveAt(i);
								lobbies[index].players.Add(player);

								byte[] join = Encoding.ASCII.GetBytes("NTF" + player.username
									+ " joined the lobby" + terminator);

								foreach (Player other in lobbies[index].players) {
									//send to all players that user left
									other.handler.SendTo(join, other.remoteEP);
								}

								player.status = "In Lobby: " + name;
								dirty = true;
								continue;
							}
						}
					}
					else if (code == "JLB") {
						string message = Encoding.ASCII.GetString(buffer, msgCodeSize, recv);
						if (char.IsDigit(message[0])) {
							//make them join the lobby if it's valid
							int index = int.Parse(message.Substring(0));
							if (index < lobbies.Count) {
								//they don't need the index, the index doesn't really matter
								player.handler.SendTo(Encoding.ASCII.GetBytes("JLB"
									+ lobbies[index].name + terminator), player.remoteEP);

								serverLobby.players.RemoveAt(i);
								lobbies[index].players.Add(player);

								byte[] join = Encoding.ASCII.GetBytes("NTF" + player.username
									+ " joined the lobby" + terminator);

								foreach (Player other in lobbies[index].players) {
									//send to all players that user left
									other.handler.SendTo(join, other.remoteEP);
								}

								player.status = "In Lobby: " + lobbies[index].name;
								dirty = true;
								continue;
							}
						}
							}
					else if (code == "LAP") {
						//left app?
						#if PRINT_TO_CONSOLE
						Console.WriteLine(player.username + " left the server");
						#endif

						byte[] left = Encoding.ASCII.GetBytes("NTF" + player.username
							+ " left the server" + terminator);

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
					if (sockExcep.SocketErrorCode == SocketError.ConnectionAborted) {
						//make the player leave
						#if PRINT_TO_CONSOLE
						Console.WriteLine(player.username + " lost connection");
						#endif

						byte[] left = Encoding.ASCII.GetBytes("NTF" + player.username
							+ " left the server" + terminator);

						serverLobby.players.RemoveAt(i);
						foreach (Player other in serverLobby.players) {
							//send to all players that user left
							other.handler.SendTo(left, other.remoteEP);
						}
						dirty = true;
						continue;
					}
					else {
						Console.WriteLine(sockExcep.ToString());
					}
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

						#if PRINT_TO_CONSOLE
						Console.WriteLine(code + " " + lobby.name);
						#endif

						if (StandardTest(code, player, lobby, recv, ref ldirty)) {
							//maybe will have a use idk
						}
						else if (code == "SRT") {
							lobby.inGame = true;
							//starting game, send all players into game, can also probably ignore dirty tags
							foreach (Player other in lobby.players) {
								other.status = "Gaming";
								//other.status = "In Game";
								other.handler.SendTo(startMsg, other.remoteEP);
							}
							ldirty = false;
						}
						else if (code == "EXT") {
							lobby.inGame = false;
							//exiting game, send them back
							foreach (Player other in lobby.players) {
								other.status = "In Lobby: " + lobby.name;
								other.handler.SendTo(exitMsg, other.remoteEP);
							}
							ldirty = false;
						}
						else if (code == "LLB") {
							//left the lobby, move them back
							serverLobby.players.Add(player);
							lobby.players.RemoveAt(i);
							//send to all people in the lobby
							if (lobby.players.Count > 0) {
								byte[] left = Encoding.ASCII.GetBytes("NTF" + player.username
									+ " left the lobby" + terminator);

								foreach (Player other in lobby.players) {
									//send to all players that user left
									other.handler.SendTo(left, other.remoteEP);
								}
							}
							//send them the fact that they did
							player.handler.SendTo(leftLBMsg, player.remoteEP);

							player.status = "Waiting";
							continue;
						}
						else if (code == "LAP") {
							//left app?
							#if PRINT_TO_CONSOLE
							Console.WriteLine(player.username + " left the server");
							#endif

							byte[] left = Encoding.ASCII.GetBytes("NTF" + player.username
								+ " left the server" + terminator);

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
						if (sockExcep.SocketErrorCode == SocketError.ConnectionAborted) {
							//make the player leave
							#if PRINT_TO_CONSOLE
							Console.WriteLine(player.username + " lost connection");
							#endif

							byte[] left = Encoding.ASCII.GetBytes("NTF" + player.username
								+ " left the server" + terminator);

							lobby.players.RemoveAt(i);
							foreach (Player other in lobby.players) {
								//send to all players that user left
								other.handler.SendTo(left, other.remoteEP);
							}
							dirty = true;
							continue;
						}
						else {
							Console.WriteLine(sockExcep.ToString());
						}
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
				#if PRINT_TO_CONSOLE
				Console.WriteLine("Lobby " + lobby.name + " deleted");
				#endif

				lobbies.RemoveAt(j);
				dirty = true;
				continue;
			}

			//if the player number changed or dirty
			if (ldirty || lobby.playerCount != lobby.players.Count) {
				lobby.playerCount = lobby.players.Count;

				//assume we need the firty flag always
				foreach (Player player in lobby.players) {
					player.handler.SendTo(dirtyMsg, player.remoteEP);
				}
				if (lobby.inGame) {
					#if PRINT_TO_CONSOLE
					Console.WriteLine("Dirty Game " + lobby.name);
					#endif
				}
				else {
					#if PRINT_TO_CONSOLE
					Console.WriteLine("Dirty " + lobby.name);
					#endif

					foreach (Player other in lobby.players) {
						byte[] message = Encoding.ASCII.GetBytes("PIN" + other.status
							+ spliter + other.username + terminator);
						//update playerlist in the lobby
						foreach (Player player in lobby.players) {
							player.handler.SendTo(message, player.remoteEP);
						}
					}
					//only change lobby if the players themselves changed
					dirty = true;
				}
				ldirty = false;
			}
			++j;
		}

		//update lobby ui with new info
		if (dirty || serverLobby.playerCount != serverLobby.players.Count) {
			//sleep for a moment
			//System.Threading.Thread.Sleep(10);

			#if PRINT_TO_CONSOLE
			Console.WriteLine("Dirty server");
			#endif

			serverLobby.playerCount = serverLobby.players.Count;

			foreach (Player player in serverLobby.players) {
				player.handler.SendTo(dirtyMsg, player.remoteEP);
			}
			foreach (Player other in serverLobby.players) {
				byte[] message = Encoding.ASCII.GetBytes("PIN"
					+ other.status + spliter + other.username + terminator);
				//send player data
				foreach (Player player in serverLobby.players) {
					player.handler.SendTo(message, player.remoteEP);
				}
			}
			for (int i = 0; i < lobbies.Count; ++i) {
				foreach (Player other in lobbies[i].players) {
					byte[] pmsg = Encoding.ASCII.GetBytes("PIN"
						+ other.status + spliter + other.username + terminator);
					//send player data
					foreach (Player player in serverLobby.players) {
						player.handler.SendTo(pmsg, player.remoteEP);
					}
				}
				byte[] message = Encoding.ASCII.GetBytes("LIN" + lobbies[i].playerCount
					+ spliter + i.ToString() + lobbies[i].name + terminator);
				//send lobby list
				foreach (Player player in serverLobby.players) {
					player.handler.SendTo(message, player.remoteEP);
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
