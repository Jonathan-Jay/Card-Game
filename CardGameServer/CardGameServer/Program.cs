#define PRINT_TO_CONSOLE

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class SynServer
{
	const int msgCodeSize = 3;
	//msg codes are also sent here
	const int gameCodeSize = 40 + msgCodeSize;
	const char terminator = '\r';
	const char spliter = '\t';
	const string player1Code = "P1";
	const string player2Code = "P2";

	//static byte[] pingMsg;
	static byte[] dirtyMsg;
	static byte[] startMsg;
	static byte[] exitMsg;
	static byte[] leftLBMsg;
	static byte[] p1LeftMsg;
	static byte[] p2LeftMsg;

	public class Player
	{
		public Socket handler;
		public IPEndPoint remoteEP;
		public IPEndPoint udpEP = null;
		public string username;
		public string status;
		public int id;
		public bool inGame = false;

		public Player(Socket handler, string username, int id) {
			this.handler = handler;
			this.remoteEP = (IPEndPoint)handler.RemoteEndPoint;
			this.username = username;
			this.id = id;
			this.status = "New";
		}

		public void Kill() {
			#if PRINT_TO_CONSOLE
			Console.WriteLine("Player \"" + username + "\" was terminated");
			#endif

			handler.Shutdown(SocketShutdown.Both);
			handler.Close();
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
		public bool inGame = false;

		public int player1 = -1;
		public int player2 = -1;

		public Socket udpSocket = null;
		public EndPoint remote = null;
		public int udpPort = -1;

		public Lobby(string name, string password = "") {
			players = new List<Player>();
			this.name = name;
			this.password = password;
		}

		public void Kill() {
			#if PRINT_TO_CONSOLE
			Console.WriteLine("Lobby \"" + name + "\" was terminated");
			#endif

			udpSocket.Shutdown(SocketShutdown.Both);
			udpSocket.Close();
		}
	}
	
	static byte[] buffer = new byte[256];
	static Socket server;
	//each lobby has their own for ease of use (dont have to find the specific server the udp was received on)
	//this is to make it unique per lobby
	static int udpSocketPort = 4200;

	//when checking new players
	static Socket tempHandler = null;
	static IPEndPoint tempIPRemote = null;

	static int playerCount = 0;

	//using a lobby for the server for ease of use lol
	static Lobby serverLobby = new Lobby("");
	static List<Lobby> lobbies = new List<Lobby>();
	//cause why not
	const int maxLobbies = 5;

	static string GetName() {
		return "NewUser" + (playerCount + 1);
	}

	static IPAddress ip = null;

	//return true on success
	public static bool StartServer(int maxQueuedPlayers) {
		IPEndPoint localEP = new IPEndPoint(ip, 42069);

		server = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		
		//we can wait for the first player
		//server.Blocking = false;

		try {
			server.Bind(localEP);
			//how many people before it drops the rest
			server.Listen(maxQueuedPlayers);
		}
		catch (Exception e) {
			Console.WriteLine(e.ToString());
			return false;
		}

		//pingMsg = Encoding.ASCII.GetBytes(terminator.ToString());
		dirtyMsg = Encoding.ASCII.GetBytes("DTY" + terminator);
		startMsg = Encoding.ASCII.GetBytes("SRT" + terminator);
		exitMsg = Encoding.ASCII.GetBytes("EXT" + terminator);
		leftLBMsg = Encoding.ASCII.GetBytes("LLB" + terminator);
		p1LeftMsg = Encoding.ASCII.GetBytes("LVP" + player1Code + terminator);
		p2LeftMsg = Encoding.ASCII.GetBytes("LVP" + player2Code + terminator);

		server.BeginAccept(new AsyncCallback(AsyncAccept), null);
		return true;
	}

	public static bool StandardTest(string code, Player player, Lobby lobby, int compoundIndex, int length, ref bool dirty) {
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
				+ Encoding.ASCII.GetString(buffer, compoundIndex, length) + terminator);

			foreach (Player other in lobby.players) {
				//don't ignore self
				//if (other == player) continue;
				other.handler.SendTo(message, other.remoteEP);
			}

			return true;
		}
		else if (code == "CNM") {
			string name = Encoding.ASCII.GetString(buffer, compoundIndex, length);
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

	static System.Threading.Mutex asyncJoinMutex = new System.Threading.Mutex();

	static void AsyncAccept(IAsyncResult result) {
		try {
			//tempHandler = server.Accept();
			tempHandler = server.EndAccept(result);

			IPEndPoint clientEP = (IPEndPoint)tempHandler.RemoteEndPoint;

			tempHandler.Blocking = false;
			string defaultName = GetName();

			Player player = new Player(tempHandler, defaultName, ++playerCount);

			tempHandler.SendTo(Encoding.ASCII.GetBytes(player.id.ToString() + spliter
				+ defaultName + terminator), clientEP);

			asyncJoinMutex.WaitOne(1000);

			serverLobby.players.Add(player);

			asyncJoinMutex.ReleaseMutex();

			#if PRINT_TO_CONSOLE
			//Print Client info (IP and PORT)
			Console.WriteLine("Client {0} connected at port {1}", clientEP.Address, clientEP.Port);
			Console.WriteLine("Player \"{0}\" id: {1}", defaultName, player.id);
			#endif

			//actually means first player
			if (playerCount == 1) {
				#if PRINT_TO_CONSOLE
				Console.WriteLine("stopped blocking");
				#endif

				server.Blocking = false;
			}

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

		//restart the loop
		server.BeginAccept(new AsyncCallback(AsyncAccept), null);
	}

	static bool RunServer() {
		/*maintain the multiple different aspects of the server
		 * listen for new players
		 * manage each lobby
		*/

		//see if new player joining, hopefully all we need to do is slap this into a function to make it async
		/*try {
			tempHandler = server.Accept();

			IPEndPoint clientEP = (IPEndPoint)tempHandler.RemoteEndPoint;

			tempHandler.Blocking = false;
			string defaultName = GetName();

			Player player = new Player(tempHandler, defaultName, ++playerCount);
			tempHandler.SendTo(Encoding.ASCII.GetBytes(player.id.ToString() + spliter
				+ defaultName + terminator), clientEP);

			serverLobby.players.Add(player);

			#if PRINT_TO_CONSOLE
			//Print Client info (IP and PORT)
			Console.WriteLine("Client {0} connected at port {1}", clientEP.Address, clientEP.Port);
			Console.WriteLine("Player \"{0}\" id: {1}", defaultName, player.id);
			#endif

			//actually means first player
			if (playerCount == 1) {
				#if PRINT_TO_CONSOLE
				Console.WriteLine("stopped blocking");
				#endif

				server.Blocking = false;
			}

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
		}*/

		//just returns true aka skips if empty (because accept will be async)
		if (server.Blocking) {
			return true;
		}

		if (lobbies.Count == 0 && serverLobby.players.Count == 0) {
			#if PRINT_TO_CONSOLE
			Console.WriteLine("no more players, resetting id");
			#endif

			playerCount = 0;
			server.Blocking = true;
			return true;
		}

		bool dirty = false;
		int recv;

		//consider a way to not use this every frame?
		asyncJoinMutex.WaitOne();
		//listen to all players in the lobby
		for (int i = 0; i < serverLobby.players.Count;) {
			Player player = serverLobby.players[i];
			bool skip = false;

			try {
				if (player.handler.Available == 0) {
					++i;
					continue;
				}

				recv = player.handler.Receive(buffer);
				if (recv >= 0) {
					//do something with it
					//Console.Write(ASCIIEncoding.ASCII.GetString(buffer, 0, recv));
					//string code = Encoding.ASCII.GetString(buffer, 0, msgCodeSize);
					string wholeMsg = Encoding.ASCII.GetString(buffer, 0, recv);

					//check if compressed messages
					int compoundIndex = msgCodeSize;
					int length = wholeMsg.IndexOf(terminator) - msgCodeSize;

					string code = wholeMsg.Substring(0, msgCodeSize);

					while (length >= 0) {
						#if PRINT_TO_CONSOLE
						Console.WriteLine(code + " " + length);
						#endif

						if (StandardTest(code, player, serverLobby, compoundIndex, length, ref dirty)) {
							//means it got completed
						}
						else if (code == "CLB") {
							string name = Encoding.ASCII.GetString(buffer, compoundIndex, length);
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
									Lobby lobby = lobbies[index];

									//create the udp socket
									lobby.udpSocket = new Socket(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
									EndPoint remote = null;
									while (lobby.remote == null) {
										remote = new IPEndPoint(ip, ++udpSocketPort);
										try {
											lobby.udpSocket.Bind(remote);
											lobby.udpSocket.Blocking = false;

											lobby.udpPort = udpSocketPort;

											lobby.remote = new IPEndPoint(IPAddress.Any, 0);
										}
										catch (SocketException sockExcep) {
											if (sockExcep.SocketErrorCode != SocketError.AddressAlreadyInUse) {
												Console.WriteLine(sockExcep.ToString());
												return false;
											}
											remote = null;
										}
										catch (Exception e) {
											Console.WriteLine(e.ToString());
											return false;
										}
									}

									#if PRINT_TO_CONSOLE
									Console.WriteLine("Lobby " + name + " created on udp Port " + udpSocketPort);
									#endif

									//send the port to them
									player.handler.SendTo(Encoding.ASCII.GetBytes("JLB"
										+ lobby.player1.ToString() + spliter
										+ lobby.player2.ToString() + spliter
										+ lobby.udpPort.ToString() + spliter
										+ name + terminator), player.remoteEP);

									//make the player join the lobby
									serverLobby.players.RemoveAt(i);
									lobby.players.Add(player);

									byte[] join = Encoding.ASCII.GetBytes("NTF" + player.username
										+ " joined the lobby" + terminator);

									foreach (Player other in lobby.players) {
										//send to all players that user left
										other.handler.SendTo(join, other.remoteEP);
									}

									player.status = "In Lobby: " + name;
									dirty = true;
									skip = true;
								}
							}
						}
						else if (code == "JLB") {
							string message = Encoding.ASCII.GetString(buffer, compoundIndex, length);
							if (char.IsDigit(message[0])) {
								//make them join the lobby if it's valid
								int index = int.Parse(message);
								if (index < lobbies.Count) {
									//they don't need the index, the index doesn't really matter
									player.handler.SendTo(Encoding.ASCII.GetBytes("JLB"
										+ lobbies[index].player1.ToString() + spliter
										+ lobbies[index].player2.ToString() + spliter
										+ lobbies[index].udpPort.ToString() + spliter
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
									skip = true;
								}
							}
						}
						else if (code == "LAP" || !player.handler.Connected) {
							//left app?
							#if PRINT_TO_CONSOLE
							Console.WriteLine(player.username + " left the server");
							#endif

							byte[] left = Encoding.ASCII.GetBytes("NTF" + player.username
								+ " left the server" + terminator);

							player.Kill();
							serverLobby.players.RemoveAt(i);
							foreach (Player other in serverLobby.players) {
								//send to all players that user left
								other.handler.SendTo(left, other.remoteEP);
							}
							dirty = true;
							skip = true;
						}
						//last because this should be the first thign we receive from players, so uncommon
						else if (code == "UDP") {
							//it's their earliest available udp port
							player.udpEP = new IPEndPoint(((IPEndPoint)player.handler.RemoteEndPoint).Address,
								int.Parse(Encoding.ASCII.GetString(buffer, compoundIndex, length)));
							//if it crashes, idk how that happened lol

							//mark dirty to also let the player reget everything
							dirty = true;
						}

						//plus one for terminator skip
						compoundIndex += length + msgCodeSize + 1;

						if (compoundIndex >= recv)
							break;

						length = wholeMsg.IndexOf(terminator, compoundIndex) - msgCodeSize;
						code = wholeMsg.Substring(compoundIndex - msgCodeSize, msgCodeSize);
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

						player.Kill();
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
			if (!skip)
				++i;
		}
		asyncJoinMutex.ReleaseMutex();

		//check all other lobbies
		for (int j = 0; j < lobbies.Count;) {
			Lobby lobby = lobbies[j];

			//do lobby stuff
			bool ldirty = false;
			for (int i = 0; i < lobby.players.Count;) {
				Player player = lobby.players[i];
				bool skip = false;

				try {
					if (player.handler.Available == 0) {
						++i;
						continue;
					}

					recv = player.handler.Receive(buffer);
					if (recv >= 0) {
						//do something with it
						//string code = Encoding.ASCII.GetString(buffer, 0, msgCodeSize);
						string wholeMsg = Encoding.ASCII.GetString(buffer, 0, recv);

						//check if compressed messages
						int compoundIndex = msgCodeSize;
						int length = wholeMsg.IndexOf(terminator) - msgCodeSize;

						string code = wholeMsg.Substring(0, msgCodeSize);

						while (length >= 0 || code == "COD") {
							#if PRINT_TO_CONSOLE
							Console.WriteLine(code + " " + length + "/" + recv + " " + lobby.name);
							#endif

							if (StandardTest(code, player, lobby, compoundIndex, length, ref ldirty)) {
								//maybe will have a use idk
							}
							else if (!lobby.inGame) {
								if (code == "SRT") {
									//make sure it's a valid start, and it's a player attempting
									//valid if both players assigned and a player attempted to start the game
									if (lobby.player1 >= 0 && lobby.player2 >= 0 &&
										(lobby.player1 == player.id || lobby.player2 == player.id)) {
										//if no udp, we can just set to true
										lobby.inGame = true;

										//starting game, send all players into game, can also probably ignore dirty tags
										foreach (Player other in lobby.players) {
											other.inGame = true;
											other.status = "Gaming";
											//other.status = "In Game";
											other.handler.SendTo(startMsg, other.remoteEP);
										}

										ldirty = false;
									}
								}
								else if (code == "LLB") {
									//make them quit if in game (somehow) and exit
									//players really shouldn't be able to do this...
									//if (player.inGame) {
									//	player.inGame = false;
									//	player.handler.SendTo(exitMsg, player.remoteEP);
									//}
									//else {
									//if a player, disconnect from table
									if (lobby.player1 == player.id) {
										lobby.player1 = -1;
										//resend player data codes
										foreach (Player other in lobby.players) {
											other.handler.SendTo(p1LeftMsg, other.remoteEP);
										}
									}
									else if (lobby.player2 == player.id) {
										lobby.player2 = -1;
										//resend player data codes
										foreach (Player other in lobby.players) {
											other.handler.SendTo(p2LeftMsg, other.remoteEP);
										}
									}
									//}

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
									ldirty = true;
									skip = true;
								}
								else if (code == "JNP") {
									//get which player they want to join
									string input = Encoding.ASCII.GetString(buffer, compoundIndex, length);
									byte[] message = null;

									if (input == player1Code) {
										if (lobby.player1 < 0) {
											lobby.player1 = player.id;
											player.status = "In Lobby: " + lobby.name + " as Player 1";
											ldirty = true;

											message = Encoding.ASCII.GetBytes("JNP"
												+ lobby.player1.ToString() + spliter
												+ player1Code + terminator);
										}
									}
									else if (input == player2Code) {
										if (lobby.player2 < 0) {
											lobby.player2 = player.id;
											player.status = "In Lobby: " + lobby.name + " as Player 2";
											ldirty = true;

											message = Encoding.ASCII.GetBytes("JNP"
												+ lobby.player2.ToString() + spliter
												+ player2Code + terminator);
										}
									}
									//send confirmation if valid
									if (message != null) {
										foreach (Player other in lobby.players) {
											other.handler.SendTo(message, other.remoteEP);
										}
									}
								}
								else if (code == "LVP") {
									byte[] message = null;

									//were they actually the player?
									if (lobby.player1 == player.id) {
										lobby.player1 = -1;
										player.status = "In Lobby: " + lobby.name;
										ldirty = true;

										message = p1LeftMsg;
									}
									else if (lobby.player2 == player.id) {
										lobby.player2 = -1;
										player.status = "In Lobby: " + lobby.name;
										ldirty = true;

										message = p2LeftMsg;
									}

									if (message != null) {
										foreach (Player other in lobby.players) {
											other.handler.SendTo(message, other.remoteEP);
										}
									}
								}
								else if (code == "LAP" || !player.handler.Connected) {
									//left app?
									#if PRINT_TO_CONSOLE
									Console.WriteLine(player.username + " left the server");
									#endif

									//check if they were a player, and if so, remove it
									if (lobby.player1 == player.id) {
										lobby.player1 = -1;
										foreach (Player other in lobby.players) {
											other.handler.SendTo(p1LeftMsg, other.remoteEP);
										}
									}
									else if (lobby.player2 == player.id) {
										lobby.player2 = -1;
										foreach (Player other in lobby.players) {
											other.handler.SendTo(p2LeftMsg, other.remoteEP);
										}
									}


									byte[] left = Encoding.ASCII.GetBytes("NTF" + player.username
										+ " left the server" + terminator);

									player.Kill();
									lobby.players.RemoveAt(i);
									foreach (Player other in lobby.players) {
										//send to all players that user left
										other.handler.SendTo(left, other.remoteEP);
									}
									ldirty = true;
									skip = true;
								}
							}
							//lobby is in game
							else {
								if (code == "COD") {
									byte[] start = null;
									//repackage and send to everyone else
									//should only be from the game players
									if (player.id == lobby.player1) {
										start = Encoding.ASCII.GetBytes("COD" + player1Code);
									}
									else if (player.id == lobby.player2) {
										start = Encoding.ASCII.GetBytes("COD" + player2Code);
									}

									if (start != null) {
										byte[] message = new byte[start.Length + gameCodeSize];
										Array.Copy(start, 0, message, 0, start.Length);
										//if player sends too much data it'll trim it, so oh well
										//if (recv - msgCodeSize > gameCodeSize) {
										//too big, idk what happened
										//}
										Array.Copy(buffer, compoundIndex, message, start.Length, gameCodeSize);

										#if PRINT_TO_CONSOLE
										//debugging purposes
										Console.WriteLine("COD size " + length + ": " + Encoding.ASCII.GetString(message, msgCodeSize, gameCodeSize + 2));
										#endif

										foreach (Player other in lobby.players) {
											//ignore self
											if (player == other) continue;
											other.handler.SendTo(message, other.remoteEP);
										}

										//make length the proper skip size
										length = gameCodeSize - 1;
									}
								}
								else if (code == "SRT") {
									//should only be spectators that aren't in game
									/*if (!player.inGame) {
										player.inGame = true;
										player.status = "Gaming";
										//other.status = "In Game";
										player.handler.SendTo(startMsg, player.remoteEP);

										ldirty = false;
									}
									*/
									//maybe we dont do this...
								}
								else if (code == "EXT") {
									//if player, make everyone quit
									if (lobby.player1 == player.id || lobby.player2 == player.id) {
										lobby.inGame = false;

										//exiting game, send them back
										foreach (Player other in lobby.players) {
											//if they already exited, dont bother
											if (!other.inGame) continue;

											other.inGame = false;
											if (lobby.player1 == other.id) {
												other.status = "In Lobby: " + lobby.name + " as Player 1";
											}
											else if (lobby.player2 == other.id) {
												other.status = "In Lobby: " + lobby.name + " as Player 2";
											}
											else {
												other.status = "In Lobby: " + lobby.name;
											}
											other.handler.SendTo(exitMsg, other.remoteEP);
										}
										ldirty = false;
									}
									else {
										//just this player exits
										player.inGame = false;
										player.status = "In Lobby: " + lobby.name;
										player.handler.SendTo(exitMsg, player.remoteEP);
									}
								}
								else if (code == "CND") {
									//for now jsut exist for testing purposes
									lobby.inGame = false;

									//exiting game, send them back
									foreach (Player other in lobby.players) {
										//if they already exited, dont bother
										if (!other.inGame) continue;

										other.inGame = false;
										if (lobby.player1 == other.id) {
											other.status = "In Lobby: " + lobby.name + "as Player 1";
										}
										else if (lobby.player2 == other.id) {
											other.status = "In Lobby: " + lobby.name + "as Player 2";
										}
										else {
											other.status = "In Lobby: " + lobby.name;
										}
										other.handler.SendTo(exitMsg, other.remoteEP);
									}
									ldirty = false;
								}
								else if (code == "LLB") {
									//make them quit if in game (somehow) and exit
									//players really shouldn't be able to do this...
									if (player.inGame) {
										player.inGame = false;
										player.handler.SendTo(exitMsg, player.remoteEP);
									}

									//if this happens, big trouble lol, well you could always make the spectators leave
									//if (lobby.player1 == player.id) {
									//	lobby.player1 = -1;
									//}
									//else if (lobby.player2 == player.id) {
									//	lobby.player1 = -1;
									//}

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
									ldirty = true;
									skip = true;
								}
								else if (code == "LAP" || !player.handler.Connected) {
									//left app?
									#if PRINT_TO_CONSOLE
									Console.WriteLine(player.username + " left the server");
									#endif

									//if this happens, big trouble lol
									//try to send the concede message, so figure something out for that
									if (lobby.player1 == player.id) {
										lobby.player1 = -1;
									}
									else if (lobby.player2 == player.id) {
										lobby.player1 = -1;
									}

									byte[] left = Encoding.ASCII.GetBytes("NTF" + player.username
										+ " left the server" + terminator);

									player.Kill();
									lobby.players.RemoveAt(i);
									foreach (Player other in lobby.players) {
										//send to all players that user left
										other.handler.SendTo(left, other.remoteEP);
									}
									ldirty = true;
									skip = true;
								}
							}

							//plus one for terminator skip
							compoundIndex += length + msgCodeSize + 1;

							if (compoundIndex >= recv)
								break;

							length = wholeMsg.IndexOf(terminator, compoundIndex) - msgCodeSize;
							code = wholeMsg.Substring(compoundIndex - msgCodeSize, msgCodeSize);
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

							//check if they were a player, and if so, remove it
							if (lobby.player1 == player.id) {
								lobby.player1 = -1;
							}
							else if (lobby.player2 == player.id) {
								lobby.player2 = -1;
							}

							byte[] left = Encoding.ASCII.GetBytes("NTF" + player.username
								+ " left the server" + terminator);

							player.Kill();
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

				if (!skip)
					++i;
			}

			//check for players that left and stuff
			if (lobby.players.Count == 0) {
				//all players left, close the lobby
				#if PRINT_TO_CONSOLE
				Console.WriteLine("Lobby " + lobby.name + " deleted");
#endif

				lobby.Kill();
				lobbies.RemoveAt(j);

				//check if that was the last lobby
				if (lobbies.Count == 0) {
					//if so, reset the udp port number
					udpSocketPort = 420;
				}

				dirty = true;
				continue;
			}

			//after all the tcp, do udp stuff if gaming
			if (lobby.inGame) {
				try {
					recv = lobby.udpSocket.ReceiveFrom(buffer, ref lobby.remote);

					tempIPRemote = (IPEndPoint)lobby.remote;
					if (recv > 0) {
						//send it to everyone else, ignore the one who sent it, the data should be properly formatted
						foreach (Player player in lobby.players) {
							if (tempIPRemote.Port == player.udpEP.Port &&
								tempIPRemote.Address.GetHashCode() == player.udpEP.Address.GetHashCode())
							{
								continue;
							}
							lobby.udpSocket.SendTo(buffer, recv, SocketFlags.None, player.udpEP);
						}
					}
					tempIPRemote = null;

					lobby.remote = new IPEndPoint(IPAddress.Any, 0);
				}
				catch (SocketException sockExcep) {
					if (sockExcep.SocketErrorCode != SocketError.WouldBlock) {
						Console.WriteLine(sockExcep.ToString());
					}
				}
				catch (Exception e) {
					Console.WriteLine(e.ToString());
				}
			}

			//if the player number changed or dirty
			if (ldirty || lobby.playerCount != lobby.players.Count) {
				lobby.playerCount = lobby.players.Count;

				//send dirty flag to non playing players
				foreach (Player player in lobby.players) {
					if (!player.inGame)
						player.handler.SendTo(dirtyMsg, player.remoteEP);
				}

				#if PRINT_TO_CONSOLE
				Console.WriteLine("Dirty " + lobby.name);
				#endif

				foreach (Player other in lobby.players) {
					byte[] message = Encoding.ASCII.GetBytes("PIN" + other.status
						+ spliter + other.id + spliter + other.username + terminator);
					//update playerlist in the lobby that arent in game
					foreach (Player player in lobby.players) {
						if (!player.inGame)
							player.handler.SendTo(message, player.remoteEP);
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

			#if PRINT_TO_CONSOLE
			Console.WriteLine("Dirty server");
			#endif

			serverLobby.playerCount = serverLobby.players.Count;

			foreach (Player player in serverLobby.players) {
				player.handler.SendTo(dirtyMsg, player.remoteEP);
			}
			foreach (Player other in serverLobby.players) {
				byte[] message = Encoding.ASCII.GetBytes("PIN" + other.status
					+ spliter + other.id + spliter + other.username + terminator);
				//send player data
				foreach (Player player in serverLobby.players) {
					player.handler.SendTo(message, player.remoteEP);
				}
			}
			for (int i = 0; i < lobbies.Count; ++i) {
				foreach (Player other in lobbies[i].players) {
					byte[] pmsg = Encoding.ASCII.GetBytes("PIN" + other.status
						+ spliter + other.id + spliter + other.username + terminator);
					//send player data
					foreach (Player player in serverLobby.players) {
						player.handler.SendTo(pmsg, player.remoteEP);
					}
				}
				byte[] message = Encoding.ASCII.GetBytes("LIN" + lobbies[i].playerCount
					+ spliter + i.ToString() + spliter + lobbies[i].name + terminator);
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

		//Kill the serverlobby an dall lobbies before clearing
		serverLobby.Kill();
		foreach (Lobby lobby in lobbies) {
			lobby.Kill();
		}
		lobbies.Clear();

		//close all sockets
		server.Shutdown(SocketShutdown.Both);
		server.Close();
	}

	public static int Main(string[] args) {

		Console.Write("Type IP address (blank for host ip): ");
		string input = Console.ReadLine();
		if (input == "") {
			ip = Dns.GetHostAddresses(Dns.GetHostName())[1];
		}
		else {
			ip = IPAddress.Parse(input);
		}

		//if the ip fails
		if (!StartServer(3)) {
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
