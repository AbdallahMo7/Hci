using System;
using System.Drawing;
using System.Windows.Forms.VisualStyles;

namespace WindowsFormsApp1.Components
{
    internal class AirMouse
    {
        private int Width;
        private int Height;
        public int Xp;
        public int Yp;
        public int Xt;
        public int Yt;
        public int radius = 35;

        public int updateTime;

        public AirMouse(int width, int height)
        {
            SetDimensions(width, height);
        }

        public void SetDimensions(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public void UpdateMousePosition(int xp, int yp, int xt, int yt, int updateTime)
        {
            Xp = xp;
            Yp = yp;
            Xt = xt;
            Yt = yt;
            this.updateTime = updateTime;
        }

        public void Draw(Graphics g)
        {
            if (g == null) throw new ArgumentNullException(nameof(g));

            const float opacity = 0.5f;

            // Brushes with opacity
            Brush yellowBrush = new SolidBrush(Color.FromArgb((int)(opacity * 255), Color.Yellow));
            Brush blueBrush = new SolidBrush(Color.FromArgb((int)(opacity * 255), Color.Blue));
            Brush redBrush = new SolidBrush(Color.FromArgb((int)(opacity * 255), Color.Red));

            // Calculate distance between two points
            double distance = Math.Sqrt(Math.Pow(Xt - Xp, 2) + Math.Pow(Yt - Yp, 2));
            bool isIntersecting = distance <= radius * 2;

            // Choose brush colors based on overlap
            Brush circlePBrush = isIntersecting ? redBrush : yellowBrush;
            Brush circleTBrush = isIntersecting ? redBrush : blueBrush;

            // Draw the circles
            g.FillEllipse(circlePBrush, Xp - radius, Yp - radius, radius * 2, radius * 2);
            g.FillEllipse(circleTBrush, Xt - radius, Yt - radius, radius * 2, radius * 2);

            

            // Dispose of brushes to release resources
            yellowBrush.Dispose();
            blueBrush.Dispose();
            redBrush.Dispose();
        }

        // check if the two circles are intersecting
        public bool IsIntersecting()
        {
            float dx = Xp - Xt;
            float dy = Yp - Yt;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);
            return distance <= radius * 2;
        }

        public void ProcessServerMessage(string serverMessage, int timeCounter)
        {
            // Strip the $AirMouse$ prefix
            string data = serverMessage.Substring(10).Trim();

            data = data.Split('$')[0];

            // Validate the format: should contain exactly one set of brackets
            if (data.StartsWith("[") && data.EndsWith("]"))
            {
                int pointerTipIndex = data.IndexOf("'pointer_tip':");
                int thumbTipIndex = data.IndexOf("'thumb_tip':");

                if (pointerTipIndex != -1 && thumbTipIndex != -1)
                {
                    // Extract the first pointer_tip x and y
                    string pointerTipData = data.Substring(pointerTipIndex + 14); // Skip "'pointer_tip':"
                    int pointerXStart = pointerTipData.IndexOf("'x':") + 4;
                    int pointerYStart = pointerTipData.IndexOf("'y':");

                    if (pointerXStart > 3 && pointerYStart > pointerXStart)
                    {
                        string pointerXString = pointerTipData.Substring(pointerXStart, pointerYStart - pointerXStart).Trim(',', ' ', '}');
                        string pointerYString = pointerTipData.Substring(pointerYStart + 4).Split(',')[0].Trim(' ', '}');

                        if (float.TryParse(pointerXString, out float pointerX) &&
                            float.TryParse(pointerYString, out float pointerY))
                        {
                            // Extract the first thumb_tip x and y
                            string thumbTipData = data.Substring(thumbTipIndex + 12); // Skip "'thumb_tip':"
                            int thumbXStart = thumbTipData.IndexOf("'x':") + 4;
                            int thumbYStart = thumbTipData.IndexOf("'y':");

                            if (thumbXStart > 3 && thumbYStart > thumbXStart)
                            {
                                string thumbXString = thumbTipData.Substring(thumbXStart, thumbYStart - thumbXStart).Trim(',', ' ', '}');
                                string thumbYString = thumbTipData.Substring(thumbYStart + 4).Split(',')[0].Trim(' ', '}');

                                if (float.TryParse(thumbXString, out float thumbX) &&
                                    float.TryParse(thumbYString, out float thumbY))
                                {
                                    // Renormalize the values
                                    int normalizedPointerX = (int)(pointerX * Width*1.2f);
                                    int normalizedPointerY = (int)(pointerY * Height*1.2f);
                                    int normalizedThumbX = (int)(thumbX * Width * 1.2f);
                                    int normalizedThumbY = (int)(thumbY * Height * 1.2f);

                                    // Update the air mouse position
                                    UpdateMousePosition(normalizedPointerX, normalizedPointerY, normalizedThumbX, normalizedThumbY, timeCounter);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
