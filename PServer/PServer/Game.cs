using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PServer
{
    /// <summary>
    /// Basic framework for any Game
    /// </summary>
    public class Game
    {
        protected List<Player> players = new List<Player>();
        protected Player owner;
        protected GameType type;
        protected string id;
        protected int maxPlayers = 2;
        protected bool started = false;

        public virtual GameType Type { get { return type; } }
        public virtual Player Owner { get { return owner; } }
        public virtual string ID { get { return id; } }
        public virtual List<Player> Players { get { return players; } }
        public virtual bool CanStart { get { return false; } }
        public virtual int MaxPlayers { get { return maxPlayers; } set { maxPlayers = value; } }

        public Game()
        {
            id = NewGameID();
        }
        public Game(Player owner, GameType type)
        {
            this.id = NewGameID();
            this.owner = owner;
            this.type = type;
        }

        private string NewGameID()
        {
            return Guid.NewGuid().ToString();
        }
        public virtual void Start()
        {
            started = true;
        }
        public virtual void AddPlayer(Player p)
        {
            lock (players)
            {
                players.Add(p);
            }
        }
        public virtual void RemovePlayer(Player p)
        {
            lock (players)
            {
                players.Remove(p);
            }
        }

    }
}
