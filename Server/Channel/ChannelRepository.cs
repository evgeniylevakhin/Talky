using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using Server.Database;

namespace Server.Channel
{
    internal class ChannelRepository
    {

        public static ChannelRepository Instance { get; } = new ChannelRepository();
        private readonly List<TalkyChannel> _channels = new List<TalkyChannel>();
        private readonly object _lock = new object();

        private ChannelRepository() { }
        
        public void Store(TalkyChannel channel, bool writeToDB)
        {
            lock (_lock)
            {
                _channels.Add(channel);


                string lobbyString = (channel is LobbyChannel ? "true" : "false");
                string lockedString = (channel.Locked ? "true" : "false");

                if (true == writeToDB)
                {
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
            }
        }

        public void RestoreFromDB()
        {
            MySqlConnection connection = MySqlConnector.GetConnection();
            if (connection != null)
            {
                MySqlCommand command = new MySqlCommand("SELECT `channel_name`, `lobby_type`, `locked` FROM `channels` ORDER BY `id` ASC", connection);
                command.Prepare();
                try
                {
                    MySqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string channelName = reader.GetString("channel_name");
                        string lobbyString = reader.GetString("lobby_type");
                        string lockedString = reader.GetString("locked");
                        TalkyChannel restoredChannel;

                        if (lobbyString.Equals("true"))
                        {
                            restoredChannel = new LobbyChannel(channelName);
                        }
                        else if (lockedString.Equals("true"))
                        {
                            restoredChannel = new SystemChannel(channelName, true);
                        }
                        else
                        {
                            restoredChannel = new ClientChannel(channelName, true);
                        }

                        Store(restoredChannel, false);
                    }
                }
                catch
                {
                    Console.WriteLine("channels table: could not SELECT in RestoreFromDB ");
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
            bool retVal = false;

            MySqlConnection connection = MySqlConnector.GetConnection();

            if (connection != null)
            {
                MySqlCommand command = new MySqlCommand("SELECT `id` FROM `channels` WHERE `channel_name`=@channel_name ORDER BY `id` ASC LIMIT 1", connection);
                command.Prepare();
                command.Parameters.AddWithValue("@channel_name", name);

                try
                {
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        retVal = true;
                    }

                }
                catch
                {
                    Console.WriteLine("channels table: could not SELECT " + name);
                }
                connection.Close();
            }

            return retVal;
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
                // Remove gets called when there are 0 clients in the channel.
                // However, if the channel is just recently restored from recovery and clients have not reconnected yet,
                // don't delete the channel!  Btw, the InRecovery flag is set back to false as soon as the first client joins.
                if (false == (channel.InRecovery))
                {
                    _channels.Remove(channel);

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
    }
}
