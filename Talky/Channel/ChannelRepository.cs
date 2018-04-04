using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using Talky.Database;

namespace Talky.Channel
{
    internal class ChannelRepository
    {

        public static ChannelRepository Instance { get; } = new ChannelRepository();
        private readonly List<TalkyChannel> _channels = new List<TalkyChannel>();
        private readonly object _lock = new object();

        private ChannelRepository() { }
        
        public void Store(TalkyChannel channel)
        {
            lock (_lock)
            {
                _channels.Add(channel);
            }

            string lobbyString = (channel is LobbyChannel ? "true" : "false");
            string lockedString = (channel.Locked ? "true" : "false");

            MySqlConnection connection = MySqlConnector.GetConnection();
            if (connection != null)
            {
                MySqlCommand command = new MySqlCommand("INSERT INTO `channels` VALUES(NULL, @channel_name, @lobby_type, @locked)", connection);
                command.Prepare();
                command.Parameters.AddWithValue("@channel_name", channel.Name);
                command.Parameters.AddWithValue("@lobby_type", lobbyString);
                command.Parameters.AddWithValue("@locked", lockedString);
                try
                {
                    command.ExecuteReader();
                }
                catch
                {
                    Console.WriteLine("channels table: could not INSERT " + channel.Name);
                }
                connection.Close();
            }

        }

        public LobbyChannel GetLobby()
        {
            lock (_lock)
            {
                return (LobbyChannel) _channels.FirstOrDefault(channel => channel is LobbyChannel);
            }
        }

        public TalkyChannel Get(string name)
        {
            lock (_lock)
            {
                return _channels.FirstOrDefault(channel => string.Equals(channel.Name, name, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        public bool Exists(string name)
        {
            return (Get(name) != null);
        }

        public IReadOnlyList<T> Get<T>() where T : TalkyChannel
        {
            lock (_lock)
            {
                var channels = _channels.FindAll(channel => channel is T);
                return channels.Cast<T>().ToList();
            }
        }

        public bool Exists<T>() where T : TalkyChannel
        {
            return Get<T>().Count > 0;
        }

        public IReadOnlyCollection<TalkyChannel> All()
        {
            lock (_lock)
            {
                return new List<TalkyChannel>(_channels).AsReadOnly();
            }
        }

        public int Count()
        {
            lock (_lock)
            {
                return _channels.Count;
            }
        }

        public void Remove(TalkyChannel channel)
        {
            lock (_lock)
            {
                _channels.Remove(channel);
            }

            MySqlConnection connection = MySqlConnector.GetConnection();
            if (connection != null)
            {
                MySqlCommand command = new MySqlCommand("DELETE FROM `channels` WHERE channel_name = @channel_name", connection);
                command.Prepare();
                command.Parameters.AddWithValue("@channel_name", channel.Name);
                command.ExecuteReader();
                connection.Close();
            }

        }

    }
}
