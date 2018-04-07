using System;
using System.Windows.Forms;
using Client.Connection;

namespace Client
{
    public partial class ConnectForm : Form
    {

        public static ConnectForm Instance { get; private set; }

        public ConnectForm()
        {
            InitializeComponent();
            Instance = this;
        }

        private static bool ValidUsername(string desiredUsername)
        {
            return !(string.IsNullOrEmpty(desiredUsername) || string.IsNullOrWhiteSpace(desiredUsername) || desiredUsername.Length > 16 || desiredUsername.Contains("%") || desiredUsername.Contains("/") || desiredUsername.Contains("@") || desiredUsername.Contains("\\") || desiredUsername.Contains(";"));
        }

        private void _quitButton_Click(object sender, EventArgs e)
        {
            Instance = null;
            Environment.Exit(0);
        }

        private void _connectButton_Click(object sender, EventArgs e)
        {
            string host = _hostInput.Text;
            bool validPort = int.TryParse(_portInput.Text, out var port);
            string username = _usernameInput.Text;

            if (string.IsNullOrEmpty(host) || string.IsNullOrWhiteSpace(host))
            {
                MessageBox.Show(@"Please correct the errors with the host field.", @"Host");
                return;
            }

            if (string.IsNullOrEmpty(_portInput.Text) || !validPort)
            {
                MessageBox.Show(@"Please correct the errors with the port field.", @"Port");
                return;
            }

            if (!ValidUsername(username))
            {
                MessageBox.Show(@"Please correct the errors with the username field.", @"Username");
                return;
            }

            string password = "";
            if (!string.IsNullOrEmpty(_passwordInput.Text) && !string.IsNullOrWhiteSpace(_passwordInput.Text))
            {
                password = _passwordInput.Text;
            }

            var config = new Config
            {
                Hostname = host,
                Port = port,
                UserName = username,
                Password = password
            };

            using (var connection = new ServerConnection(config))
            using (var chat = new ChatWindow(connection))
            {
                Hide();
                chat.ShowDialog();
            }
            Show();
        }

        private void _hostInput_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_hostInput.Text) || string.IsNullOrWhiteSpace(_hostInput.Text))
            {
                _hostErrorImage.Visible = true;
            } else
            {
                _hostErrorImage.Visible = false;
            }
        }

        private void _portInput_TextChanged(object sender, EventArgs e)
        {
            int ignored;
            if (string.IsNullOrEmpty(_portInput.Text) || string.IsNullOrWhiteSpace(_portInput.Text) || !int.TryParse(_portInput.Text, out ignored))
            {
                _portErrorImage.Visible = true;
            } else
            {
                _portErrorImage.Visible = false;
            }
        }

        private void _usernameInput_TextChanged(object sender, EventArgs e)
        {
            _usernameErrorImage.Visible = !ValidUsername(_usernameInput.Text);
        }

        private void ConnectForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Instance = null;
            Environment.Exit(0);
        }

    }
}
