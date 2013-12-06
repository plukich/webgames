using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PServer
{
    public class ConnectFouGameOverMessage : Message
    {
        private const int category = 3;
        private const int id = 101;
        public override int Category { get { return category; } set { } }
        public override int ID { get { return id; } set { } }

        public string GameID { get; set; }
        public int WinnerPlayerNumber { get; set; }
    }
}
