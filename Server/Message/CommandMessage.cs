﻿using System;
using Server.Client;
using Server.Command;

namespace Server.Message
{
    class CommandMessage : ChatMessage
    {

        public CommandMessage(ServerClient client, string message) : base(client, message) { }

        public override bool Valid()
        {
            return _message.StartsWith("M:/");
        }

        protected override void Process()
        {
            string[] split = _message.Substring(3).Split(new char[] { ' ' });
            string command = split[0].ToLower();
            string[] args = new string[split.Length - 1];

            if (args.Length > 0)
            {
                args = _message.Substring(4 + command.Length).Split(new char[] { ' ' });
            }

            if (_client.Channel == null && !(command.Equals("name") || command.Equals("auth")))
            {
                _client.SendMessage("§1Please use /name <name> to set a username before using commands.");
                _client.SendMessage("§1If you are a registered client, please use /auth to claim your username.");
                return;
            }

            TalkyCommand theCommand = CommandManager.Instance.Get(command);
            if (theCommand == null)
            {
                _client.SendMessage("§2That command does not exist.");
                return;
            }

            int time = (int) (DateTime.UtcNow.Subtract(Program.StartTime)).TotalSeconds;
            int clientTime = _client.LastCommand;
            bool isAdmin = (_client.Account != null && _client.Account.Role == Authentication.Role.Admin);

            if (!isAdmin && time - clientTime < Program.SpamDelay)
            {
                _client.SendMessage("§2Please slow down those commands! You must wait a few moments in between commands to prevent SPAM (not the meaty type).");
                return;
            }

            _client.LastCommand = time;
            theCommand.Execute(_client, args);
        }

    }
}
