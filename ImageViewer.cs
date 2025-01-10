using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Components
{
    internal class ImageViewer
    {
        private int Width, Height;
        private int x, y;
        private Bitmap bitmap;
        private int MyWidth, MyHeight;
        private Rectangle Src, Dist, Frame;
        public ImageViewer(Bitmap bitmap, int width, int height)
        {
            this.bitmap = bitmap;
            SetDimensions(width, height);
        }
        public void SetDimensions(int width, int height)
        {
            this.Width = width;
            this.Height = height;

            MyWidth = width / 7 * 4;
            MyHeight = height / 7 * 4;
            x = width / 7 * 3 - 20;
            y = height / 7;
            Src = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            Dist = new Rectangle(x, y, MyWidth, MyHeight);
            Frame = new Rectangle(x - 10, y - 10, MyWidth + 20, MyHeight + 20);

        }

        public void Draw(Graphics g)
        {
            g.FillRectangle(Brushes.White, Frame);
            g.DrawImage(bitmap, Dist, Src, GraphicsUnit.Pixel);
        }
    }
}
