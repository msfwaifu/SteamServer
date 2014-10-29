/*
	This project is licensed under the GPL 2.0 license. Please respect that.

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
    class Base
    {
        public virtual void HandlePacket(ref NetworkPacket Packet, ref SteamClient Client) { }
    }
}
