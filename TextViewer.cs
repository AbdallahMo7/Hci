using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO; // Required for File operations
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Components
{
    internal class TextViewer
    {
        private int Width, Height;
        private int x, y;
        private Bitmap bitmap;
        private int MyWidth, MyHeight;
        private Rectangle Src, Dist, Frame, storyFrame;
        private string story; // String to store file content
        
        public TextViewer(Bitmap bitmap, string filePath, int width, int height)
        {
            this.bitmap = bitmap;
            SetDimensions(width, height);
            LoadStory(filePath);
        }

        private void LoadStory(string filePath)
        {
            try
            {
                story = File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                story = "Error loading text: " + ex.Message;
            }
        }

        public void SetDimensions(int width, int height)
        {
            this.Width = width;
            this.Height = height;

            MyWidth = width / 10 * 3;
            MyHeight = height / 7 * 5;
            x = width / 7 * 3 - 20;
            y = height / 7;
            storyFrame = new Rectangle(x, y, MyWidth, MyHeight);
            Src = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            Dist = new Rectangle(x, y, MyWidth, MyHeight/3);
            Frame = new Rectangle(x - 10, y - 10, MyWidth + 20, MyHeight + 20);
        }

        public void Draw(Graphics g)
        {
            // Draw the image
            g.FillRectangle(Brushes.White, storyFrame);
            g.FillRectangle(Brushes.White, Frame);
            g.DrawImage(bitmap, Dist, Src, GraphicsUnit.Pixel);

            // Draw the text below the image
            var textPosition = new Point(x, y + MyHeight/3 + 10);
            var textFont = new Font("Arial", 12, FontStyle.Regular);
            var textBrush = Brushes.Black;

            // Draw the story content with word wrapping
            RectangleF textRectangle = new RectangleF(textPosition.X, textPosition.Y, MyWidth, MyHeight);
            g.DrawString(story, textFont, textBrush, textRectangle);
        }
    }
}
