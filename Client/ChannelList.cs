using System;
using System.Windows.Forms;
using Client.Connection;

namespace Client
{
    public partial class ChannelList : Form
    {
        private readonly string[] _channels;
        private readonly ServerConnection _connection;

        public ChannelList(string[] channels, ServerConnection connection)
        {
            InitializeComponent();
            _channels = channels;
            _connection = connection;
        }

        private void ChannelList_Load(object sender, EventArgs e)
        {
            foreach (var channel in _channels)
            {
                _channelList.Items.Add(channel);
            }
        }

        private void _joinButton_Click(object sender, EventArgs e)
        {
            if (!(_channelList.SelectedItem is string)) return;
            _connection.Send("M:/join " + _channelList.SelectedItem);
            _connection.Send("S:Client");
            Close();
        }
    }
}
