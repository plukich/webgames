using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PServer
{
    public class CreateGameRequest : Message
    {
        private const int category = 2;
        private const int id = 0;
        public override int Category { get { return category; } set { } }
        public override int ID { get { return id; } set { } }

        public GameType Type { get; set; }
    }
    public class CreateGameResponse : Message
    {
        private const int category = 2;
        private const int id = 0;
        public override int Category { get { return category; } set { } }
        public override int ID { get { return id; } set { } }

        public bool Success { get; set; }
        public string Reason { get; set; }
        public string GameID { get; set; }
        public int MaxPlayers { get; set; }
    }
}
