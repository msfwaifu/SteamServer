/*
	This project is licensed under the GPL 2.0 license. Please respect that.

	Initial author: (https://github.com/)Convery
	Started: 2014-10-28
	Notes:
*/

using System;
using System.Collections.Generic;
using System.Threading;

namespace SteamServer
{
    class Program
    {
        public static void PrintInfo()
        {
            DateTime StartTime = DateTime.Now;

            // Give the server some time to start.
            System.Threading.Thread.Sleep(250);

            while (true)
            {
                Console.WriteLine("=========== Steam server ===========\n");
                Console.WriteLine("Port: {0}", SteamServer.Port);
                Console.WriteLine("Clients: {0}", SteamServer.Clients.Count);
                Console.WriteLine("Uptime: {1}:{0:mm}:{0:ss}", (DateTime.Now - StartTime), Math.Floor((DateTime.Now - StartTime).TotalHours).ToString());

                Console.WriteLine("\n\n=========== Log ===========\n");
                Log.PrintBuffer();

                System.Threading.Thread.Sleep(5000);
                Console.Clear();
            }
        }

        static void Main(string[] args)
        {
            Log.Initialize(System.IO.Path.Combine(Environment.CurrentDirectory, "SteamServer.log"), LogLevel.All, false);
            new Thread(new ThreadStart(PrintInfo)).Start();

            SteamServer.InitServer();
            SteamServer.StartListening();
        }
    }
}
