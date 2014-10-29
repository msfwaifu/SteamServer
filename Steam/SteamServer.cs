/*
	This project is licensed under the GPL 2.0 license. Please respect that.

	Initial author: (https://github.com/)Convery
	Started: 2014-10-28
	Notes:
        Async TCP server.
        Settings can be imported from an XML file.
        TODO: Link the packet handler.
*/

using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace SteamServer
{
    class SteamServer
    {
        // Node info.
        public static Dictionary<UInt64, IPAddress> KnownNodes;
        public static UInt64 NodeID;        // [Public] This nodes identifier on the network.
        public static UInt64 SessionID;     // [Private] Identity verification.
        public static Boolean isConnected;  // [Public] Is connected to other nodes.
        public static Boolean ShouldConnect;// [Private] Should connect to the master and other nodes.
        public static Boolean Anonymous;    // [Public] Node does not authenticate the client.

        // Server info.
        public  static UInt16 Port; // Listen port.
        private static UInt16 Limit = 250; // Backlog.
        private static ManualResetEvent MRE = new ManualResetEvent(false);
        public  static Dictionary<UInt32, SteamClient> Clients = new Dictionary<UInt32, SteamClient>(); // Client map.
        //public  static redPacketHandler PacketHandler = new redPacketHandler(); // Handles all incomming packets.

        // General methods.
        public static void InitServer()
        {
            if (!File.Exists("NodeSettings.xml"))
            {
                NodeID = 0;
                SessionID = 0;
                isConnected = false;
                ShouldConnect = false;
                Anonymous = false;
                Port = 42042;
            }
            else
            {
                XmlTextReader xReader = new XmlTextReader("NodeSettings.xml");
                String LastElement = null;

                while (xReader.Read())
                {
                    switch (xReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            LastElement = xReader.Name;
                            break;
                        case XmlNodeType.Text:
                            if (LastElement != null) // This should never happen.
                            {
                                switch (LastElement)
                                {
                                    // Node info.
                                    case "NodeID":
                                        NodeID = UInt64.Parse(xReader.Value);
                                        break;
                                    case "ShouldConnect":
                                        ShouldConnect = Boolean.Parse(xReader.Value);
                                        break;
                                    case "Anonymous":
                                        Anonymous = Boolean.Parse(xReader.Value);
                                        break;

                                    // Server info.
                                    case "Port":
                                        Port = UInt16.Parse(xReader.Value);
                                        break;
                                }
                            }
                            break;
                        case XmlNodeType.EndElement:
                            LastElement = null;
                            break;
                    }
                }
            }
        }
        public static void Send(UInt32 ID, Byte[] Data)
        {
            if (!isClientConnected(ID))
            {
                RemoveClient(ID);
                return;
            }

            try
            {
                lock (Clients)
                {
                    Clients[ID].ClientSocket.BeginSend(Data, 0, Data.Length, SocketFlags.None, new AsyncCallback(SendCallback), Clients[ID]);
                }
            }
            catch (Exception e)
            {
                Log.Warning(e.Message);

                if (e is SocketException)
                {
                    RemoveClient(ID);
                }
            }
        }
        public static void StartListening()
        {
            IPAddress IP = IPAddress.Any;
            IPEndPoint Socket = new IPEndPoint(IP, Port);

            try
            {
                using (Socket Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    Listener.Bind(Socket);
                    Listener.Listen(Limit);
                    while (true)
                    {
                        MRE.Reset();
                        Listener.BeginAccept(new AsyncCallback(ConnectCallback), Listener);
                        MRE.WaitOne();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warning(e.InnerException.ToString());
            }
        }

        // Client methods.
        public static UInt32 FindClient(UInt64 XUID)
        {
            try
            {
                var Query = (from Client in Clients
                             where (Client.Value.XUID == XUID)
                             select Client.Key).ToList();

                if (Query.Count >= 1)
                    return Query[0];
            }
            catch (Exception e)
            {
            }
            return 0xFFFFFFFF;
        }
        public static UInt32 FindClient(Byte[] Username)
        {
            try
            {
                var Query = (from Client in Clients
                             where (SteamCrypto.fnv1_hash(Client.Value.Username) == SteamCrypto.fnv1_hash(Username))
                             select Client.Key).ToList();

                if (Query.Count >= 1)
                    return Query[0];
            }
            catch (Exception e)
            {
            }
            return 0xFFFFFFFF;
        }
        public static bool isClientConnected(UInt32 ID)
        {
            if (ID == 0xFFFFFFFF)
                return false;

            try
            {
                lock(Clients)
                    return !(Clients[ID].ClientSocket.Poll(1, SelectMode.SelectRead) && Clients[ID].ClientSocket.Available == 0);
            }
            catch (Exception e)
            {
                Log.Warning(e.Message);
                return false;
            }
        }
        public static void RemoveClient(UInt32 ID)
        {
            if (ID == 0xFFFFFFFF)
                return;

            lock (Clients)
            {
                try
                {
                    // Free the memory.
                    Array.Clear(Clients[ID].Buffer, 0, Clients[ID].Buffer.Length);
                    Clients[ID].Buffer = null;

                    // Send a proper disconnect.
                    Clients[ID].ClientSocket.BeginDisconnect(false, new AsyncCallback(DisconnectCallback), Clients[ID]);
                }
                catch (Exception e)
                {
                    Log.Warning(e.Message);
                }
                finally
                {
                    // Remove the client from the map.
                    Clients.Remove(ID);
                }
            }
        }

        // Async callbacks.
        private static void ConnectCallback(IAsyncResult result)
        {
            SteamClient Client = new SteamClient();

            try
            {
                Client.ClientSocket = (Socket)result.AsyncState;
                Client.ClientSocket = Client.ClientSocket.EndAccept(result);

                Log.Info(String.Format("Client connected from {0}", Client.ClientSocket.RemoteEndPoint.ToString()));

                lock (Clients)
                {
                    Client.ClientID = !Clients.Any() ? 0 : Clients.Keys.Max() + 1;
                    Clients.Add(Client.ClientID, Client);
                }

                Client.ClientSocket.BeginReceive(Client.Buffer, 0, SteamClient.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), Client);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
        }
        private static void DisconnectCallback(IAsyncResult result)
        {
            SteamClient Client = (SteamClient)result.AsyncState;

            try
            {
                Client.ClientSocket.Close();
                Client.ClientSocket.Dispose();

            }
            catch (Exception e)
            {
                Client.ClientSocket.EndDisconnect(result);
                Log.Warning(e.ToString());
            }
        }
        private static void SendCallback(IAsyncResult result)
        {
            SteamClient Client = (SteamClient)result.AsyncState;
            try
            {
                Client.ClientSocket.EndSend(result);
            }
            catch (Exception e)
            {
                if (e is SocketException)
                {
                    RemoveClient(Client.ClientID);
                    return;
                }

                Log.Warning(e.ToString());
            }
        }
        private static void ReceiveCallback(IAsyncResult result)
        {
            SteamClient Client = (SteamClient)result.AsyncState;

            try
            {
                if (Client.ClientSocket.EndReceive(result) == SteamClient.BufferSize)
                {
                    Client.ClientSocket.BeginReceive(Client.Buffer, 0, SteamClient.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), Client);
                }
                else
                {
                    // We handle the packet and send a response here.
                    //Send(Client.ClientID, PacketHandler.HandlePacket(Client));

                    // Continue.
                    Client.ClientSocket.BeginReceive(Client.Buffer, 0, SteamClient.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), Client);
                }
            }
            catch (Exception e)
            {
                Log.Warning(e.Message);
                RemoveClient(Client.ClientID);
            }
        }
    }
}
