using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client.Connection;

namespace Client
{
    public partial class ChatWindow : Form
    {
        private readonly ServerConnection _connection;
        private Thread _listeningThread;
        private bool _isRunning;

        public ChatWindow(ServerConnection conn)
        {
            InitializeComponent();
            _connection = conn;
        }

        private void ChatWindow_Shown(object sender, EventArgs e)
        {
            if (!_connection.IsConnected())
            {
                MessageBox.Show(@"Could not connect to the Talky Chat Server!", @"Connection Failure");
                Disconnect();
                return;
            }

            _listeningThread = new Thread(Listen) { Name = "Message Listener Thread" };
            _listeningThread.Start();
        }

        private void Listen()
        {
            _isRunning = true;
            while (_isRunning)
            {
                //move message processing into different function
                string line = _connection.Read();

                if (!_isRunning)
                    break;

                if (!_connection.IsConnected())
                {
                    _connection.Connect();
                    Task.Delay(1000);
                    continue;
                }

                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                if (line.StartsWith("M:"))
                {
                    ChatMessage theMessage = new ChatMessage(line.Substring(2));

                    _messageLog.Invoke((MethodInvoker)delegate
                   {
                       foreach (string piece in theMessage.Pieces.Keys)
                       {
                           _messageLog.SelectionColor = theMessage.Pieces[piece];
                           _messageLog.AppendText(piece);
                       }
                       _messageLog.AppendText(Environment.NewLine);
                   });

                    if (_connection.IsConnected())
                    {
                        _connection.Send("S:ChannelClientList");
                    }
                }
                else if (line.StartsWith("S:ChannelList:"))
                {
                    string[] channels = line.Substring(14).Split(';');
                    new ChannelList(channels, _connection).ShowDialog();
                }
                else if (line.StartsWith("S:ChannelClientList:"))
                {
                    string[] clients = line.Substring(20).Split(';');
                    _clientListComboBox.Invoke((MethodInvoker)delegate
                   {
                       _clientListComboBox.Items.Clear();
                       _clientListComboBox.Items.AddRange(clients);
                   });
                }
                else if (line.StartsWith("S:Client:"))
                {
                    string[] data = line.Substring(9).Split(';');
                    string username = data[0];
                    string muted = data[1];
                    string channel = data[2];

                    _titleLabel.Invoke((MethodInvoker)delegate
                   {
                       _titleLabel.Text = $@"[{username}]  {channel} on {_connection.Host}:{_connection.Port}";
                   });

                    Invoke((MethodInvoker)delegate
                   {
                       Text = $@"[{username}] {channel} on {_connection.Host}:{_connection.Port}";
                   });
                }
                else if (line.StartsWith("S:Account:"))
                {
                    string[] data = line.Substring(9).Split(';');
                    string accountId = data[0];
                    string username = data[1];
                    string role = data[2];

                    _accountLabel.Invoke((MethodInvoker)delegate
                   {
                       _accountLabel.Visible = true;
                   });

                    _accountUsernameLabel.Invoke((MethodInvoker)delegate
                   {
                       _accountUsernameLabel.Visible = true;
                       _accountUsernameLabel.Text = username;
                   });

                    _accountRoleLabel.Invoke((MethodInvoker)delegate
                   {
                       _accountRoleLabel.Visible = true;
                       _accountRoleLabel.Text = role;
                   });

                    _connection.Send("S:ChannelClientList");
                }
            }
        }

        private void _channelsButton_Click(object sender, EventArgs e)
        {
            _connection.Send("S:ChannelList");
            _connection.Send("S:ChannelClientList");
        }

        private void ChatWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Disconnect();
            ConnectForm.Instance.Show();

            _accountLabel.Visible = true;
            _accountUsernameLabel.Visible = true;
            _accountRoleLabel.Visible = true;
        }

        private void _sendButton_Click(object sender, EventArgs e)
        {
            string message = _messageInput.Text;
            if (string.IsNullOrEmpty(message) || string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            _connection.Send("M:" + message);
            _connection.Send("S:Client");
            _connection.Send("S:Account");
            _connection.Send("S:ChannelClientList");
            _messageInput.Text = "";
        }

        private void _disconnectButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Disconnect()
        {
            _isRunning = false;
            _connection?.Disconnect();
            _listeningThread?.Join();
        }
    }
}
