using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;
using System.Threading;

namespace PServer
{
    public class ConnectFourServer
    {
        private List<IWebSocketConnection> clients = new List<IWebSocketConnection>();
        bool running = false;

        public void Start()
        {
            var server = new WebSocketServer("ws://localhost:7750");
            bool running = true;
            server.Start((socket) => SetupClient(socket));

            // keep the main thread alive..
            while (running)
            {
                Thread.Sleep(1000);
            }
        }
        public void Stop()
        {
            running = false;
        }

        private void SetupClient(IWebSocketConnection c)
        {
            c.OnOpen = () => {
                Console.WriteLine("[" + c.ConnectionInfo.ClientIpAddress + "] connected");
                clients.Add(c); 
            };
            c.OnError = (ex) => { try { clients.Remove(c); } catch (Exception) { };};
            c.OnClose = () => { try { clients.Remove(c); } catch (Exception) { };};
            c.OnMessage = (s) => ReceivedClientMessage(c, s);
            c.Send("welcome");
        }

        private void ReceivedClientMessage(IWebSocketConnection c, string s)
        {
            Console.WriteLine("[" + c.ConnectionInfo.ClientIpAddress + "]: " + s);
            
            try
            {
                foreach (var item in clients)
                    item.Send("[" + c.ConnectionInfo.ClientIpAddress + "]: " + s);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
