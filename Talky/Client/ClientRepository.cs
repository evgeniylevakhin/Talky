﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Talky.Channel;

namespace Talky.Client
{
    class ClientRepository
    {

        public static ClientRepository Instance { get; } = new ClientRepository();
        private List<ServerClient> _clients = new List<ServerClient>();
        private object _lock = new object();

        private ClientRepository() { }

        public void Store(ServerClient client)
        {
            lock (_lock)
            {
                _clients.Add(client);
            }
        }

        public ServerClient Find(string username)
        {
            username = username.ToLower();
            lock (_lock)
            {
                foreach (ServerClient client in _clients)
                {
                    if (client.Username.Equals(username))
                    {
                        return client;
                    }
                }
            }
            return null;
        }

        public bool Exists(string username)
        {
            if (username.Equals("%"))
            {
                return true;
            }
            return (Find(username) != null);
        }

        public ServerClient Find(TcpClient tcpClient)
        {
            lock (_lock)
            {
                foreach (ServerClient client in _clients)
                {
                    if (client.TcpClient.Equals(tcpClient))
                    {
                        return client;
                    }
                }
            }
            return null;
        }

        public IReadOnlyCollection<ServerClient> Find(ServerChannel channel)
        {
            lock (_lock)
            {
                List<ServerClient> clients = new List<ServerClient>();
                foreach (ServerClient client in _clients)
                {
                    if (client.Channel.Equals(channel))
                    {
                        clients.Add(client);
                    }
                }
                return clients.AsReadOnly();
            }
        }

        public IReadOnlyCollection<ServerClient> All()
        {
            lock (_lock)
            {
                return new List<ServerClient>(_clients).AsReadOnly();
            }
        }

        public int Count()
        {
            lock (_lock)
            {
                return _clients.Count;
            }
        }

        public void Remove(ServerClient client)
        {
            lock (_lock)
            {
                _clients.Remove(client);
            }
        }

    }
}
