using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;
using System.Threading;
using Newtonsoft.Json;
using System.IO;

namespace PServer
{
    public class Server
    {
        const string DATE_FORMAT = "MM/dd/yyyy hh:mm:ss:fff";

        static object counterLock = new object();
        static object logLock = new object();
        static StreamWriter logger = null;

        private List<Player> clients = new List<Player>();
        private List<Game> games = new List<Game>();
        private bool running = false;
        private int counter = 0;
        private object nextPlayerIDLock = new object();
        private int nextPlayerID = 1;

        static Server()
        {
            logger = new StreamWriter(File.Open("PServer.log", FileMode.Append));
            logger.AutoFlush = true;
        }

        public void Start()
        {
            Log("Server started at: " + DateTime.Now.ToString(DATE_FORMAT));
            var server = new WebSocketServer("ws://localhost:7750");
            running = true;
            server.Start((socket) => SetupClient(socket));

            // keep the main thread alive..
            while (running)
            {
                Thread.Sleep(1000);
            }
        }
        public void Stop()
        {
            logger.Close();
            running = false;
        }

        private Player AddClient(IWebSocketConnection c)
        {
            Player p = new Player { WebSocket = c, State = PlayerState.CONNECTED };
            clients.Add(p);
            return p;
        }

        private void SetupClient(IWebSocketConnection c)
        {
            try
            {
                c.OnOpen = () =>
                {
                    Server.Log("[" + c.ConnectionInfo.ClientIpAddress + "] connected");
                    AddClient(c);
                };
                c.OnError = (ex) =>
                {
                    try
                    {
                        var p = clients.FirstOrDefault(x => x.WebSocket == c);
                        if (p != null) clients.Remove(p);
                    }
                    catch (Exception) { };
                };
                c.OnClose = () =>
                {
                    try
                    {
                        var p = clients.FirstOrDefault(x => x.WebSocket == c);
                        if (p != null) clients.Remove(p);
                    }
                    catch (Exception) { };
                };
                c.OnMessage = (s) => ReceivedClientMessage(c, s);
            }
            catch (Exception ex)
            {
                Server.Log(ex.ToString());
            }
        }

        private void ReceivedClientMessage(IWebSocketConnection c, string s)
        {
            int msgID;
            lock (counterLock)
            {
                msgID = ++counter;
            }
            Server.Log("begin: msg " + msgID);

            Player player = clients.FirstOrDefault(x => x.WebSocket == c || x.WebSocket.ConnectionInfo.Id.Equals(c.ConnectionInfo.Id));
            if (player == null)
            {
                player = AddClient(c);
            }


            string username = (player != null && player.Username != null) ? player.Username : "";
            Server.Log(String.Format("in:  [{0} <{1}>]: {2}", c.ConnectionInfo.ClientIpAddress, username, s));


            // Generate Message Classes here:
            // http://json2csharp.com/

            // Deserialize Category and ID
            try
            {
                Message request = JsonConvert.DeserializeObject<Message>(s);

                // Deserialize remainder of message to specific type
                request = Message.Deserialize(s, request.Category, request.ID);


                if (request is LoginRequest)
                    HandleRequest(player, request as LoginRequest); //1.0
                else if (request is LogoutRequest)
                    HandleRequest(player, request as LogoutRequest); //1.1
                else if (request is PingRequest)
                    HandleRequest(player, request as PingRequest); //1.2
                else if (request is ChatRequest)
                    HandleRequest(player, request as ChatRequest); //1.4
                else if (request is CreateGameRequest)
                    HandleRequest(player, request as CreateGameRequest); //2.0
                else if (request is StartGameRequest)
                    HandleRequest(player, request as StartGameRequest); //2.1
                else if (request is InvitePlayerRequest)
                    HandleRequest(player, request as InvitePlayerRequest); //2.3
                else if (request is JoinGameRequest)
                    HandleRequest(player, request as JoinGameRequest); //2.5
                else if (request is LeaveGameRequest)
                    HandleRequest(player, request as LeaveGameRequest); //2.6
                else if (request is ResetGameRequest)
                    HandleRequest(player, request as ResetGameRequest); //2.7
                else if (request is ConnectFourMoveRequest)
                    HandleRequest(player, request as ConnectFourMoveRequest); //3.100

            }
            catch (Exception ex)
            {
                Server.Log(ex.ToString());
            }
            Server.Log("end:   msg " + msgID);

        }

