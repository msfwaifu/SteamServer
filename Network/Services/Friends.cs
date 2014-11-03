/*
	All files containing this header is released under the GPL 3.0 license.

	Initial author: (https://github.com/)Convery
	Started: 2014-11-03
	Notes:
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamServer.Services
{
    class Friends : Base
    {
        public override void HandlePacket(ref NetworkPacket Packet, ref SteamClient Client)
        {
            redBuffer Buffer = new redBuffer();

            // We send the length first.
            Client.UpdateFriendslist();
            Buffer.WriteUInt32((UInt32)Client.FriendsList.Count);

            foreach(SteamFriend Friend in Client.FriendsList)
            {
                Buffer.WriteUInt32((UInt32)Friend.Status);
                Buffer.WriteBlob(Friend.Username);
                Buffer.WriteUInt64(Friend.XUID);
            }

            Packet.CreatePacket(Packet._TransactionID, 200, Buffer, Client);

            // Clear the buffer.
            Buffer = new redBuffer();

            // Serialize the packet.
            Packet.Serialize(ref Buffer, Client);

            // Send the response.
            SteamServer.Send(Client.ClientID, Buffer.GetBuffer());
        }
    }
}