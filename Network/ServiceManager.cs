/*
	This project is licensed under the GPL 2.0 license. Please respect that.

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
        Dictionary<ulong, Services.Base> ServiceMap;

        // Initialize the services.
        public ServiceManager()
        {
            ServiceMap = new Dictionary<ulong, Services.Base>();


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
            ServiceMap[ServicePacket._Type].HandlePacket(ref ServicePacket, ref Client);
        }

    }
}
