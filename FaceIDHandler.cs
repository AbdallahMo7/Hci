using System;
using System.Text;
using WindowsFormsApp1;

namespace WindowsFormsApp1
{
    internal class FaceIDHandler
    {
        public SocketClient _socketClient;

        public FaceIDHandler(SocketClient socketClient)
        {
            _socketClient = socketClient;
        }

        public void Login()
        {
            try
            {
                _socketClient.SendMessage("$FaceIdentification$Login$");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
            }
        }
        public void Register(string username)
        {
            try
            {
                string message = "$FaceIdentification$Register$" + username;
                _socketClient.SendMessage(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during registration: {ex.Message}");
            }
        }

        public string DecodeMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return "Invalid message received.";

            try
            {
                if (message.StartsWith("$FaceIdentification$Login$"))
                {
                    string[] parts = message.Split('$');
                    if (parts.Length > 3)
                    {
                        return parts[3];
                    }
                    return "Failed";
                }
                else if (message.StartsWith("$FaceIdentification$Register$"))
                {
                    string[] parts = message.Split('$');
                    if (parts.Length > 4 && parts[3] == "Ok")
                    {
                        return parts[3];
                    }
                    else if (parts.Length > 4)
                    {
                        return parts[3] + "$" + parts[4];
                    }
                    return "Registration failed. Unknown error.";
                }

                return "Unknown message format.";
            }
            catch (Exception ex)
            {
                return $"Error decoding message: {ex.Message}";
            }
        }
    }
}