        private void HandleRequest(Player p, LoginRequest m)
        {
            try
            {
                if (clients.Count > 100)
                {
                    p.Send(Serialize(new LoginResponse { Success = false, Reason = "The maximum number of clients has been reached on this server.  Please try again later." }));
                }
                else
                {
                    // remove invalid chars
                    m.Username = RemoveInvalidUsernameCharacters(m.Username);

                    bool nameValid = true;
                    lock (clients)
                    {
                        foreach (var client in clients)
                        {
                            if (client.Username == m.Username)
                            {
                                nameValid = false;
                                break;
                            }
                        }

                        // reserve the name for this user
                        if (nameValid)
                            p.Username = m.Username;
                    }
                    if (nameValid)
                    {
                        lock (nextPlayerIDLock)
                        {
                            p.ID = nextPlayerID;
                            nextPlayerID++;
                        }
                        p.State = PlayerState.IN_LOBBY;
                        p.Send(Serialize(new LoginResponse { Success = true }));
                        lock (clients)
                        {
                            foreach (var player in clients.Where(x => IsInLobby(x)))
                            {
                                // tell existing players, that 'p' has connected
                                player.Send(Serialize(new UpdatePlayerMessage { PlayerID = p.ID, Status = p.State.ToString(), Username = p.Username, Reason = String.Format("{0} connected.", p.Username) }));

                                // tell 'p' that each existing player is online
                                if(player.ID != p.ID)
                                {
                                    p.Send(Serialize(new UpdatePlayerMessage { PlayerID = player.ID, Status = player.State.ToString(), Username = player.Username  }));
                                    
                                }
                            }
                        }
                    }
                    else
                    {
                        p.Send(Serialize(new LoginResponse { Success = false, Reason = String.Format("The Username '{0}' is already taken.  Please try another Username.", m.Username) }));
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }
        private void HandleRequest(Player p, LogoutRequest m)
        {
            // free-up username
            try
            {
                p.Username = "";
                p.Send(Serialize(new LogoutResponse { }));
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }
        private void HandleRequest(Player p, PingRequest m)
        {
            p.Send(Serialize(new PingResponse { }));
        }
        private void HandleRequest(Player p, ChatRequest m)
        {
            lock (clients)
            {
                foreach (var player in clients.Where(x => IsInLobby(x)))
                {
                    try
                    {
                        Send(player, new ChatResponse { Username = p.Username, Text = m.Text });
                    }
                    catch { }
                }
            }
        }

        private void HandleRequest(Player p, CreateGameRequest m)
        {
            if (games.Count > 100)
            {
                p.Send(Serialize(new CreateGameResponse { Success = false, Reason = "The server has reached the maximum number of simultaneous games allowed.  Please try to create a game later." }));
            }
            else
            {
                var game = new ConnectFourGame(p);
                games.Add(game);
                game.AddPlayer(p);
                p.Send(Serialize(new CreateGameResponse { Success = true, GameID = game.ID, MaxPlayers = game.MaxPlayers }));
                p.Send(Serialize(new AddPlayerMessage { GameID = game.ID, Username = p.Username, PlayerNumber = 1, CanStartGame = false }));
            }
        }
        private void HandleRequest(Player p, StartGameRequest m)
        {
            bool gameFound = false;
            foreach (var game in games)
            {
                if (game.ID == m.GameID)
                {
                    gameFound = true;
                    game.Start();
                    var response = new StartGameResponse { Success = true };
                    foreach (var player in game.Players)
                    {
                        player.Send(Serialize(response));
                    }
                    break;
                }
            }
            if (!gameFound)
                p.Send(Serialize(new StartGameResponse { Success = false, Reason = "Failed to start the game.  The game was not found on the server." }));
        }
        private void HandleRequest(Player p, InvitePlayerRequest m)
        {
            bool gameFound = false;
            bool playerFound = false;

            // find game
            foreach (var game in games)
            {
                if (game.ID == m.GameID)
                {
                    // game found.  find player
                    foreach (var player in clients)
                    {
                        if (player.Username == m.UsernameInvited)
                        {
                            // player found.  send invitation
                            player.Send(Serialize(new PlayerInvitationMessage { GameID = game.ID, InvitedByUsername = p.Username }));
                            playerFound = true;
                            break;
                        }
                    }

                    gameFound = true;
                    break;
                }
            }

            if (!gameFound)
                p.Send(Serialize(new InvitePlayerResponse { Success = false, Reason = String.Format("Failed to invite {0} to the game.  The game was not found on the server.", m.UsernameInvited) }));
            else if (!playerFound)
                p.Send(Serialize(new InvitePlayerResponse { Success = false, Reason = String.Format("Failed to invite {0} to the game.  The user is not currently logged in.", m.UsernameInvited) }));
            else
                p.Send(Serialize(new InvitePlayerResponse { Success = true }));
        }
        private void HandleRequest(Player p, JoinGameRequest m)
        {
            bool gameFound = false;
            Game game = null;
            int newPlayerNumber = 0;

            foreach (var g in games)
            {
                if (g.ID == m.GameID)
                {
                    game = g;
                    lock (game)
                    {
                        game.AddPlayer(p);
                        newPlayerNumber = game.Players.Count;
                        gameFound = true;
                        break;
                    }
                }
            }

            if (!gameFound)
            {
                p.Send(Serialize(new JoinGameResponse { Success = false, Reason = String.Format("Failed to join the game.  The game was not found on the server.") }));

            }
            else
            {
                p.Send(Serialize(new JoinGameResponse { Success = true, MaxPlayers = game.MaxPlayers }));
                // broadcast player joined
                foreach (var player in game.Players)
                {
                    player.Send(Serialize(new AddPlayerMessage { GameID = game.ID, Username = p.Username, CanStartGame = game.CanStart, PlayerNumber = newPlayerNumber }));

                    // Tell the joining player what other players are in the game.
                    if (player != p)
                    {
                        p.Send(Serialize(new AddPlayerMessage { GameID = game.ID, PlayerNumber = game.Players.IndexOf(player) + 1, Username = player.Username }));
                    }
                }
            }
        }
        private void HandleRequest(Player p, LeaveGameRequest m)
        {
            Game g = null;
            foreach (var game in games)
            {
                if (game.ID == m.GameID)
                {
                    g = game;
                    int playerNumber = game.Players.IndexOf(p) + 1;
                    game.RemovePlayer(p);
                    foreach (var player in game.Players)
                    {
                        player.Send(Serialize(new LeaveGameResponse { GameID = m.GameID, PlayerNumber = playerNumber, UsernameLeftGame = p.Username }));
                    }
                    break;
                }
            }
            if (g != null && g.Players.Count == 0)
            {
                // delete game if no players left
                games.Remove(g);
                g = null;
            }
        }
        private void HandleRequest(Player p, ResetGameRequest m)
        {
            Game game = games.FirstOrDefault(x => x.ID == m.GameID);
            if (game != null)
            {
                ConnectFourGame g = game as ConnectFourGame;
                g.Start();
                foreach (var player in g.Players)
                {
                    player.Send(Serialize(new ResetGameResponse { Success = true, GameID = m.GameID, PlayerNumber = g.Players.IndexOf(player) + 1, FirstTurnPlayerNumber = g.NextTurnPlayerNumber() }));
                }
            }
            else
            {
                p.Send(Serialize(new ResetGameResponse { GameID = m.GameID, Success = false, Reason = "The game you attempted to reset was not found on the server." }));
            }
        }

        private void HandleRequest(Player p, ConnectFourMoveRequest m)
        {
            bool gameFound = false;
            ConnectFourGame game = null;

            foreach (var g in games)
            {
                if (g.ID == m.GameID)
                {
                    game = g as ConnectFourGame;
                    gameFound = true;
                    break;
                }
            }

            if (!gameFound)
            {
                p.Send(Serialize(new ConnectFourMoveResponse { Success = false, Reason = String.Format("Move failed.  The game was not found on the server.") }));
            }
            else
            {
                game.TryMove(m.PlayerNumber, m.Column);
            }
        }

        private void Send(Player p, Message m)
        {
            p.Send(Serialize(m));
        }
        private bool IsInLobby(Player p)
        {
            return p.State == PlayerState.IN_LOBBY || p.State == PlayerState.IN_LOBBY_CLOAKED;
        }
        private string RemoveInvalidUsernameCharacters(string s)
        {
            if (s == null)
                return "";
            else if (s == "")
                return s;
            else
                return s.Replace("'", "").Replace("\"", "").Replace(" ", "");
        }
        public static string Serialize(Message m)
        {
            return JsonConvert.SerializeObject(m, new JsonSerializerSettings { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() });
        }
        public static void Log(string s)
        {
            lock (logLock)
            {
#if DEBUG
                Console.WriteLine(s);
#endif

                logger.WriteLine(s);

            }
        }
    }
}
