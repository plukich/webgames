using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PServer
{
    public class LeaveGameRequest : Message
    {
        private const int category = 2;
        private const int id = 6;
        public override int Category { get { return category; } set { } }
        public override int ID { get { return id; } set { } }

        public string GameID { get; set; }
        public int PlayerNumber { get; set; }
    }
    public class LeaveGameResponse : Message
    {
        private const int category = 2;
        private const int id = 6;
        public override int Category { get { return category; } set { } }
        public override int ID { get { return id; } set { } }

        public string GameID { get; set; }
        public int PlayerNumber { get; set; }
        public string UsernameLeftGame { get; set; }
    }
}
