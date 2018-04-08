using System;
using System.Threading;

namespace Server
{
    class Program
    {
        public const double SpamDelay = 0.5;
        public static readonly DateTime StartTime = DateTime.Now;
        private static ChatServer _chatServer1;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            Console.Write("Starting server... ");
            int port = 0;
            if (args.Length > 0)
            {
                string thePort = args[0];
                int.TryParse(thePort, out port);
            }

            if (port <= 0)
            {
                port = 4096;
            }

            Console.WriteLine($" on port {port}");
            _chatServer1 = new ChatServer(port);
            _chatServer1.Init();

            try
            {
                (new Thread(_chatServer1.Start)).Start();
            }
            catch (System.Exception ex)
            {
                Console.Write($"Server start error : {ex}");
            }
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            _chatServer1.ShutDown();
        }

    }
}
