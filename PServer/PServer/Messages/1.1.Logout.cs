using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PServer
{
    public class LogoutRequest : Message
    {
        private const int category = 1;
        private const int id = 1;
        public override int Category { get { return category; } set { } }
        public override int ID { get { return id; } set { } }
    }
    public class LogoutResponse : Message
    {
        private const int category = 1;
        private const int id = 1;
        public override int Category { get { return category; } set { } }
        public override int ID { get { return id; } set { } }
    }
}
