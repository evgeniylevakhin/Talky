﻿using Server.Channel;
using Server.Client;

namespace Server.Command
{
    class CommandClist : TalkyCommand
    {

        public CommandClist() : base("clist", "List all available channels.", "/clist") { }

        public override void Execute(ServerClient client, string[] args)
        {
            if (ChannelRepository.Instance.Count() == 0)
            {
                client.SendMessage("A problem happened. Sorry. :'(");
                return;
            }
            
            client.SendMessage("§1=CHANNELS=============");
            foreach (TalkyChannel channel in ChannelRepository.Instance.All())
            {
                client.SendMessage("[" + channel.Name + "] " + (channel.Locked ? "Locked" : "Unlocked"));
            }
            client.SendMessage("§1=CHANNELS=============");
        }

    }
}
