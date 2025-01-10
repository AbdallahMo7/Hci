using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    internal class IdleScreen
    {
        private int Width, Height;
        private Bitmap background;
        private Rectangle backgroundSrc, backgroundDest;

        public IdleScreen(int width, int height)
        {
            background = new Bitmap("images/background.jpg");
            SetDimensions(width, height);
        }
        public void SetDimensions(int width, int height)
        {
            Width = width;
            Height = height;
            backgroundSrc = new Rectangle(0, 0, background.Width, background.Height);
            backgroundDest = new Rectangle(0, 0, Width, Height);
        }
        public void Draw(Graphics g)
        {
            if (g == null) throw new ArgumentNullException(nameof(g), "Graphics object is null!");
            if (Width <= 0 || Height <= 0) return; // Avoid invalid dimensions

            try
            {
                g.DrawImage(background, backgroundDest, backgroundSrc, GraphicsUnit.Pixel);

                string text = "Welcome to the museum!";
                Font font;

                try
                {
                    font = new Font("Arial", 70);
                }
                catch (ArgumentException)
                {
                    font = new Font(FontFamily.GenericSansSerif, 70); // Fallback font
                }

                SizeF size = g.MeasureString(text, font);

                // Shadow effect
                g.DrawString(text, font, Brushes.Black, Width / 2 - size.Width / 2 - 5, Height / 2 - size.Height / 2 + 5);
                g.DrawString(text, font, Brushes.White, Width / 2 - size.Width / 2, Height / 2 - size.Height / 2);
                font.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in draw method: {ex.Message}");
            }
        }

    }
}
