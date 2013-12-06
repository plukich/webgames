using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PServer
{
    public class ConnectFourMoveRequest : Message
    {
        private const int category = 3;
        private const int id = 100;
        public override int Category { get { return category; } set { } }
        public override int ID { get { return id; } set { } }

        public string GameID { get; set; }
        public int PlayerNumber { get; set; }
        public int Column { get; set; }
    }
    public class ConnectFourMoveResponse : Message
    {
        private const int category = 3;
        private const int id = 100;
        public override int Category { get { return category; } set { } }
        public override int ID { get { return id; } set { } }
        
        public bool Success { get; set; }
        public string Reason { get; set; }
        public int PlayerNumber { get; set; }
        public int Column { get; set; }
    }
}
