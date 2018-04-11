using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Client.Connection
{
    public class ServerConnection : IDisposable
    {

        public string Host { get; }
        public int Port { get; }
        public string Username { get; set; }
        private TcpClient _client;
        private StreamReader _reader;
        private StreamWriter _writer;

        private readonly Config _config;

        private static readonly object Lock = new object();

        public ServerConnection(Config config) 
        {
            Host = config.Hostname;
            Port = config.Port;
            _config = config;
            Connect();
        }

        public void Connect()
        {
            //while (true)
            {
                try
                {
                    Disconnect();
                    _client = new TcpClient(Host, Port);
                    _reader = new StreamReader(_client.GetStream());
                    _writer = new StreamWriter(_client.GetStream());

                    if (_config.Password.Equals(""))
                    {
                        Send("M:/name " + _config.UserName);
                    }
                    else
                    {
                        Send("M:/auth " + _config.UserName + " " + _config.Password);
                    }

                    Send("S:Client");
                    Send("S:Account");
                    Send("S:ChannelClientList");
                }
                catch
                {
                    _client = null;
                }
                //Thread.Sleep(500);
            }
        }

        public bool IsConnected()
        {
            lock (Lock)
            {
                return _client != null && _client.Connected;
            }
        }

        public bool Send(string msg)
        {
            try
            {
                _writer.WriteLine(msg);
                _writer.Flush();
            }
            catch (Exception e)
            {
                //todo:
                return false;
            }
            return true;
        }

        public void Disconnect()
        {
            lock (Lock)
            {
                try
                {
                    _reader?.Dispose();
                    _writer?.Dispose();
                    _client.Close();
                    ((IDisposable)_client)?.Dispose();
                }
                catch (Exception e)
                {
                    //log exceptions somewhere?
                }
            }
        }

        public string Read()
        {
            var msg = string.Empty;

            try
            {
                msg = _reader.ReadLine();
            }
            catch (Exception e)
            {
                //todo::
            }

            return msg;
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
