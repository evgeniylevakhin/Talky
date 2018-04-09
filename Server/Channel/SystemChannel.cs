namespace Server.Channel
{
    internal class SystemChannel : TalkyChannel
    {

        public SystemChannel(string name, bool locked) : base(name, locked, false) { }

    }
}
