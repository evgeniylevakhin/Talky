using System.Collections.Generic;
using Server.Client;

namespace Server.Command
{
    class CommandHelp : TalkyCommand
    {

        public CommandHelp() : base("help", "See a list of commands.", "/help") { }

        public override void Execute(ServerClient client, string[] args)
        {
            IReadOnlyList<TalkyCommand> commands = CommandManager.Instance.All();
            client.SendMessage("§1=COMMANDS=============");

            foreach (TalkyCommand command in commands)
            {
                client.SendMessage("[" + command.Usage + "] " + command.Description);
            }

            client.SendMessage("§1=COMMANDS=============");
        }

    }
}
