using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp1.Components;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    internal class WelcomeScreen
    {
        SocketClient socketClient;
        public FaceIDHandler faceIDHandler;
        public SessionTracker sessionTracker;
        public TextBox textBox;
        public Button submit;
        public Button TryAgain;
        string UserMessage = "";
        public bool loggedIn = false, loggingin = false, registering = false;


        private int Width, Height;
        public OptionsPannel optionsPannel;
        private Bitmap background;
        private Rectangle backgroundSrc, backgroundDest;

        public WelcomeScreen(int width, int height)
        {
            background = new Bitmap("images/background.jpg");
            optionsPannel = new OptionsPannel(Width, Height);
            optionsPannel.Options.AddRange(new[] { "Exhibits", "Personalized Recommendations", "About the Museum", "Exit" });
            optionsPannel.Bitmaps.Add(new Bitmap("images/optionspannel/welcomescreen/Exhibits.jpg"));
            optionsPannel.Bitmaps.Add(new Bitmap("images/optionspannel/welcomescreen/Personalized Recommendations.png"));
            optionsPannel.Bitmaps.Add(new Bitmap("images/optionspannel/welcomescreen/About the Museum.png"));
            optionsPannel.Bitmaps.Add(new Bitmap("images/optionspannel/welcomescreen/Exit.jpg"));
            SetDimensions(width, height);
            faceIDHandler = new FaceIDHandler(this.socketClient);

        }
        public void Start()
        {
            if (!loggingin && !loggedIn && !registering)
            {
                faceIDHandler.Login();
                loggingin = true;
            }
        }
        public void HandleLogin(string Message)
        {
            if (Message.StartsWith("$FaceIdentification$Login$"))
            {
                string DecodedMessage = faceIDHandler.DecodeMessage(Message);
                if (DecodedMessage.StartsWith("Failed"))
                {
                    loggedIn = false;
                    loggingin = false;

                    registering = true;
                    textBox.Show();
                    submit.Show();
                    TryAgain.Show();
                    submit.Click += new EventHandler(Submit_Click);
                    TryAgain.Click += new EventHandler(TryAgain_Click);
                }
                else
                {
                    Console.WriteLine(DecodedMessage);
                    loggingin = false;
                    loggedIn = true;
                    sessionTracker = new SessionTracker(DecodedMessage);
                    UserMessage = "";
                }
            }
        }
        public void HandleRegester(string message)
        {
            if (message.StartsWith("$FaceIdentification$Register$"))
            {
                string DecodedMessage = faceIDHandler.DecodeMessage(message);
                if (DecodedMessage == "Ok")
                {
                    textBox.Text = "";
                    textBox.Hide();
                    submit.Hide();
                    TryAgain.Hide();
                    UserMessage = "";
                    END();
                    Start();
                }
                Console.WriteLine(DecodedMessage);
                if (DecodedMessage.StartsWith("Failed"))
                {
                    string[] parts = DecodedMessage.Split('$');
                    if (parts[1] == "User name already exists")
                    {
                        UserMessage = "Please use a different user name as the User name already exists";
                    }
                    else if (parts[1] == "We have similar face data please login")
                    {
                        textBox.Hide();
                        submit.Hide();
                        TryAgain.Hide();
                        END();
                        Start();
                    }
                    else
                    {
                        faceIDHandler.Register(textBox.Text);
                    }
                }
            }
        }
        public void Submit_Click(object sender, EventArgs e)
        {
            faceIDHandler.Register(textBox.Text);

        }
        public void TryAgain_Click(object sender, EventArgs e)
        {
            textBox.Hide();
            submit.Hide();
            TryAgain.Hide();
            END();
            Start();
        }
        public void SetDimensions(int width, int height)
        {
            Width = width;
            Height = height;
            backgroundSrc = new Rectangle(0, 0, background.Width, background.Height);
            backgroundDest = new Rectangle(0, 0, Width, Height);
            if (optionsPannel != null)
                optionsPannel.setDimentions(Width, Height);
        }
        public void Draw(Graphics g)
        {
            //g.Clear(Color.LightSeaGreen);
            g.DrawImage(background, backgroundDest, backgroundSrc, GraphicsUnit.Pixel);
            g.DrawString("Welcome to the museum!", new Font("Arial", 12), Brushes.Black, 10, 10);
            
            if (loggingin && !loggedIn && !registering)
            {
                g.DrawString("Logging in... please face the Camera", new Font("Arial", 12), Brushes.Black, 10, 25);
            }
            else if (loggedIn)
            {
                g.DrawString($"Welcome {sessionTracker.UserName}", new Font("Arial", 12), Brushes.Green, 10, 25);
                if (optionsPannel != null)
                {
                    optionsPannel.Draw(g);
                }
            }
            else if (registering)
            {
                g.DrawString("Registering... please face the Camera", new Font("Arial", 12), Brushes.Black, 10, 25);
            }
            g.DrawString(UserMessage, new Font("Arial", 18), Brushes.Red, 10, 50);
        }

        public void END()
        {
            loggedIn = false;
            loggingin = false;
            registering = false;
            sessionTracker = null;
        }

    }
}
