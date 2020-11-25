using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml.Media;

namespace System.Graphics.SharpDX
{
    public static class GraphicsExtensions
    {
        public static PointF AsEWPoint(this PointerPoint point, float scale = 1)
        {
            var position = point.Position;
            return new PointF((float)position.X * scale, (float)position.Y * scale);
        }

        public static global::Windows.UI.Color AsColor(this Color color)
        {
            if (color != null)
            {
                var uiColor = new global::Windows.UI.Color
                {
                    R = (byte) (255*color.Red),
                    G = (byte) (255*color.Green), 
                    B = (byte) (255*color.Blue), 
                    A = (byte) (255*color.Alpha)
                };
                return uiColor;
            }

            return global::Windows.UI.Colors.Black;           
        }

        public static Brush AsBrush(this Color color)
        {
            if (color != null)
            {
                var uiColor = color.AsColor();
                var brush = new SolidColorBrush(uiColor);
                return brush;
            }

            return null;
        }
    }
}
