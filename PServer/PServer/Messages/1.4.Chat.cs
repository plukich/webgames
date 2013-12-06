using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PServer
{
    public class ChatRequest: Message
    {
        private const int category = 1;
        private const int id = 4;
        public override int Category { get { return category; } set { } }
        public override int ID { get { return id; } set { } }

        public string Text { get; set; }
    }
    public class ChatResponse: Message
    {
        private const int category = 1;
        private const int id = 4;
        public override int Category { get { return category; } set { } }
        public override int ID { get { return id; } set { } }

        public string Username { get; set; }
        public string Text { get; set; }
    }
}
