namespace Server.Channel
{
    class ClientChannel : TalkyChannel
    {

        public ClientChannel(string name, bool inRecovery) : base(name, false, inRecovery) { }

    }
}
