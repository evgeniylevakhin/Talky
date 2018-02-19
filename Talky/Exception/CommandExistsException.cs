using Talky.Command;

namespace Talky.Exception
{
    class CommandExistsException : System.Exception
    {

        public TalkyCommand Command { get; private set; }

        public CommandExistsException(string message, TalkyCommand command) : base(message)
        {
            Command = command;
        }

    }
}
