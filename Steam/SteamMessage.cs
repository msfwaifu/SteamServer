/*
	All files containing this header is released under the GPL 3.0 license.

	Initial author: (https://github.com/)Convery
	Started: 2014-10-28
	Notes:
        The server does not check the contents of the packet.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamServer
{
    // Standard message format.
    struct InstantMessage
    {
        public UInt64 FromXUID;
        public UInt64 ToXUID;
        public UInt32 TimeStamp;

        public Byte[] Message;
    }

    // Client-client or server-client communication.
    class SteamMessage
    {
        public InstantMessage IM;
        public DateTime Timestamp;

        public bool Serialize(ref redBuffer Buffer)
        {
            try
            {
                Buffer.ReadUInt64(ref IM.FromXUID);
                Buffer.ReadUInt64(ref IM.ToXUID);
                Buffer.ReadUInt32(ref IM.TimeStamp);
                Buffer.ReadBlob(ref IM.Message);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return false;
            }

            return true;
        }
        public bool Deserialize(redBuffer Buffer)
        {
            try
            {
                Buffer.WriteUInt64(IM.FromXUID);
                Buffer.WriteUInt64(IM.ToXUID);
                Buffer.WriteUInt32(IM.TimeStamp);
                Buffer.WriteBlob(IM.Message);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return false;
            }

            return true;
        }
    }
}
