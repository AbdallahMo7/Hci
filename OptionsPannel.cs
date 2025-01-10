using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace WindowsFormsApp1
{
    internal class OptionsPannel
    {
        private int width, height;
        public List<string> Options { get; private set; } = new List<string>();
        public List<Bitmap> Bitmaps { get; private set; } = new List<Bitmap>();

        public int selectedIndex = 0;
        public bool selectedOption = false;

        private Rectangle previousRectangle;
        private Rectangle nextRectangle;
        private Rectangle selectedRectangle;

        public OptionsPannel(int width, int height)
        {
            this.width = width;
            this.height = height;
            RecalculateRectangles();
        }

        public void setDimentions(int width, int height)
        {
            this.width = width;
            this.height = height;
            RecalculateRectangles();
        }

        private void RecalculateRectangles()
        {
            int selectedWidth = width / 3, selectedHeight = height / 3;
            int unselectedWidth = (selectedWidth / 3) * 2, unselectedHeight = (selectedHeight / 3) * 2;

            int previousX = width / 2 - selectedWidth / 2 - unselectedWidth - 10;
            int nextX = width / 2 + selectedWidth / 2 + 10;
            int selectedX = width / 2 - selectedWidth / 2;

            int previousY = height / 2 - unselectedHeight / 2;
            int nextY = height / 2 - unselectedHeight / 2;
            int selectedY = height / 2 - selectedHeight / 2;

            previousRectangle = new Rectangle(previousX, previousY, unselectedWidth, unselectedHeight);
            nextRectangle = new Rectangle(nextX, nextY, unselectedWidth, unselectedHeight);
            selectedRectangle = new Rectangle(selectedX, selectedY, selectedWidth, selectedHeight);
        }

        public void Draw(Graphics g)
        {
            if (Options.Count == 0 || Bitmaps.Count == 0)
            {
                return;
            }

            g.FillRectangle(Brushes.Gold, previousRectangle.X - 10, previousRectangle.Y + 10, previousRectangle.Width, previousRectangle.Height);
            g.FillRectangle(Brushes.Gold, nextRectangle.X - 10, nextRectangle.Y + 10, nextRectangle.Width, nextRectangle.Height);
            g.FillRectangle(Brushes.Gold, selectedRectangle.X - 10, selectedRectangle.Y + 10, selectedRectangle.Width, selectedRectangle.Height);

            g.FillRectangle(Brushes.Beige, previousRectangle);
            g.FillRectangle(Brushes.Beige, nextRectangle);
            g.FillRectangle(Brushes.Beige, selectedRectangle);
            g.DrawRectangle(Pens.Black, previousRectangle);
            g.DrawRectangle(Pens.Black, nextRectangle);
            g.DrawRectangle(Pens.Black, selectedRectangle);

            int previous = selectedIndex > 0 ? selectedIndex - 1 : Options.Count - 1;
            int next = selectedIndex < Options.Count - 1 ? selectedIndex + 1 : 0;

            g.DrawImage(Bitmaps[previous], previousRectangle.X + 20, previousRectangle.Y + 20, previousRectangle.Width - 40, previousRectangle.Height - 40);
            g.DrawImage(Bitmaps[next], nextRectangle.X + 20, nextRectangle.Y + 20, nextRectangle.Width - 40, nextRectangle.Height - 40);
            g.DrawImage(Bitmaps[selectedIndex], selectedRectangle.X + 20, selectedRectangle.Y + 20, selectedRectangle.Width - 40, selectedRectangle.Height - 40);

            Font font = new Font("Arial", 12);
            SizeF previousLen = g.MeasureString(Options[previous], font);
            SizeF nextLen = g.MeasureString(Options[next], font);
            SizeF selectedLen = g.MeasureString(Options[selectedIndex], font);

            g.DrawString(Options[previous], font, Brushes.Black, previousRectangle.X + previousRectangle.Width / 2 - previousLen.Width / 2, previousRectangle.Y + previousRectangle.Height - previousLen.Height);
            g.DrawString(Options[next], font, Brushes.Black, nextRectangle.X + nextRectangle.Width / 2 - nextLen.Width / 2, nextRectangle.Y + nextRectangle.Height - nextLen.Height);
            g.DrawString(Options[selectedIndex], font, Brushes.Black, selectedRectangle.X + selectedRectangle.Width / 2 - selectedLen.Width / 2, selectedRectangle.Y + selectedRectangle.Height - selectedLen.Height);
            
            font.Dispose();
        }

        public void ChangeSelection(int direction)
        {
            if (direction == 1)
            {
                selectedIndex = (selectedIndex < Options.Count - 1) ? selectedIndex + 1 : 0;
            }
            else
            {
                selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : Options.Count - 1;
            }
        }

        public bool Hit(Point circleCenter, int circleRadius)
        {
            bool hitMiddle = IsCircleIntersectingRectangle(circleCenter, circleRadius, selectedRectangle);
            bool hitLeft = IsCircleIntersectingRectangle(circleCenter, circleRadius, previousRectangle);
            bool hitRight = IsCircleIntersectingRectangle(circleCenter, circleRadius, nextRectangle);

            if (hitMiddle)
            {
                selectedOption = true;
            }
            else if (hitLeft)
            {
                ChangeSelection(-1);
            }
            else if (hitRight)
            {
                ChangeSelection(1);
            }

            return hitMiddle || hitLeft || hitRight;
        }

        private bool IsCircleIntersectingRectangle(Point circleCenter, int circleRadius, Rectangle rect)
        {
            int closestX = Math.Max(rect.Left, Math.Min(circleCenter.X, rect.Right));
            int closestY = Math.Max(rect.Top, Math.Min(circleCenter.Y, rect.Bottom));

            int distanceX = circleCenter.X - closestX;
            int distanceY = circleCenter.Y - closestY;

            return (distanceX * distanceX + distanceY * distanceY) <= (circleRadius * circleRadius);
        }
    }
}
