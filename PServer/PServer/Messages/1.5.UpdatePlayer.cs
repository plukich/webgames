using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PServer
{
    public class UpdatePlayerMessage : Message
    {
        private const int category = 1;
        private const int id = 5;
        public override int Category { get { return category; } set { } }
        public override int ID { get { return id; } set { } }

        public int PlayerID { get; set; }
        public string Username { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
    }
}
