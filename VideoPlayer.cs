using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AxWMPLib;


namespace WindowsFormsApp1.Components
{
    internal class VideoPlayer
    {
        private AxWindowsMediaPlayer axPlayer;
        private int x, y, width, height;
        private int MyWidth, MyHeight;
        public bool Showing = false;
        public Rectangle BackButtonSrc, BackButtonDist;
        public Bitmap BackButtonBitmap;

        Form form;
        public VideoPlayer(Form form)
        {
            this.form = form;
            axPlayer = new AxWindowsMediaPlayer();

            // Add the control to the form
            form.Controls.Add(axPlayer);

            // Register the ActiveX control properly
            axPlayer.BeginInit();
            axPlayer.EndInit();
            SetDimensions(form.ClientSize.Width, form.ClientSize.Height);

            // Force the control to create and render
            axPlayer.CreateControl();
            Hide();

            BackButtonBitmap = new Bitmap("images/optionspannel/back.png");
            BackButtonSrc = new Rectangle(0, 0, BackButtonBitmap.Width, BackButtonBitmap.Height);
        }

        public void Draw(Graphics g)
        {
            if (Showing)
            {
                g.DrawImage(BackButtonBitmap, BackButtonDist, BackButtonSrc, GraphicsUnit.Pixel);
            }
        }
        public void Hide()
        {
            axPlayer.Visible = false;
            Showing = false;
        }

        public void Show()
        {
            axPlayer.Visible = true;
            Showing = true;
        }

        public void PlayVideo(string videoPath)
        {
            if (!System.IO.File.Exists(videoPath))
            {
                MessageBox.Show($"File not found: {videoPath}");
                return;
            }
            // Set the video URL and play
            axPlayer.URL = videoPath;
            axPlayer.Ctlcontrols.play();
            Show();
        }

        public void StopVideo()
        {
            axPlayer.Ctlcontrols.stop();
            Hide();
        }

        public void SetDimensions(int width, int height)
        {
            this.width = width;
            this.height = height;

            MyWidth = width / 6 * 4;
            MyHeight = height / 6 * 4;
            x = width / 6;
            y = height / 6;

            axPlayer.Location = new System.Drawing.Point(x, y);  // Set position (X, Y)
            axPlayer.Size = new System.Drawing.Size(MyWidth, MyHeight); // Set size (Width, Height)

            BackButtonDist = new Rectangle(width - 100, 100, 100, 100);
        }

        public bool IsCircleIntersectingRectangle(Point circleCenter, int circleRadius)
        {
            int closestX = Math.Max(BackButtonDist.Left, Math.Min(circleCenter.X, BackButtonDist.Right));
            int closestY = Math.Max(BackButtonDist.Top, Math.Min(circleCenter.Y, BackButtonDist.Bottom));

            int distanceX = circleCenter.X - closestX;
            int distanceY = circleCenter.Y - closestY;

            return (distanceX * distanceX + distanceY * distanceY) <= (circleRadius * circleRadius);
        }
    }

}
