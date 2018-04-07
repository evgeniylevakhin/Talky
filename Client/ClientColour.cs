using System.Drawing;

namespace Client
{
    class ClientColour
    {

        public Color Color { get; private set; }
        public bool MinusOne { get; private set; } = false;

        public ClientColour(Color color, bool minusOne)
        {
            Color = color;
            MinusOne = minusOne;
        }

    }
}
