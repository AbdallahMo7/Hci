using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using WindowsFormsApp1.Components;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        Bitmap bitmap;
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        MainAppUI mainAppUI;
        SocketClient socketClient, YOLOSocketClient;
        Thread receiveThread, YOLOReceiveThread;

        VideoPlayer videoPlayer;
        TextBox textBox;
        Button Submit, TryAgain;

        public Form1()
        {
            Paint += new PaintEventHandler(OnPaint);
            Load += new EventHandler(OnLoad);
            FormClosing += new FormClosingEventHandler(OnFormClosing);
            timer.Tick += new EventHandler(time);
            timer.Start();
        }
        private void OnLoad(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            bitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
            videoPlayer = new VideoPlayer(this);
            mainAppUI = new MainAppUI(ClientSize.Width, ClientSize.Height, videoPlayer);
            // Initialize the SocketClient
            socketClient = new SocketClient();
            if (socketClient.Connect())
            {
                receiveThread = new Thread(ReceiveMessages);
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            mainAppUI.socketClient = socketClient;
            mainAppUI.markerHandler = new MarkerHandler(socketClient);
            mainAppUI.welcomeScreen.faceIDHandler._socketClient = socketClient;

            YOLOSocketClient = new SocketClient("127.0.0.2", 3322);
            if (YOLOSocketClient.Connect())
            {
                YOLOReceiveThread = new Thread(ReceiveMessagesYOLO);
                YOLOReceiveThread.IsBackground = true;
                YOLOReceiveThread.Start();
            }
            mainAppUI.YOLOSocketClient = YOLOSocketClient;

            // textbox
            textBox = new TextBox();
            textBox.Width = 200; textBox.Height = 25;
            textBox.Name = "MyTextBox";
            textBox.Font = new Font("Arial", 18);
            textBox.Location = new Point(ClientSize.Width/2 - textBox.Width/2, ClientSize.Height/2 - textBox.Height/2);
            this.Controls.Add(textBox);
            textBox.Hide();
            mainAppUI.welcomeScreen.textBox = textBox;

            // button submit
            Submit = new Button();
            Submit.Width = 100; Submit.Height = 30;
            Submit.Name = "Submit";
            Submit.Text = Submit.Name;
            Submit.BackColor = Color.Green;
            Submit.Location = new Point(textBox.Location.X + textBox.Width / 2 - Submit.Width / 2, textBox.Bottom + 20);
            this.Controls.Add(Submit);
            Submit.Hide();
            mainAppUI.welcomeScreen.submit = Submit;

            // button TryAgain
            TryAgain = new Button();
            TryAgain.Width = 150; TryAgain.Height = 30;
            TryAgain.Name = "Try login Again";
            TryAgain.Text = TryAgain.Name;
            TryAgain.BackColor = Color.Yellow;
            TryAgain.Location = new Point(textBox.Location.X + textBox.Width / 2 - TryAgain.Width / 2, Submit.Bottom + 20);
            this.Controls.Add(TryAgain);
            TryAgain.Hide();
            mainAppUI.welcomeScreen.TryAgain = TryAgain;


        }
        void DrawDubb(Graphics g)
        {
            Graphics g2 = Graphics.FromImage(bitmap);
            Draw(g2);
            g.DrawImage(bitmap, 0, 0);
        }
        void Draw(Graphics g2)
        {
            g2.Clear(Color.White);
            if (mainAppUI != null)
                mainAppUI.Draw(g2);
        }
        void OnPaint(object sender, PaintEventArgs e)
        {
            mainAppUI.SetDimensions(ClientSize.Width, ClientSize.Height);
            DrawDubb(e.Graphics);
        }
        void time(object sender, EventArgs e)
        {
            mainAppUI.ScreenTransitionHandler();
            DrawDubb(CreateGraphics());
        }
        private void ReceiveMessages()
        {
            try
            {
                while (true)
                {
                    string message = socketClient.ReceiveMessage();
                    if (!string.IsNullOrEmpty(message))
                    {
                        Console.WriteLine("Received from server: " + message);
                        mainAppUI.SetServerMessage(message);
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in receiving thread: " + ex.Message);
            }
        }
        private void ReceiveMessagesYOLO()
        {
            try
            {
                while (true)
                {
                    string message = YOLOSocketClient.ReceiveMessage();
                    if (!string.IsNullOrEmpty(message))
                    {
                        Console.WriteLine("Received from YOLO server: " + message);
                        mainAppUI.SetYOLOServerMessage(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in receiving thread: " + ex.Message);
            }
        }
        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (receiveThread != null && receiveThread.IsAlive)
            {
                receiveThread.Abort();
            }
            socketClient.SendMessage("$EmotionAnalysisStop$");
            socketClient.SendMessage("$GestureRecognitionStop$");
            if(videoPlayer != null)
            {
                videoPlayer.StopVideo();
            }
            if(mainAppUI != null)
            {
                if(mainAppUI.markerHandler != null)
                {
                    mainAppUI.markerHandler.StopMarkerRecognition();
                }
            }
        }
    }
}
