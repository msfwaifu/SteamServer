/*
	All files containing this header is released under the GPL 3.0 license.

	Initial author: (https://github.com/)Convery
	Started: 2014-10-29
	Notes:
        The manager simply routes the data to the handler.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamServer
{
    class ServiceManager
    {
        // Map of services.
        Dictionary<UInt32, Services.Base> ServiceMap;

        // Initialize the services.
        public ServiceManager()
        {
            ServiceMap = new Dictionary<UInt32, Services.Base>();

            ServiceMap.Add((UInt32)PublicPacketTypes.P2PMessage, new Services.Message());
            ServiceMap.Add((UInt32)PrivatePacketTypes.Authentication, new Services.Auth());
        }

        // Handles all packets and calls the service designated to the packet type.
        public void HandlePacket(SteamClient Client)
        {
            redBuffer InternalBuffer;
            Byte[] ServiceData = new Byte[1];
            NetworkPacket ServicePacket = new NetworkPacket();

            // Serialize and create the packet.
            InternalBuffer = new redBuffer(Client.Buffer);
            InternalBuffer.ReadBlob(ref ServiceData);
            ServicePacket.Deserialize(new redBuffer(ServiceData), Client);

            // Log that we got a packet for debug.
            Log.Debug(String.Format("PacketType: {0}", ServicePacket._Type));

            // Send the packet to the right service.
            if(ServiceMap.ContainsKey(ServicePacket._Type))
                ServiceMap[ServicePacket._Type].HandlePacket(ref ServicePacket, ref Client);
        }
    }
}
