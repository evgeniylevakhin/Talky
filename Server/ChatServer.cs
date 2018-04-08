using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Server.Channel;
using Server.Client;
using Server.Command;
using Server.Connection;
using Server.Exception;

namespace Server
{
    public class ChatServer
    {
        private readonly int _port;
        private readonly DateTime _started = DateTime.Now;
        private bool _isActive = true;

        private readonly AutoResetEvent _consoleWaitEvent = new AutoResetEvent(false);
        private readonly ChannelRepository _channelRepository = ChannelRepository.Instance;
        private readonly ClientRepository _clientRepository = ClientRepository.Instance;
        private readonly CommandManager _commandManager = CommandManager.Instance;
        private Thread _listenerThread;
        private TcpListener _listener;

        public ChatServer(int port)
        {
            _port = port;
        }

        public int UserCount => _clientRepository.Count();
        public int ChannelCount => _channelRepository.Count();
        public int CommandCount => _commandManager.Count();

        public void ShutDown()
        {
            _isActive = false;
            _listener?.Stop();
        }

        public void Start()
        {
            _isActive = true;

            while (_isActive)
            {
                try
                {
                    if (!_listenerThread.IsAlive)
                    {
                        _listenerThread.Start();
                    }

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

                    var clients = _clientRepository.All();
                    if (clients.Count > 0)
                    {
                        foreach (var client in clients)
                        {
                            int now = (int) (DateTime.UtcNow.Subtract(_started)).TotalSeconds;
                            if (client.LastActivity < now - 300)
                            {
                                client.Disconnect("Idle for " + (now - client.LastActivity) + " seconds.");
                            }
                        }
                    }

                    Console.Clear();
                    Console.WriteLine("Talky | Created by SysVoid");
                    Console.WriteLine("==========================");
                    Console.WriteLine("Clients: " + UserCount);
                    Console.WriteLine("Channels: " + ChannelCount);
                    Console.WriteLine("Commands: " + CommandCount);
                    Console.WriteLine("==========================");
                    _consoleWaitEvent.WaitOne(1000);
                }
                catch (System.Exception ex)
                {
                    //todo:: 
                }
            }
        }

        private void ListenForClients()
        {
            while (_isActive)
            {
                try
                {
                    if (_listener == null)
                    {
                        _listener = new TcpListener(IPAddress.Any, _port);
                        _listener.Start();
                    }

                    var tcpClient = _listener.AcceptTcpClient();
                    var serverClient = new ServerClient(tcpClient);

                    if (_channelRepository.GetLobby() == null)
                    {
                        serverClient.Disconnect("§2Server Error: No Lobby!");
                        continue;
                    }

                    Thread clientThread = new Thread(new ServerConnection(serverClient).HandleMessages);
                    clientThread.Start();
                }
                catch (System.Exception e)
                {
                    //todo:: add loggin
                }
            }
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

        public void Init()
        {
            RegisterCommands();
            InitChannels();
            _listenerThread = new Thread(ListenForClients);
        }

        private void RegisterCommands()
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
    }
}
