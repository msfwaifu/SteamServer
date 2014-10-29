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

namespace SteamServer
{
    public enum PersonaState
    {
        Offline = 0,
        Online = 1,
        Busy = 2,
        Away = 3,
        Snooze = 4,
    }

    class SteamFriend
    {
        public PersonaState Status;
        public Byte[] Username;
        public UInt64 XUID;

        public static SteamFriend CreateFromClient(UInt32 ID)
        {
            SteamFriend NewFriend = new SteamFriend();

            lock (SteamServer.Clients)
            {
                NewFriend.Status = SteamServer.Clients[ID].SocialStatus;
                NewFriend.XUID = SteamServer.Clients[ID].XUID;

                NewFriend.Username = new Byte[SteamServer.Clients[ID].Username.Length];
                Array.Copy(SteamServer.Clients[ID].Username, NewFriend.Username, SteamServer.Clients[ID].Username.Length);
            }

            return NewFriend;
        }
    }
}
