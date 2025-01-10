using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Components
{
    internal class VerticalGallery
    {
        private int width, height;
        public List<string> Options { get; private set; } = new List<string>();
        public List<Bitmap> Bitmaps { get; private set; } = new List<Bitmap>();


        public int selectedIndex = 0;
        public bool selectedOption = false;

        private Rectangle upRectangle;
        private Rectangle downRectangle;
        private Rectangle selectedRectangle;

        public VerticalGallery(int width, int height)
        {
            this.width = width;
            this.height = height;
            RecalculateRectangles();
        }

        public void SetDimensions(int width, int height)
        {
            this.width = width;
            this.height = height;
            RecalculateRectangles();
        }

        private void RecalculateRectangles()
        {
            int selectedHeight = height / 3;
            int unselectedHeight = (selectedHeight / 3) * 2;

            int upY = height / 2 - selectedHeight / 2 - unselectedHeight - 10;
            int downY = height / 2 + selectedHeight / 2 + 10;
            int selectedY = height / 2 - selectedHeight / 2;

            upRectangle = new Rectangle(width / 5 - width / 8, upY, width / 4, unselectedHeight);
            downRectangle = new Rectangle(width / 5 - width / 8, downY, width / 4, unselectedHeight);
            selectedRectangle = new Rectangle(width / 5 - width / 6, selectedY, width / 3, selectedHeight);
        }

        public void Draw(Graphics g)
        {
            if (Options.Count == 0 || Bitmaps.Count == 0)
                return;

            // Draw rectangles and images for up, down, and selected
            g.FillRectangle(Brushes.LightGray, upRectangle);
            g.FillRectangle(Brushes.Gray, downRectangle);
            g.FillRectangle(Brushes.White, selectedRectangle);

            int upIndex = selectedIndex > 0 ? selectedIndex - 1 : Options.Count - 1;
            int downIndex = selectedIndex < Options.Count - 1 ? selectedIndex + 1 : 0;

            g.DrawImage(Bitmaps[upIndex], upRectangle.X + 10, upRectangle.Y + 10, upRectangle.Width - 20, upRectangle.Height - 20);
            g.DrawImage(Bitmaps[selectedIndex], selectedRectangle.X + 10, selectedRectangle.Y + 10, selectedRectangle.Width - 20, selectedRectangle.Height - 20);
            g.DrawImage(Bitmaps[downIndex], downRectangle.X + 10, downRectangle.Y + 10, downRectangle.Width - 20, downRectangle.Height - 20);
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
            bool hitUp = IsCircleIntersectingRectangle(circleCenter, circleRadius, upRectangle);
            bool hitDown = IsCircleIntersectingRectangle(circleCenter, circleRadius, downRectangle);

            if (hitMiddle)
            {
                selectedOption = true;
            }
            else if (hitUp)
            {
                ChangeSelection(-1);
            }
            else if (hitDown)
            {
                ChangeSelection(1);
            }

            return hitMiddle || hitUp || hitDown;
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
