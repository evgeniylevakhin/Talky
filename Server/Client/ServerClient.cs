﻿using System;
using System.IO;
using System.Net.Sockets;
using Server.Authentication;
using Server.Channel;

namespace Server.Client
{
    public class ServerClient
    {
        public TcpClient TcpClient { get; set; }

        public int LastMessage { get; set; } = 0;
        public int LastCommand { get; set; } = 0;
        public int LastActivity { get; set; } = 0;

        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                if (value.Length > 16)
                {
                    value = value.Substring(0, 15);
                }
                _username = value.Replace(";", "-");
            }
        }

        public bool Muted { get; set; } = false;
        public TalkyChannel Channel { get; private set; }

        public UserAccount Account { get; set; } = null;

        public ServerClient(TcpClient client)
        {
            Username = "%";
            TcpClient = client;
            LastActivity = (int) (DateTime.UtcNow.Subtract(Program.StartTime)).TotalSeconds;
        }

        public void SendMessage(string message)
        {
            SendRawMessage("M:" + message);
        }

        public void SendRawMessage(string message)
        {
            try
            {
                StreamWriter writer = new StreamWriter(TcpClient.GetStream());
                writer.WriteLine(message);
                writer.Flush();
            } catch
            {
                Disconnect();
            }
        }

        public void Disconnect(string reason = null)
        {
            ClientRepository.Instance.Remove(this);
            if (reason != null)
            {
                Channel.BroadcastMessage(Username + " disconnected: " + reason);
                using (var writer = new StreamWriter(TcpClient.GetStream()))
                {
                    writer.WriteLine("M:§2You were disconnected from the server. Reason: " + reason);
                    writer.Flush();
                }
            } else
            {
                if (!string.IsNullOrEmpty(Username) && !Username.Equals("%"))
                {
                    Channel.BroadcastMessage(Username + " disconnected.");
                }
            }

            TcpClient.Client.Close();
        }

        public void JoinChannel(TalkyChannel channel, bool announce = true)
        {
            if (channel.Locked)
            {
                if (Account == null || Account.Role != Role.Admin)
                {
                    SendMessage("§2That channel is locked!");
                    return;
                }
            }

            Channel?.BroadcastMessage(Username + " left " + "§4" + Channel.Name + "§0.");
            Channel = channel;

            if (announce)
            {
                channel.BroadcastMessage(Username + " joined " + "§4" + channel.Name + "§0!");
            }
            channel.InRecovery = false;
        }
    }
}
