using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes2
{
    public class Ellipse : IShape
    {
        public PathF PathForBounds(Graphics.Rectangle rect, float density = 1)
        {
            var path = new PathF();

            path.AppendEllipse(rect);

            return path;
        }
    }
}