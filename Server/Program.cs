using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Server
{
    class Program
    {
        public const double SpamDelay = 0.5;
        public static readonly DateTime StartTime = DateTime.Now;
        private static ChatServer _chatServer1;
        private static bool _isFirst;
        private static readonly EventWaitHandle SyncHandle = new EventWaitHandle(false, EventResetMode.AutoReset, "talkyservermtx", out _isFirst);
        private static ConsoleEventDelegate _exitHandler;
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);


        static void Main(string[] args)
        {
            _exitHandler = CurrentDomain_ProcessExit;
            SetConsoleCtrlHandler(_exitHandler, true);

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

            WaitForAnotherInstance();

            try
            {
                (new Thread(_chatServer1.Start)).Start();
                ManageInstances();
            }
            catch (System.Exception ex)
            {
                Console.Write($"Server start error : {ex}");
            }
        }

        private static bool CurrentDomain_ProcessExit(int eventType)
        {
            _chatServer1.ShutDown();
            SyncHandle.Set();
            return false;
        }

        private static int CountProcess(string name)
        {
            return Process.GetProcessesByName(name).Length;
        }

        private static void WaitForAnotherInstance()
        {
            Console.Write("Waiting for syncronization handle...");
            if (!_isFirst)
                SyncHandle.WaitOne();

            Console.WriteLine(" acquired");
        }

        private static void ManageInstances()
        {
            //var num = CountProcess(Process.GetCurrentProcess().ProcessName);
            //Console.WriteLine($"Number of processes running {num}");

            //if (num >= 2) return;

            Console.WriteLine("Starting another instance");
            Process.Start(Application.ExecutablePath);
        }
    }
}
