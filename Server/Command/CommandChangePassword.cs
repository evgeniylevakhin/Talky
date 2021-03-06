﻿using System;
using Server.Client;

namespace Server.Command
{
    class CommandChangePassword : TalkyCommand
    {

        public CommandChangePassword() : base("changepassword", "Change your password.", "/changepassword <current> <new> <repeat>") { }

        public override void Execute(ServerClient client, string[] args)
        {
            if (client.Account == null)
            {
                client.SendMessage("§2You must be authenticated to use this command. See /auth.");
                return;
            }

            if (args.Length != 3)
            {
                SendUsage(client);
                return;
            }

            string currentPassword = args[0];
            string newPassword = args[1];
            string newPasswordRepeat = args[2];

            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                client.SendMessage("§2Invalid password. Passwords must contain at least 6 characters. Passwords may not be whitespace.");
                return;
            }

            if (!client.Account.ComparePassword(currentPassword))
            {
                client.SendMessage("§2Failed to authenticate. Invalid password.");
                return;
            }

            if (client.Account.ComparePassword(newPassword))
            {
                client.SendMessage("§2New password is the same as your old password.");
                return;
            }
            
            if (!newPassword.Equals(newPasswordRepeat, StringComparison.Ordinal))
            {
                client.SendMessage("§2New password and password confirmation do not match. Please try again.");
                return;
            }

            client.Account.SetPassword(newPassword);
            client.SendMessage("§4Your password has been updated.");
        }

    }
}
