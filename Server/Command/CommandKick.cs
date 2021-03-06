﻿using Server.Client;

namespace Server.Command
{
    class CommandKick : TalkyCommand
    {

        public CommandKick() : base("kick", "Kick a client from the server.", "/kick <username>") { }

        public override void Execute(ServerClient client, string[] args)
        {
            if (client.Account == null || client.Account.Role != Authentication.Role.Admin)
            {
                client.SendMessage("§2That command is admin only!");
                return;
            }

            if (args.Length != 1)
            {
                SendUsage(client);
                return;
            }

            string username = args[0];
            if (ClientRepository.Instance.Find(username) == null)
            {
                client.SendMessage("§2No client found with that username.");
                return;
            }
            ClientRepository.Instance.Find(username).Disconnect("§2Kicked from server.");
            client.SendMessage("§4" + username + " was kicked from the server.");
        }

    }
}
