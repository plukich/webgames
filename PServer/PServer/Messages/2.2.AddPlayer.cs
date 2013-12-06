using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PServer
{
    public class AddPlayerMessage : Message
    {
        private const int category = 2;
        private const int id = 2;
        public override int Category { get { return category; } set { } }
        public override int ID { get { return id; } set { } }

        public string GameID { get; set; }
        public string Username { get; set; }
        public int PlayerNumber { get; set; }
        public bool CanStartGame { get; set; }
    }
}
