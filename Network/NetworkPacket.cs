/*
	This project is licensed under the GPL 2.0 license. Please respect that.

	Initial author: (https://github.com/)Convery
	Started: 2014-10-29
	Notes:
        Straight port of the C++ part, should work.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamServer
{
    // The packets we handle.
    public enum PublicPacketTypes : ulong
    {
        P2PMessage = 134,
    }
    public enum PrivatePacketTypes : ulong
    {
        Authentication = 99,
    }

    class NetworkPacket
    {
        public UInt64 _TransactionID;
        public UInt64 _XUID;
        public UInt64 _Username;
        public UInt32 _Type;
        public UInt32 _Seed;
        public UInt32 _IP;
        public Byte[] _Data;

        public void Serialize(ref redBuffer Buffer, SteamClient Client)
        {
            Byte[] IV   = new Byte[24];
            Byte[] Key  = new Byte[24];
            Byte[] EncryptedData;

            // The boring stuff.
            Buffer.WriteUInt64(_TransactionID);
            Buffer.WriteUInt64(_XUID);
            Buffer.WriteUInt64(_Username);
            Buffer.WriteUInt32(_Type);
            Buffer.WriteUInt32(_Seed);
            Buffer.WriteUInt32(_IP);

            // Generate the IV and key.
            IV = SteamCrypto.CalculateIV(_Seed);
            Key = SteamCrypto.CalculateIV(Client.SessionID);

            // Resize the buffers to keep 3DES happy.
            Array.Resize<Byte>(ref IV, 8);
            Array.Resize<Byte>(ref _Data, _Data.Length + (8 - _Data.Length % 8));

            // Create a new buffer and encrypt the data.
            EncryptedData = new Byte[_Data.Length];
            EncryptedData = SteamCrypto.Encrypt(_Data, Key, IV);

            // Write the encrypted data to the packet.
            Buffer.WriteBlob(EncryptedData);
        }
        public void Deserialize(redBuffer Buffer, SteamClient Client)
        {
            Byte[] IV = new Byte[24];
            Byte[] Key = new Byte[24];
            Byte[] EncryptedData = new Byte[1];

            // Generate the IV and key.
            IV = SteamCrypto.CalculateIV(_Seed);
            Key = SteamCrypto.CalculateIV(Client.SessionID);

            // Read the packet.
            Buffer.ReadUInt64(ref _TransactionID);
            Buffer.ReadUInt64(ref _XUID);
            Buffer.ReadUInt64(ref _Username);
            Buffer.ReadUInt32(ref _Type);
            Buffer.ReadUInt32(ref _Seed);
            Buffer.ReadUInt32(ref _IP);
            Buffer.ReadBlob(ref EncryptedData);

            // Create a new buffer and encrypt the data.
            _Data = new Byte[_Data.Length];
            _Data = SteamCrypto.Decrypt(EncryptedData, Key, IV);
        }
        public void CreatePacket(UInt64 TransactionID, UInt32 Type, redBuffer Data, SteamClient Client)
        {
            _TransactionID = TransactionID;
            _XUID = Client.XUID;
            _Username = SteamCrypto.fnv1_hash(Client.Username);
            _Type = Type;
            _Seed = (UInt32)(new Random((Int32)DateTime.Now.Ticks).Next());
            _IP = (UInt32)Client.GetIP().Address; // We will only use ipv4 so we ignore that it's deprecated.

            _Data = new Byte[Data.Length()];
            Array.Copy(Data.GetBuffer(), _Data, Data.Length());
        }

        public static Byte[] QuickMessage(UInt64 TransactionID, UInt32 Type, String Data, SteamClient Client)
        {
            NetworkPacket NewPacket = new NetworkPacket();
            redBuffer NewBuffer = new redBuffer();

            NewBuffer.WriteBlob(Encoding.ASCII.GetBytes(Data));
            NewPacket.CreatePacket(TransactionID, Type, NewBuffer, Client);

            NewBuffer.Rewind();
            NewPacket.Serialize(ref NewBuffer, Client);

            return NewBuffer.GetBuffer();
        }
    }
}
