using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Server.Channel;
using Server.Client;
using Server.Command;
using Server.Connection;
using Server.Exception;

namespace Server
{
    class Program
    {
        public const double SpamDelay = 0.5;
        public static readonly DateTime StartTime = new DateTime(1970, 1, 1);
        private static bool _isFirst;
        private static readonly EventWaitHandle SyncHandle = new EventWaitHandle(false, EventResetMode.AutoReset, "talkyservermtx", out _isFirst);
        private static readonly AutoResetEvent ConsoleWaitEvent = new AutoResetEvent(false);

        private static ConsoleEventDelegate _exitHandler; 
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        private bool _panicMode;

        private readonly ChannelRepository _channelRepository = ChannelRepository.Instance;
        private readonly ClientRepository _clientRepository = ClientRepository.Instance;
        private readonly CommandManager _commandManager = CommandManager.Instance;

        public static int Port { get; private set; } = 4096;

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

            Port = port;
            new Program();
        }

        private static int CountProcess(string name)
        {
            return Process.GetProcessesByName(name).Length;
        }

        private void InitChannels()
        {

            if (_channelRepository.Exists("+lobby"))
            {
                // Lobby is already in DB. Previous server already has channels in DB so this must be recovery
                // Recreate channel list from DB, don't write new channels to DB
                Console.Write("+lobby channel is already in DB! Server restarted and now doing recovery");

                // TODO: Recreate channel repository list but do not write duplicates to DB
                // Right now this is just making 1 lobby channel upon recovery
                if (_channelRepository.GetLobby() == null)
                {
                    _channelRepository.Store(new LobbyChannel("+lobby"), false);
                }

                return;
            }

            IReadOnlyDictionary<string, string> channels = new Dictionary<string, string> { { "+lobby", "true,false" }, { "+admins", "false,true" }, { "+publicChat", "false,false" } };

            foreach (string key in channels.Keys)
            {
                string channelName = key;
                if (!channelName.StartsWith("+"))
                {
                    channelName = "+" + channelName;
                }

                channels.TryGetValue(key, out var settings);
                var splitSettings = settings.Split(new[] { ',' }, 2);

                if (splitSettings.Length != 2)
                {
                    continue;
                }

                bool lobby = splitSettings[0].Equals("true");
                bool locked = splitSettings[1].Equals("true");

                if (lobby && _channelRepository.GetLobby() != null)
                {
                    continue;
                }

                if (_channelRepository.Get(channelName) != null)
                {
                    continue;
                }

                _channelRepository.Store(lobby
                    ? new LobbyChannel(channelName)
                    : new SystemChannel(channelName, locked), true);
            }
        }

        private bool RegisterCommands()
        {
            try
            {
                _commandManager.RegisterCommand(new CommandHelp());
                _commandManager.RegisterCommand(new CommandName());
                _commandManager.RegisterCommand(new CommandJoin());
                _commandManager.RegisterCommand(new CommandClist());
                _commandManager.RegisterCommand(new CommandCC());
                _commandManager.RegisterCommand(new CommandAuth());
                _commandManager.RegisterCommand(new CommandRegister());
                _commandManager.RegisterCommand(new CommandRole());
                _commandManager.RegisterCommand(new CommandKick());
                _commandManager.RegisterCommand(new CommandMute());
                _commandManager.RegisterCommand(new CommandChangePassword());
                _commandManager.RegisterCommand(new CommandMsg());
            }
            catch (CommandExistsException cEE)
            {
                Console.WriteLine(cEE.StackTrace);
                return false;
            }
            return true;
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
            var num = CountProcess(Process.GetCurrentProcess().ProcessName);
            Console.WriteLine($"Number of processes running {num}");

            if (num >= 2) return;

            Console.WriteLine("Starting another instance");
            Process.Start(Application.ExecutablePath);
        }

        private Program()
        {

            if (!RegisterCommands())
                return;

            WaitForAnotherInstance();
            ManageInstances();

            InitChannels();

            //add loop to clean up/reinit?
            try
            {
                Thread channelManagerThread = new Thread(MonitorChannels);
                channelManagerThread.Start();

                Thread activityMonitorThread = new Thread(MonitorActivity);
                activityMonitorThread.Start();

                Thread listenerThread = new Thread(ListenForClients);
                listenerThread.Start();

                ShowConsole();
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Exception caught from starting threads");
            }
        }

        private static bool CurrentDomain_ProcessExit(int eventType)
        {
            ConsoleWaitEvent.Set();
            SyncHandle.Set();
            return false;
        }


        public void OHGODNO(string WHAT, System.Exception theRealProblem = null)
        {
            _panicMode = true;

            IReadOnlyCollection<ServerClient> clients = _clientRepository.All();
            foreach (ServerClient client in clients)
            {
                client.Disconnect("§1Server went into panic mode.");
            }

            Thread.Sleep(501);

            Console.Clear();
            Console.WriteLine("======================================");
            Console.WriteLine("=        SERVER IN PANIC MODE        =");
            Console.WriteLine("=         SEE PROBLEMS BELOW         =");
            Console.WriteLine("======================================");
            Console.WriteLine("");
            Console.WriteLine("DEAR GOD! NO!! A BUG! IT'S ALL BROKEN!");
            Console.WriteLine(WHAT);

            if (theRealProblem != null)
            {
                Console.WriteLine("");
                Console.WriteLine(theRealProblem.Message);
            }
            Console.WriteLine("");

            Console.WriteLine("Press a key or something to exit... I won't mind...");
            Console.ReadKey();
            Environment.Exit(0);
        }

        private void ShowConsole()
        {
            while (!_panicMode)
            {
                Console.Clear();
                Console.WriteLine("Talky | Created by SysVoid");
                Console.WriteLine("==========================");
                Console.WriteLine("Clients: " + _clientRepository.Count());
                Console.WriteLine("Channels: " + _channelRepository.Count());
                Console.WriteLine("Commands: " + _commandManager.Count());
                Console.WriteLine("==========================");
                ConsoleWaitEvent.WaitOne(5000);
            }
        }

        private void ListenForClients()
        {
            var listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();
            while (!_panicMode)
            {
                var tcpClient = listener.AcceptTcpClient();
                var serverClient = new ServerClient(tcpClient);

                if (_channelRepository.GetLobby() == null)
                {
                    serverClient.Disconnect("§2Server Error: No Lobby!");
                    continue;
                }

                Thread clientThread = new Thread(new ServerConnection(serverClient).HandleMessages);
                clientThread.Start();
            }
        }

        private void MonitorChannels()
        {
            var delay = new AutoResetEvent(false);

            while (!_panicMode)
            {
                IReadOnlyCollection<ClientChannel> clientChannels = _channelRepository.Get<ClientChannel>();

                if (clientChannels.Count > 0)
                {
                    foreach (ClientChannel clientChannel in clientChannels)
                    {
                        if (_clientRepository.Find(clientChannel).Count == 0)
                        {
                            _channelRepository.Remove(clientChannel);
                        }
                    }
                }

                delay.WaitOne(5000);
            }
        }

        private void MonitorActivity()
        {
            var delay = new AutoResetEvent(false);

            while (!_panicMode)
            {
                var clients = _clientRepository.All();
                if (clients.Count > 0)
                {
                    foreach (var client in clients)
                    {
                        int now = (int)(DateTime.UtcNow.Subtract(StartTime)).TotalSeconds;
                        if (client.LastActivity < now - 300)
                        {
                            client.Disconnect("Idle for " + (now - client.LastActivity) + " seconds.");
                        }
                    }
                }

                delay.WaitOne(5000);
            }
        }

    }
}
