using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    internal class SessionTracker
    {
        public readonly string UserName;
        string folderPath = "Sessions";
        public int currentExibitIndex = -1;
        public List<string> VisitedExibits = new List<string>();
        public List<int> VisitedExibitsTime = new List<int>();

        public SessionTracker(string userName)
        {
            UserName = userName;
            GetUserData();
        }

        private void GetUserData()
        {
            
            string filePath = Path.Combine(folderPath, UserName + ".txt");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (File.Exists(filePath))
            {
                try
                {
                    string[] lines = File.ReadAllLines(filePath);

                    foreach (var line in lines)
                    {
                        string[] parts = line.Split(',');

                        if (parts.Length == 2 && int.TryParse(parts[1], out int timeSpent))
                        {
                            VisitedExibits.Add(parts[0]);
                            VisitedExibitsTime.Add(timeSpent);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading user data: {ex.Message}");
                }
            }
            else
            {
                // Create an empty file if none exists
                File.Create(filePath).Dispose();
            }
        }

        public void SaveUserData()
        {
            string filePath = Path.Combine(folderPath, UserName + ".txt");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, false))
                {
                    for (int i = 0; i < VisitedExibits.Count; i++)
                    {
                        writer.WriteLine($"{VisitedExibits[i]},{VisitedExibitsTime[i]}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving user data: {ex.Message}");
            }
        }
    }
}
