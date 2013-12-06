using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PServer
{
    public class LoginRequest : Message
    {
        private const int category = 1;
        private const int id = 0;
        public override int Category { get { return category; } set { } }
        public override int ID { get { return id; } set { } }

        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class LoginResponse : Message
    {
        private const int category = 1;
        private const int id = 0;
        public override int Category { get { return category; } set { } }
        public override int ID { get { return id; } set { } }

        public bool Success { get; set; }
        public string Reason { get; set; }
    }
}
