using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PServer
{
    public class Message
    {
        // consider 0.0 invalid or 'reserved'.

        public virtual int Category { get; set; }
        public virtual int ID { get; set; }

        public static Message Deserialize(string s, int category, int id)
        {
            if (category == 0)
            {
            }
            else if (category == 1)
            {
                if (id == 0)
                {
                    return JsonConvert.DeserializeObject<LoginRequest>(s);
                }
                else if (id == 1)
                {
                    return JsonConvert.DeserializeObject<LogoutRequest>(s);
                }
                else if (id == 2)
                {
                    return JsonConvert.DeserializeObject<PingRequest>(s);
                }
                else if (id == 4)
                {
                    return JsonConvert.DeserializeObject<ChatRequest>(s);
                }
            }
            else if (category == 2)
            {
                if (id == 0)
                {
                    return JsonConvert.DeserializeObject<CreateGameRequest>(s);
                }
                else if (id == 1)
                {
                    return JsonConvert.DeserializeObject<StartGameRequest>(s);
                }
                else if (id == 2)
                {
                    return JsonConvert.DeserializeObject<AddPlayerMessage>(s);
                }
                else if (id == 3)
                {
                    return JsonConvert.DeserializeObject<InvitePlayerRequest>(s);
                }
                else if (id == 4)
                {
                    return JsonConvert.DeserializeObject<PlayerInvitationMessage>(s);
                }
                else if (id == 5)
                {
                    return JsonConvert.DeserializeObject<JoinGameRequest>(s);
                }
                else if (id == 6)
                {
                    return JsonConvert.DeserializeObject<LeaveGameRequest>(s);
                }
                else if (id == 7)
                {
                    return JsonConvert.DeserializeObject<ResetGameRequest>(s);
                }
            }
            else if (category == 3)
            {
                if (id == 100)
                {
                    return JsonConvert.DeserializeObject<ConnectFourMoveRequest>(s);
                }
            }

            return null;

        }
    }
}
