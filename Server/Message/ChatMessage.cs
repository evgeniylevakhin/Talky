﻿using System;
using Server.Authentication;
using Server.Client;

namespace Server.Message
{
    class ChatMessage : ServerMessage
    {

        public ChatMessage(ServerClient client, string message) : base(client, message) { }

        public override bool Valid()
        {
            return (_message.StartsWith("M:") && !_message.StartsWith("M:/"));
        }

        protected override void Process()
        {
            string actualMessage = _message.Substring(2);
            if (actualMessage.StartsWith(" "))
            {
                actualMessage = actualMessage.Substring(1);
            }

            if (actualMessage.StartsWith("register") || actualMessage.StartsWith("auth") || actualMessage.StartsWith("changepass"))
            {
                _client.SendMessage("§1To protect our users, we do not send messages starting with 'register', 'changepass' or 'auth'. This is in case they miss out the / on the register, changepassword or auth commands.");
                return;
            }

            int time = (int) (DateTime.UtcNow.Subtract(Program.StartTime)).TotalSeconds;
            int clientTime = _client.LastMessage;
            bool isAdmin = (_client.Account != null && _client.Account.Role == Role.Admin);

            if (!isAdmin && time - clientTime < Program.SpamDelay)
            {
                _client.SendMessage("§2Please slow down those messages! You must wait a few moments in between messages to prevent SPAM (not the meaty type).");
                return;
            }

            _client.LastMessage = time;
            _client.Channel.BroadcastMessage($"<{(_client.Account != null && _client.Account.Role.Equals(Role.Admin) ? "§2%" : "")}{_client.Username}§0> {actualMessage}");
        }

    }
}
