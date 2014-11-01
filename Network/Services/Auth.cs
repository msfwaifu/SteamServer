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
    class Auth : Base
    {
        public override void HandlePacket(ref NetworkPacket Packet, ref SteamClient Client)
        {
            Byte[] Username     = new Byte[1];
            Byte[] Reserved1    = new Byte[1];
            Byte[] Reserved2    = new Byte[1];
            Byte[] Reserved3    = new Byte[1];
            Byte[] Reserved4    = new Byte[1];
            Byte[] WindowsName  = new Byte[1];
            redBuffer Buffer;

            // Read the boring info.
            Buffer = new redBuffer(Packet._Data);
            Buffer.ReadBlob(ref Username);
            Buffer.ReadBlob(ref Reserved1);
            Buffer.ReadBlob(ref Reserved2);
            Buffer.ReadBlob(ref Reserved3);
            Buffer.ReadBlob(ref Reserved4);
            Buffer.ReadBlob(ref WindowsName);

            // Set the client info.
            if (SteamServer.Anonymous)
            {
                Client.Username = Username;
                Client.XUID = (UInt64)0x110000110000000;
                Client.XUID += Client.ClientID;
                Client.SessionID = Client.ClientID;
                Client.isAuthenticated = true;
            }
            else
            {
                // Reserved.
            }

            // Clear the buffer.
            Buffer = new redBuffer();

            // Return the info.
            Buffer.WriteBlob(Client.Username);
            Buffer.WriteUInt64(0); // Reserved.
            Buffer.WriteUInt32(Client.SessionID);

            if(Client.isAuthenticated == true)
                Packet.CreatePacket(Packet._TransactionID, 200, Buffer, Client);
            else
                Packet.CreatePacket(Packet._TransactionID, 404, Buffer, Client);

            // Clear the buffer.
            Buffer = new redBuffer();

            // Serialize the packet.
            Packet.Serialize(ref Buffer, Client);

            // Send the response.
            SteamServer.Send(Client.ClientID, Buffer.GetBuffer());
        }
    }
}