using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace WindowsFormsApp1
{
    internal class SocketClient
    {
        private string _host;
        private int _port;
        private TcpClient _client;
        private NetworkStream _stream;
        private bool _connected;

        public SocketClient(string host = "127.0.0.1", int port = 65432)
        {
            _host = host;
            _port = port;
            _client = new TcpClient();
        }

        public bool Connect()
        {
            try
            {
                _client.Connect(_host, _port);
                _stream = _client.GetStream();
                _connected = true;
                Console.WriteLine("Connected to the server.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to server: {ex.Message}");
                return false;
            }
        }

        public void SendMessage(string message)
        {
            if (_connected && _stream != null)
            {
                try
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    _stream.Write(data, 0, data.Length);
                    Console.WriteLine("Message sent: " + message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending message: {ex.Message}");
                    Disconnect();
                }
            }
            Thread.Sleep(500);
        }

        public string ReceiveMessage()
        {
            if (_connected && _stream != null)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine("Message received: " + message);
                        return message;
                    }
                    else
                    {
                        Console.WriteLine("Server closed the connection.");
                        Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving message: {ex.Message}");
                    Disconnect();
                }
            }
            return null;
        }

        public void Disconnect()
        {
            if (_connected)
            {
                try
                {
                    _stream?.Close();
                    _client?.Close();
                    _connected = false;
                    Console.WriteLine("Disconnected from the server.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error disconnecting: {ex.Message}");
                }
            }
        }

        public void StartCommunication()
        {
            if (!Connect())
            {
                return;
            }

            Thread thread = new Thread(() =>
            {
                try
                {
                    while (_connected)
                    {
                        string receivedMessage = ReceiveMessage();
                        if (!string.IsNullOrEmpty(receivedMessage))
                        {
                            Console.WriteLine("Server: " + receivedMessage);
                            SendMessage("Echo: " + receivedMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in communication thread: {ex.Message}");
                }
                finally
                {
                    Disconnect(); // Ensure cleanup
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }
    }
}
