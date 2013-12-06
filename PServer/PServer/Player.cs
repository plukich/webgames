using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;

namespace PServer
{
    public enum PlayerState : byte
    {
        CONNECTED = 0,
        IN_LOBBY,
        IN_LOBBY_CLOAKED,
        PLAYING_GAME,
        LOGGED_OFF,
    }
    public class Player
    {
        private string name;

        public IWebSocketConnection WebSocket { get; set; }
        public string Username
        {
            get
            {
                if (name == null)
                    name = "";
                return name;
            }
            set { name = value; }
        }
        public int ID { get; set; }
        public PlayerState State { get; set; }

        public void Send(string s)
        {
            Server.Log(String.Format("out: [{0} <{1}>]: {2}", WebSocket.ConnectionInfo.ClientIpAddress, Username ?? "", s));

            WebSocket.Send(s);
        }
    }
}
