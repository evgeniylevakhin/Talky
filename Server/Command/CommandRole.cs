﻿using Server.Authentication;
using Server.Client;

namespace Server.Command
{
    class CommandRole : TalkyCommand
    {

        public CommandRole() : base("role", "Update a user's account role.", "/role <username> <user/admin>") { }

        public override void Execute(ServerClient client, string[] args)
        {
            if (client.Account == null || !client.Account.Role.Equals("admin"))
            {
                client.SendMessage("§2That command is admin only!");
                return;
            }

            if (args.Length != 2)
            {
                SendUsage(client);
                return;
            }

            string username = args[0];
            string role = "user";
            if (args[1].ToLower().Equals("admin"))
            {
                role = "admin";
            }

            UserAccount account = UserAccount.Find(username);
            if (account == null)
            {
                client.SendMessage("§2User not found.");
                return;
            }

            if (account.SetRole(role))
            {
                client.SendMessage("§4Account role for " + account.Username + " set to " + role + ".");

                ServerClient foundClient = ClientRepository.Instance.Find(account.Username);
                if (foundClient != null)
                {
                    foundClient.Account = account;
                    if (foundClient.Account != null)
                    {
                        foundClient.SendRawMessage($"S:Account:{foundClient.Account.AccountId};{foundClient.Account.Username};{foundClient.Account.Role}");
                    }
                    foundClient.SendMessage("§1Your role was set to " + role + " by " + client.Username + ".");
                }
                return;
            }
            client.SendMessage("§2Failed to update account role for " + account.Username + ".");
        }

    }
}
