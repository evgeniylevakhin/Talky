﻿using Server.Channel;
using Server.Client;

namespace Server.Command
{
    class CommandJoin : TalkyCommand
    {

        public CommandJoin() : base("join", "Join another channel.", "/join +<channel>") { }

        public override void Execute(ServerClient client, string[] args)
        {
            if (args.Length != 1)
            {
                SendUsage(client);
                return;
            }

            string desiredChannel = args[0];

            if (!desiredChannel.StartsWith("+"))
            {
                desiredChannel = "+" + desiredChannel;
            }

            TalkyChannel channel = ChannelRepository.Instance.Get(desiredChannel);
            if (channel == null)
            {
                client.SendMessage("§2That channel does not exist.");
                client.SendMessage("§2Use /clist to see a list of channels.");
                client.SendMessage("§2Use /cc to create a temporary channel.");
                return;
            }

            if (client.Channel.Equals(channel))
            {
                client.SendMessage("§2You are already in that channel!");
                return;
            }

            client.JoinChannel(channel);
        }

    }
}
