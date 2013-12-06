﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PServer
{
    public class StartGameRequest : Message
    {
        private const int category = 2;
        private const int id = 1;
        public override int Category { get { return category; } set { } }
        public override int ID { get { return id; } set { } }

        public string GameID { get; set; }
    }
    public class StartGameResponse : Message
    {
        private const int category = 2;
        private const int id = 1;
        public override int Category { get { return category; } set { } }
        public override int ID { get { return id; } set { } }

        public bool Success { get; set; }
        public string Reason { get; set; }
    }
}
