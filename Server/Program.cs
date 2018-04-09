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

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;


        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            ConsoleColor colorBefore = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.ExceptionObject.ToString());
            Console.ForegroundColor = colorBefore;
            _chatServer1?.ShutDown();
            Environment.Exit(1);
        }



        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool Handler(CtrlType sig)
        {
            switch (sig)
            {
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                    _chatServer1?.ShutDown();
                    return true;
                default:
                    return false;
            }
        }



        static void Main(string[] args)
        {
            _handler += Handler;
            SetConsoleCtrlHandler(_handler, true);
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

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

            WaitForAnotherInstance();
            ManageInstances();
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
        private static void WaitForAnotherInstance()
        {
            var appSingleton = new Mutex(false, "talkysingleinstancemtx");

            try
            {
                appSingleton.WaitOne();
            }
            catch (AbandonedMutexException ex)
            {
                //another process exited 
            }
            catch (System.Exception e)
            {
                ManageInstances();
                Environment.Exit(-1);
            }
            Console.WriteLine(" acquired");

        }

        private static void ManageInstances()
        {
            Console.WriteLine("Starting another instance");
            Process.Start(Application.ExecutablePath);
        }
    }
}
