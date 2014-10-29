/*
	All files containing this header is released under the GPL 3.0 license.

	Initial author: (https://github.com/)Convery
	Started: 2014-10-29
	Notes:
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamServer.Services
{
    class Message : Base
    {
        public override void HandlePacket(ref NetworkPacket Packet, ref SteamClient Client)
        {
            SteamMessage NewMessage = new SteamMessage();
            NewMessage.Deserialize(new redBuffer(Packet._Data));

            // Debug log.
            Log.Debug(String.Format("Got a message from {0}, forwarding to {1}", NewMessage.IM.FromXUID, NewMessage.IM.ToXUID));

            if (SteamServer.isClientConnected(SteamServer.FindClient(NewMessage.IM.ToXUID)))
            {
                if (!SteamServer.Clients[SteamServer.FindClient(NewMessage.IM.ToXUID)].EnqueueMessage(NewMessage))
                {
                    SteamServer.Send(Client.ClientID, NetworkPacket.QuickMessage(Packet._TransactionID, 403,
                                    "Too many messages are being sent to that client right now.", Client));
                }
            }
            else
            {
                SteamServer.Send(Client.ClientID, NetworkPacket.QuickMessage(Packet._TransactionID, 404,
                                    "The client is offline.", Client));
            }
        }
    }
}
