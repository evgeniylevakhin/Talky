using System;
using System.IO;
using Server.Client;
using Server.Message;

namespace Server.Connection
{
    public class ServerConnection
    {
        private bool _isRunning = true;

        public ServerClient Client { get; }

        public ServerConnection(ServerClient client)
        {
            Client = client;
            ClientRepository.Instance.Store(client);
        }

        public void Disconnect()
        {
            _isRunning = false;
        }

        public void HandleMessages()
        {
            var reader = new StreamReader(Client.TcpClient.GetStream());

            while (_isRunning)
            {

                string line = null;

                try
                {
                    line = reader.ReadLine()?.Replace("§", "");
                }
                catch
                {
                    line = null;
                }

                if (string.IsNullOrEmpty(line))
                {
                    Client.Disconnect();
                    return;
                }

                while (line.Contains("  "))
                {
                    line = line.Replace("  ", " ");
                }

                while (line.EndsWith(" "))
                {
                    line = line.Substring(0, line.Length - 1);
                }

                ChatMessage chatMessage = new ChatMessage(Client, line);
                CommandMessage commandMessage = new CommandMessage(Client, line);
                StatMessage statMessage = new StatMessage(Client, line);

                Client.LastActivity = (int)(DateTime.UtcNow.Subtract(Program.StartTime)).TotalSeconds;

                if (chatMessage.Valid())
                {
                    chatMessage.Handle();
                }
                else if (commandMessage.Valid())
                {
                    commandMessage.Handle();
                }
                else if (statMessage.Valid())
                {
                    statMessage.Handle();
                }
                else
                {
                    // ???
                    Client.Disconnect("§2What was that?");
                    return;
                }
            }
        }

    }
}
