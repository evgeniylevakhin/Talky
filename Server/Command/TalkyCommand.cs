using Server.Client;

namespace Server.Command
{
    abstract class TalkyCommand
    {

        public string CommandName { get; private set; }
        public string Description { get; private set; }
        public string Usage { get; private set; }

        protected TalkyCommand(string commandName, string description, string usage)
        {
            CommandName = commandName;
            Description = description;
            Usage = usage;
        }

        public void SendUsage(ServerClient client)
        {
            client.SendMessage("§1Usage: " + Usage);
        }

        public abstract void Execute(ServerClient client, string[] args);

    }
}
