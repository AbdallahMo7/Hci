using System;
using System.Collections.Generic;

namespace WindowsFormsApp1
{
    internal class MarkerHandler
    {
        private readonly SocketClient _socketClient;
        public bool started = false;

        public MarkerHandler(SocketClient socketClient)
        {
            _socketClient = socketClient ?? throw new ArgumentNullException(nameof(socketClient));
        }

        // Sends start command to the server
        public void StartMarkerRecognition()
        {
            _socketClient.SendMessage("$MarkerRecognition$");
            started = true;
            Console.WriteLine("Marker recognition started.");
        }

        // Sends stop command to the server
        public void StopMarkerRecognition()
        {
            _socketClient.SendMessage("$MarkerRecognitionStop$");
            started = false;
            Console.WriteLine("Marker recognition stopped.");
        }

        public Dictionary<string, object> ProcessMarkerData(string response)
        {
            try
            {
                if (string.IsNullOrEmpty(response) || !response.StartsWith("$MarkerRecognition$"))
                {
                    Console.WriteLine("Invalid or empty response received.");
                    return null;
                }

                // Example response: "$MarkerRecognition$Object ID: 2, Position: (0.35, 0.75), Orientation: 45.0$"
                string dataSection = response.Split('$')[2];

                // Parse the data and extract information
                var parsedData = ParseMarkerData(dataSection);

                Console.WriteLine("Marker data processed successfully.");
                return parsedData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing marker data: {ex.Message}");
                return null;
            }
        }

        // Helper method to parse the marker data into a structured format
        private Dictionary<string, object> ParseMarkerData(string data)
        {
            var result = new Dictionary<string, object>();

            try
            {
                // Split the string by commas
                string[] parts = data.Split(',');

                foreach (string part in parts)
                {
                    string[] keyValue = part.Split(new[] { ':' }, 2);

                    if (keyValue.Length == 2)
                    {
                        string key = keyValue[0].Trim();
                        string value = keyValue[1].Trim();

                        // Parse numeric values where possible
                        if (double.TryParse(value, out double numericValue))
                        {
                            result[key] = numericValue;
                        }
                        else
                        {
                            result[key] = value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing marker data: {ex.Message}");
            }

            return result;
        }
    }
}
