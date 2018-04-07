using System.Collections.Generic;
using System.Drawing;

namespace Client
{
    internal sealed class ChatMessage
    {

        public Dictionary<string, Color> Pieces = new Dictionary<string, Color>();

        public const string BLACK = "0";
        public const string BLUE = "1";
        public const string RED = "2";
        public const string YELLOW = "3";
        public const string GREEN = "4";

        private readonly Color _previousColour = Color.Black;

        public ChatMessage(string message)
        {
            foreach (string piece in message.Split('§'))
            {
                ClientColour colour = GetColour(piece);
                Pieces.Add(piece.Substring(colour.MinusOne ? 1 : 0), colour.Color);
                _previousColour = colour.Color;
            }
        }

        private ClientColour GetColour(string theString)
        {
            if (theString.StartsWith(BLACK))
            {
                return new ClientColour(Color.Black, true);
            } else if (theString.StartsWith(BLUE))
            {
                return new ClientColour(Color.Blue, true);
            } else if (theString.StartsWith(RED))
            {
                return new ClientColour(Color.Red, true);
            } else if (theString.StartsWith(YELLOW))
            {
                return new ClientColour(Color.Yellow, true);
            } else if (theString.StartsWith(GREEN))
            {
                return new ClientColour(Color.Green, true);
            }
            return new ClientColour(_previousColour, false);
        }

    }
}
