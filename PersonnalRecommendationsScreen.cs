using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1.Screens
{
    internal class PersonnalRecommendationsScreen // responsible for login, register, and personal recommendations
    {
        
        private int Width, Height;
        public OptionsPannel optionsPannel;
        private Bitmap background;
        private Rectangle backgroundSrc, backgroundDest;
        public bool profile = false, recomm = false, activateGestureRecognition;
        public string maxExibit;
        SessionTracker sessionTracker;

        public Rectangle BackButtonSrc, BackButtonDist;
        public Bitmap BackButtonBitmap;

        public PersonnalRecommendationsScreen(int width, int height, SocketClient socketClient)
        {
            background = new Bitmap("images/background.jpg");
            optionsPannel = new OptionsPannel(Width, Height);
            optionsPannel.Options.AddRange(new[] { "Explore Recommendations","Profile","Back" });
            optionsPannel.Bitmaps.Add(new Bitmap("images/optionspannel/personnalrecommendationsscreen/Personalized Recommendations.jpg"));
            optionsPannel.Bitmaps.Add(new Bitmap("images/optionspannel/personnalrecommendationsscreen/Profile.jpg"));
            optionsPannel.Bitmaps.Add(new Bitmap("images/optionspannel/back.png"));
            SetDimensions(width, height);

            BackButtonBitmap = new Bitmap("images/optionspannel/back.png");
            BackButtonSrc = new Rectangle(0, 0, BackButtonBitmap.Width, BackButtonBitmap.Height);
        }
        public bool IsCircleIntersectingRectangle(Point circleCenter, int circleRadius)
        {
            int closestX = Math.Max(BackButtonDist.Left, Math.Min(circleCenter.X, BackButtonDist.Right));
            int closestY = Math.Max(BackButtonDist.Top, Math.Min(circleCenter.Y, BackButtonDist.Bottom));

            int distanceX = circleCenter.X - closestX;
            int distanceY = circleCenter.Y - closestY;

            return (distanceX * distanceX + distanceY * distanceY) <= (circleRadius * circleRadius);
        }
        public void SetDimensions(int width, int height)
        {
            Width = width;
            Height = height;
            backgroundSrc = new Rectangle(0, 0, background.Width, background.Height);
            backgroundDest = new Rectangle(0, 0, Width, Height);
            if (optionsPannel != null)
                optionsPannel.setDimentions(Width, Height);

            BackButtonDist = new Rectangle(Width - 400, 100, 100, 100);
        }
        public void Draw(Graphics g)
        {
            g.DrawImage(background, backgroundDest, backgroundSrc, GraphicsUnit.Pixel);
            g.DrawString("Personnal Recommendations Screen", new Font("Arial", 12), Brushes.Black, 10, 10);
            if (optionsPannel != null && !profile && !recomm)
            {
                optionsPannel.Draw(g);
            }
            else if (profile)
            {
                g.DrawString("Profile", new Font("Arial", 12), Brushes.Black, 10, 25);
                if (sessionTracker != null)
                {
                    int fontSize = 20, gap = 30;
                    float startY = -1.0f;
                    string mytext = $"User Name: {sessionTracker.UserName}";
                    SizeF myTesxtLengh = g.MeasureString(mytext, new Font("Arial", fontSize));
                    startY = Height / 2 - myTesxtLengh.Height / 2 - (gap * sessionTracker.VisitedExibits.Count + 1) / 2;
                    g.FillRectangle(Brushes.White, Width / 6 - 5, (startY - gap) + 5, Width / 6 * 4 , myTesxtLengh.Height / 2 + (gap * sessionTracker.VisitedExibits.Count) + gap * 2);
                    g.FillRectangle(Brushes.PaleGoldenrod, Width / 6, startY - gap, Width / 6 * 4, myTesxtLengh.Height / 2 + (gap * sessionTracker.VisitedExibits.Count) + gap*2);
                    g.DrawRectangle(Pens.Black, Width / 6, startY - gap, Width / 6 * 4, myTesxtLengh.Height / 2 + (gap * sessionTracker.VisitedExibits.Count) + gap * 2);
                    g.DrawString(mytext, new Font("Arial", fontSize), Brushes.White, Width / 2 - myTesxtLengh.Width / 2, startY - gap);
                    g.DrawString(mytext, new Font("Arial", fontSize), Brushes.Black, Width / 2 - myTesxtLengh.Width / 2 - 2, startY + 2 - gap);


                    for (int i = 0; i < sessionTracker.VisitedExibits.Count; i++)
                    {
                        mytext = $"{i + 1}. Exibit Name: {sessionTracker.VisitedExibits[i]}, Time spent in it: {(float)sessionTracker.VisitedExibitsTime[i] / 20 / 20} Minutes";
                        myTesxtLengh = g.MeasureString(mytext, new Font("Arial", fontSize));
                        g.DrawString(mytext, new Font("Arial", fontSize), Brushes.White, Width/2 - myTesxtLengh.Width / 2, startY + (i + 1) * gap);
                        g.DrawString(mytext, new Font("Arial", fontSize), Brushes.Black, Width / 2 - myTesxtLengh.Width / 2 - 2, startY + (i + 1) * gap + 2);

                    }
                }
            }
            else if (recomm)
            {
                g.DrawString("Recommendations based on your history", new Font("Arial", 12), Brushes.Black, 10, 25);
                if (sessionTracker != null)
                {
                    int fontSize = 20, gap = 30;
                    float startY = -1.0f;
                    string mytext = $"User Name: {sessionTracker.UserName}";
                    SizeF myTesxtLengh = g.MeasureString(mytext, new Font("Arial", fontSize));
                    startY = Height / 2 - myTesxtLengh.Height / 2 - (gap * 2) / 2;
                    g.FillRectangle(Brushes.White, Width / 8 - 5, (startY - gap) + 5, Width / 8 * 6, myTesxtLengh.Height / 2 + (gap * sessionTracker.VisitedExibits.Count) + gap * 5);
                    g.FillRectangle(Brushes.PaleGoldenrod, Width / 8, startY - gap, Width / 8 * 6, myTesxtLengh.Height / 2 + (gap * sessionTracker.VisitedExibits.Count) + gap * 5);
                    g.DrawRectangle(Pens.Black, Width / 8, startY - gap, Width / 8 * 6, myTesxtLengh.Height / 2 + (gap * sessionTracker.VisitedExibits.Count) + gap * 5);

                    float max = -1.0f;
                    maxExibit = "";
                    for (int i = 0; i < sessionTracker.VisitedExibits.Count; i++)
                    {
                        if (sessionTracker.VisitedExibitsTime[i] > max)
                        {
                            max = sessionTracker.VisitedExibitsTime[i];
                            maxExibit = sessionTracker.VisitedExibits[i];
                        }
                    }

                    mytext = $"We Recommend you to visit {maxExibit} Exibit, As you spent much time Watching it.";
                    myTesxtLengh = g.MeasureString(mytext, new Font("Arial", fontSize));
                    g.DrawString(mytext, new Font("Arial", fontSize), Brushes.White, Width / 2 - myTesxtLengh.Width / 2, startY +  gap);
                    g.DrawString(mytext, new Font("Arial", fontSize), Brushes.Black, Width / 2 - myTesxtLengh.Width / 2 - 2, startY + gap + 2);
                    mytext = $"Do the ok sign with your hand if you want to navigate to it";
                    myTesxtLengh = g.MeasureString(mytext, new Font("Arial", fontSize));
                    g.DrawString(mytext, new Font("Arial", fontSize), Brushes.White, Width / 2 - myTesxtLengh.Width / 2, startY + gap * 4);
                    g.DrawString(mytext, new Font("Arial", fontSize), Brushes.Black, Width / 2 - myTesxtLengh.Width / 2 - 2, startY + gap * 4 + 2);

                }
            }

            if (recomm || profile) 
            {
                g.DrawImage(BackButtonBitmap, BackButtonDist, BackButtonSrc, GraphicsUnit.Pixel);
            }
        }
        public void HandleMenuSelection(SessionTracker session)
        {
            sessionTracker = session;
            if(optionsPannel.selectedOption)
            {
                optionsPannel.selectedOption = false;
                if(optionsPannel.selectedIndex == 0)
                {
                    recomm = true;
                    profile = false;
                    activateGestureRecognition = true;
                }
                else if(optionsPannel.selectedIndex == 1)
                {
                    profile = true;
                    recomm = false;
                }
                else
                {
                    profile = false;
                    recomm = false;
                    activateGestureRecognition = false;
                }
            }

        }
    }
}
