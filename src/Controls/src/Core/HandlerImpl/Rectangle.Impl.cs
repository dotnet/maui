using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes2
{
	public class Rectangle : IShape
    {
        public Rectangle()
        {

        }

        public Rectangle(CornerRadius cornerRadius) : this()
        {
            CornerRadius = cornerRadius;
        }

        public CornerRadius CornerRadius { get; set; }

        public PathF PathForBounds(Graphics.Rectangle rect, float density = 1)
        {
            var path = new PathF();

            path.AppendRoundedRectangle(
                rect,
                (float)CornerRadius.TopLeft,
                (float)CornerRadius.TopRight,
                (float)CornerRadius.BottomLeft,
                (float)CornerRadius.BottomRight);

            return path;
        }
    }
}